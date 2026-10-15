using Assets.MyAssets.Scripts.Runtime.Combat;
using Assets.MyAssets.Scripts.Runtime.Enemy;
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
    /// 착탄형 탄(<see cref="LobbedShell"/>)을 포물선으로 움직이고, 착탄하면
    /// ① 반경 안 적에게 피해 ② 지형 효과 요청(기름 / 점화) 을 낸 뒤 풀로 돌려보낸다.
    ///
    /// 지형은 직접 고치지 않고 <see cref="TerrainEffects"/> 큐에 넣는다 (그 이유는 큐 주석 참조).
    /// 피해도 Health 를 직접 고치지 않고 <see cref="DamageEventBus"/> 에 스트림을 등록한다 — 피해 적용을 한 곳에 모으는 규칙.
    ///
    /// 왜 잡이 아니라 메인 스레드인가:
    /// 날아다니는 탄은 많아야 수 발(풀 32)이다. 잡을 띄우는 비용이 일보다 크다.
    /// 그리고 효과 큐를 비우는 <see cref="TerrainEffectSystem"/> 이 메인 스레드라, 여기서 잡으로 넣으면
    /// 그 잡이 끝나길 기다리는 핸들을 하나 더 관리해야 한다.
    ///
    /// 순서:
    /// - 발사 시스템 **뒤** — 이번 프레임에 쏜 탄도 바로 한 프레임 움직인다.
    /// - <see cref="TerrainEffectSystem"/> **앞** — 착탄한 프레임에 기름이 깔린다 (한 프레임 밀리지 않게).
    /// - <see cref="DamageApplySystem"/> **앞** — 등록한 피해 스트림이 같은 프레임에 적용된다.
    /// - 적 이동·투사체 이동 **앞** — LocalTransform 을 메인 스레드에서 쓰므로 그 잡들을 기다리지 않게.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(OilBarrelFireSystem))]
    [UpdateAfter(typeof(FireballFireSystem))]
    [UpdateBefore(typeof(TerrainEffectSystem))]
    [UpdateBefore(typeof(DamageApplySystem))]
    [UpdateBefore(typeof(EnemyChaseSystem))]
    [UpdateBefore(typeof(ProjectileMoveSystem))]
    public partial struct LobbedShellSystem : ISystem
    {
        /// <summary>
        /// 포물선 꼭대기의 화면상 높이 (월드 유닛). 탑다운이라 진짜 높이가 없어서 **화면 위쪽(+y)으로 띄워** 표현한다.
        /// 순수 연출값이라 판정에는 영향이 없다 — 착탄점은 항상 <see cref="LobbedShell.To"/> 다.
        /// </summary>
        private const float ArcHeight = 1.5f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LobbedShell>();
            state.RequireForUpdate<EnemySpatialHash>();
            state.RequireForUpdate<DamageEventBus>();
            state.RequireForUpdate<TerrainEffects>();
            state.RequireForUpdate<TerrainGrid>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            var landedShells = new NativeList<LobbedShell>(Allocator.Temp);
            var landedEntities = new NativeList<Entity>(Allocator.Temp);

            foreach ((RefRW<LobbedShell> shell, RefRW<LocalTransform> transform, Entity entity) in
                     SystemAPI.Query<RefRW<LobbedShell>, RefRW<LocalTransform>>()
                         .WithAll<Active>()
                         .WithEntityAccess())
            {
                shell.ValueRW.Elapsed += deltaTime;

                LobbedShell s = shell.ValueRO;
                float t = math.saturate(s.Elapsed / s.FlightTime);

                // 바닥 위치는 직선 보간, 높이는 4t(1-t) — t=0 과 t=1 에서 0, t=0.5 에서 1 인 포물선.
                float2 ground = math.lerp(s.From, s.To, t);
                float lift = ArcHeight * 4f * t * (1f - t);
                transform.ValueRW.Position.xy = ground + new float2(0f, lift);

                if (t >= 1f)
                {
                    landedShells.Add(s);
                    landedEntities.Add(entity);
                }
            }

            if (landedShells.Length > 0)
            {
                // 순회가 끝난 뒤에 끈다. 순회 중인 쿼리의 필터(Active)를 그 자리에서 바꾸지 않기 위해서.
                for (int i = 0; i < landedEntities.Length; i++)
                {
                    PoolUtility.SetAlive(state.EntityManager, landedEntities[i], false);
                }

                Land(ref state, landedShells);
            }

            landedShells.Dispose();
            landedEntities.Dispose();
        }

        private void Land(ref SystemState state, NativeList<LobbedShell> shells)
        {
            // 메인 스레드에서 해시를 읽으므로 이번 프레임 재구축 잡을 먼저 끝낸다.
            EnemySpatialHash hash = SystemAPI.GetSingleton<EnemySpatialHash>();
            hash.BuildHandle.Complete();

            // CellOf 는 Origin 만 쓰고 타일 배열을 건드리지 않으므로 그리드 잡을 기다릴 필요가 없다.
            TerrainGrid grid = SystemAPI.GetSingleton<TerrainGrid>();
            RefRW<TerrainEffects> effects = SystemAPI.GetSingletonRW<TerrainEffects>();

            // 메인 스레드에서 쓰는 스트림이라 foreach 인덱스는 하나면 된다. 해제는 DamageApplySystem 의 몫.
            var stream = new NativeStream(1, Allocator.TempJob);
            NativeStream.Writer writer = stream.AsWriter();
            writer.BeginForEachIndex(0);

            for (int i = 0; i < shells.Length; i++)
            {
                LobbedShell shell = shells[i];

                if (shell.Damage > 0f && shell.Radius > 0f)
                {
                    AreaDamage.Write(hash.Map, shell.To, shell.Radius, shell.Damage, Entity.Null, ref writer);
                }

                TerrainEffectKind kind = shell.Payload == ShellPayload.Oil ? TerrainEffectKind.Oil : TerrainEffectKind.Ignite;
                effects.ValueRW.Request(grid.CellOf(shell.To), kind, shell.Radius);
            }

            writer.EndForEachIndex();

            // 메인 스레드에서 다 썼으므로 기다릴 잡이 없다.
            SystemAPI.GetSingletonRW<DamageEventBus>().ValueRW.Register(stream, default);
        }
    }
}
