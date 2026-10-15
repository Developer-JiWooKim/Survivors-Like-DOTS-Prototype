using Assets.MyAssets.Scripts.Runtime.Enemy;
using Assets.MyAssets.Scripts.Runtime.Player;
using Assets.MyAssets.Scripts.Runtime.Pooling;
using Assets.MyAssets.Scripts.Runtime.Run;
using Assets.MyAssets.Scripts.Runtime.Spatial;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 쿨다운이 끝나면 사거리 안의 **가장 가까운 적**에게 기름통을 던진다.
    /// 착탄과 기름 칠하기는 <see cref="LobbedShellSystem"/> 이 한다.
    ///
    /// 왜 가장 가까운 적인가 (2026-10-15 사용자 결정): 화염구도 가장 가까운 적을 노리므로
    /// 두 무기가 같은 곳에 떨어져 **준비 → 점화** 콤보가 따로 조작하지 않아도 일어난다.
    ///
    /// 메인 스레드인 이유는 <see cref="ShardFireSystem"/> 과 같다 — 발사는 3 초에 한 번이고, 풀에서 꺼내는 일이 메인 스레드 전용이다.
    ///
    /// 순서: 해시 재구축 뒤(조준), 적 이동·투사체 이동 **앞**.
    /// 탄의 LocalTransform 을 메인 스레드에서 쓰므로, LocalTransform 을 쓰는 잡이 예약된 뒤라면 그걸 기다려야 한다.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(PlayerMoveSystem))]
    [UpdateAfter(typeof(BuildEnemySpatialHashSystem))]
    [UpdateBefore(typeof(EnemyChaseSystem))]
    [UpdateBefore(typeof(ProjectileMoveSystem))]
    public partial struct OilBarrelFireSystem : ISystem
    {
        private EntityQuery _weaponQuery;
        private EntityQuery _freeShellQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // 싱글턴 API 대신 쿼리를 직접 쓰는 이유: 무기가 enableable 이라 "켜진 것만" 을 확실히 거르고 싶다.
            // 꺼져 있으면(아직 획득 전) 이 쿼리가 비어 시스템이 아예 돌지 않는다.
            _weaponQuery = SystemAPI.QueryBuilder()
                .WithAllRW<OilBarrelWeapon>()
                .WithAll<LocalTransform>()
                .Build();

            _freeShellQuery = SystemAPI.QueryBuilder()
                .WithAll<LobbedShell>()
                .WithDisabled<Active>()
                .Build();

            state.RequireForUpdate(_weaponQuery);
            state.RequireForUpdate<EnemySpatialHash>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeArray<Entity> owners = _weaponQuery.ToEntityArray(Allocator.Temp);
            float deltaTime = SystemAPI.Time.DeltaTime;

            for (int i = 0; i < owners.Length; i++)
            {
                Entity owner = owners[i];
                OilBarrelWeapon weapon = SystemAPI.GetComponent<OilBarrelWeapon>(owner);

                weapon.Cooldown -= deltaTime;
                if (weapon.Cooldown <= 0f)
                {
                    float2 origin = SystemAPI.GetComponent<LocalTransform>(owner).Position.xy;

                    // 메인 스레드에서 해시를 읽으므로 이번 프레임 재구축 잡을 먼저 끝낸다.
                    EnemySpatialHash hash = SystemAPI.GetSingleton<EnemySpatialHash>();
                    hash.BuildHandle.Complete();

                    var shell = new LobbedShell
                    {
                        FlightTime = weapon.FlightTime,
                        Radius = weapon.Radius,
                        Damage = weapon.Damage,
                        Payload = ShellPayload.Oil,
                    };

                    if (LobbedShellLauncher.TryFireAtNearest(state.EntityManager, hash, _freeShellQuery, origin, weapon.Range, shell))
                    {
                        // 간격이 프레임보다 훨씬 길어 넘친 시간을 이월할 필요가 없다 (파편탄과 다름).
                        weapon.Cooldown = weapon.Interval;
                    }
                    else
                    {
                        // 대상이 없거나 풀이 바닥났으면 0 에 묶어 두고 다음 프레임에 다시 본다.
                        weapon.Cooldown = 0f;
                    }
                }

                SystemAPI.SetComponent(owner, weapon);
            }

            owners.Dispose();
        }
    }
}
