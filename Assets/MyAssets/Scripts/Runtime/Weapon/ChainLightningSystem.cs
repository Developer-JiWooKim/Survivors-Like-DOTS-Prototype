using Assets.MyAssets.Scripts.Runtime.Combat;
using Assets.MyAssets.Scripts.Runtime.Enemy;
using Assets.MyAssets.Scripts.Runtime.Player;
using Assets.MyAssets.Scripts.Runtime.Pooling;
using Assets.MyAssets.Scripts.Runtime.Run;
using Assets.MyAssets.Scripts.Runtime.Spatial;
using Assets.MyAssets.Scripts.Runtime.Terrain;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 전격 사슬 (기획서 5.1 의 4번, 규칙은 5.6). 쿨다운이 끝나면 한 번에:
    /// 1. 사거리 안 가장 가까운 적에게 낙뢰
    /// 2. 맞은 적에서 연쇄 거리 안의, **아직 안 맞은** 가장 가까운 적으로 튄다 — 예산이 남는 동안 반복
    /// 3. 타격마다 발밑 타일을 본다: 물·빙판이면 예산 절반만 소모(<see cref="ChainBudget"/>),
    ///    물이면 감전 요청(<see cref="TerrainEffects"/> → 물 덩어리 전체 flood fill)
    /// 4. 구간마다 낙뢰 선(<see cref="LightningBolt"/>)을 잠깐 띄운다
    ///
    /// 연쇄하는 동안 지형을 **타격마다 다시 본다** — 물가에서 물 밖으로 튀면 그때부터 마른 땅 비용이 든다.
    ///
    /// 왜 메인 스레드인가: 연쇄는 앞 타격의 결과(누구를 맞혔나)가 다음 탐색의 입력이라 본질적으로 순차다.
    /// 1.5 초에 한 번, 최대 수 타라 잡으로 나눌 이유가 없다. 타일 읽기·효과 큐·풀 꺼내기도 전부 메인 스레드다.
    ///
    /// 순서 이유는 <see cref="LobbedShellSystem"/> 과 같다 — 효과 큐를 비우기 전, 피해 적용 전,
    /// LocalTransform 을 쓰는 잡(적·투사체 이동) 전.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(PlayerMoveSystem))]
    [UpdateAfter(typeof(BuildEnemySpatialHashSystem))]
    [UpdateBefore(typeof(EnemyChaseSystem))]
    [UpdateBefore(typeof(ProjectileMoveSystem))]
    [UpdateBefore(typeof(TerrainEffectSystem))]
    [UpdateBefore(typeof(DamageApplySystem))]
    public partial struct ChainLightningSystem : ISystem
    {
        /// <summary>낙뢰 선 두께 (월드 유닛). 순수 연출값.</summary>
        private const float BoltThickness = 0.08f;

        private EntityQuery _weaponQuery;
        private EntityQuery _freeBoltQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // 쿼리를 직접 쓰는 이유는 OilBarrelFireSystem 과 같다 (enableable — 획득 전엔 돌지 않는다).
            _weaponQuery = SystemAPI.QueryBuilder()
                .WithAllRW<ChainLightningWeapon>()
                .WithAll<LocalTransform>()
                .Build();

            _freeBoltQuery = SystemAPI.QueryBuilder()
                .WithAll<LightningBolt>()
                .WithDisabled<Active>()
                .Build();

            state.RequireForUpdate(_weaponQuery);
            state.RequireForUpdate<EnemySpatialHash>();
            state.RequireForUpdate<DamageEventBus>();
            state.RequireForUpdate<TerrainEffects>();
            state.RequireForUpdate<TerrainGrid>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeArray<Entity> owners = _weaponQuery.ToEntityArray(Allocator.Temp);
            float deltaTime = SystemAPI.Time.DeltaTime;

            for (int i = 0; i < owners.Length; i++)
            {
                Entity owner = owners[i];
                ChainLightningWeapon weapon = SystemAPI.GetComponent<ChainLightningWeapon>(owner);

                weapon.Cooldown -= deltaTime;
                if (weapon.Cooldown <= 0f)
                {
                    float2 origin = SystemAPI.GetComponent<LocalTransform>(owner).Position.xy;

                    // 대상이 없으면 0 에 묶어 두고 다음 프레임에 다시 본다 (다른 무기와 같은 규칙).
                    weapon.Cooldown = Strike(ref state, origin, weapon) ? weapon.Interval : 0f;
                }

                SystemAPI.SetComponent(owner, weapon);
            }

            owners.Dispose();
        }

        /// <returns>한 번이라도 맞혔으면 true.</returns>
        private bool Strike(ref SystemState state, float2 origin, in ChainLightningWeapon weapon)
        {
            // 메인 스레드에서 해시를 읽으므로 이번 프레임 재구축 잡을 먼저 끝낸다.
            EnemySpatialHash hash = SystemAPI.GetSingleton<EnemySpatialHash>();
            hash.BuildHandle.Complete();

            // 맞은 적 목록. 연쇄 탐색의 제외 목록이기도 하다 — 두 적 사이를 왕복하지 않게.
            var struck = new FixedList128Bytes<Entity>();
            if (!SpatialQueries.TryFindNearestExcluding(hash.Map, origin, weapon.Range, ref struck, out AgentRef target))
            {
                return false;
            }

            // 발밑 타일을 메인 스레드에서 읽으므로, 그리드에 쓰는 확산 틱 잡이 끝나 있어야 한다.
            // 읽기만 하므로 렌더 잡(ReadersHandle)은 기다리지 않는다.
            TerrainGrid grid = SystemAPI.GetSingleton<TerrainGrid>();
            grid.WriteHandle.Complete();

            RefRW<TerrainEffects> effects = SystemAPI.GetSingletonRW<TerrainEffects>();

            // 피해는 1 칸짜리 스트림에 모아 버스에 넘긴다 (LobbedShellSystem 과 같은 방식). 해제는 DamageApplySystem 의 몫.
            var stream = new NativeStream(1, Allocator.TempJob);
            NativeStream.Writer writer = stream.AsWriter();
            writer.BeginForEachIndex(0);

            NativeArray<Entity> freeBolts = _freeBoltQuery.ToEntityArray(Allocator.Temp);
            int boltCursor = 0;

            var budget = new ChainBudget(weapon.ChainBudget);
            float2 from = origin;

            while (true)
            {
                struck.Add(target.Entity);
                writer.Write(new DamageEvent
                {
                    Target = target.Entity,
                    Amount = weapon.Damage,
                });

                int2 cell = grid.CellOf(target.Position);
                TileType tile = grid.TileAt(cell).TypeValue;

                if (ChainLightningRules.TriggersShock(tile))
                {
                    // 같은 웅덩이를 여러 번 때려도 감전은 시간을 덮어쓸 뿐이다 (TileStateRules.Shock).
                    effects.ValueRW.Request(cell, TerrainEffectKind.Shock);
                }

                budget.Spend(tile, weapon.WetChainCost);

                // 풀이 바닥나면 선만 안 그린다. 피해·감전은 연출과 무관하게 준다.
                if (boltCursor < freeBolts.Length)
                {
                    ShowBolt(state.EntityManager, freeBolts[boltCursor], from, target.Position, weapon.BoltDuration);
                    boltCursor++;
                }

                // 제외 목록이 가득 차면 더 튈 수 없다 (15 타 — 예산 3 · 젖은 칸 0.5 면 최대 6 타라 실제로는 닿지 않는다).
                if (!budget.CanStrike || struck.Length >= struck.Capacity)
                {
                    break;
                }

                from = target.Position;
                if (!SpatialQueries.TryFindNearestExcluding(hash.Map, from, weapon.ChainRange, ref struck, out target))
                {
                    break; // 튈 곳이 없다 — 남은 예산은 버린다.
                }
            }

            writer.EndForEachIndex();
            SystemAPI.GetSingletonRW<DamageEventBus>().ValueRW.Register(stream, default);

            freeBolts.Dispose();
            return true;
        }

        private static void ShowBolt(EntityManager entityManager, Entity bolt, float2 from, float2 to, float duration)
        {
            float2 delta = to - from;
            float length = math.length(delta);

            // 쿼드(1×1)를 구간 가운데에 놓고, 구간 방향으로 돌린 뒤, 길이 방향으로만 늘린다.
            // 프리팹의 Z 는 보존한다 (그리는 순서).
            LocalTransform transform = entityManager.GetComponentData<LocalTransform>(bolt);
            transform.Position = new float3((from + to) * 0.5f, transform.Position.z);
            transform.Rotation = quaternion.RotateZ(math.atan2(delta.y, delta.x));
            transform.Scale = 1f;
            entityManager.SetComponentData(bolt, transform);

            entityManager.SetComponentData(bolt, new PostTransformMatrix
            {
                Value = float4x4.Scale(math.max(length, 0.01f), BoltThickness, 1f),
            });

            entityManager.SetComponentData(bolt, new LightningBolt { Remaining = duration });
            PoolUtility.SetAlive(entityManager, bolt, true);
        }
    }
}
