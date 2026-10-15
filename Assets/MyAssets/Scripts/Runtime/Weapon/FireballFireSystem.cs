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
    /// 쿨다운이 끝나면 사거리 안의 **가장 가까운 적**에게 화염구를 쏜다 (기획서 5.1 의 2번 — 점화원).
    /// 착탄 피해와 점화는 <see cref="LobbedShellSystem"/> 이 한다.
    ///
    /// <see cref="OilBarrelFireSystem"/> 과 절차가 같다 (공통부는 <see cref="LobbedShellLauncher"/>).
    /// 시스템을 하나로 합치지 않은 이유: 무기마다 enableable 컴포넌트가 따로라 쿼리도 따로다.
    /// 한 시스템에 쿼리 둘을 두면 "획득 전에는 아예 돌지 않는다" 를 RequireForUpdate 로 표현할 수 없다.
    ///
    /// 순서 이유는 <see cref="OilBarrelFireSystem"/> 주석 참조.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateAfter(typeof(PlayerMoveSystem))]
    [UpdateAfter(typeof(BuildEnemySpatialHashSystem))]
    [UpdateBefore(typeof(EnemyChaseSystem))]
    [UpdateBefore(typeof(ProjectileMoveSystem))]
    public partial struct FireballFireSystem : ISystem
    {
        private EntityQuery _weaponQuery;
        private EntityQuery _freeShellQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // 싱글턴 API 대신 쿼리를 직접 쓰는 이유: 무기가 enableable 이라 "켜진 것만" 을 확실히 거르고 싶다.
            // 꺼져 있으면(아직 획득 전) 이 쿼리가 비어 시스템이 아예 돌지 않는다.
            _weaponQuery = SystemAPI.QueryBuilder()
                .WithAllRW<FireballWeapon>()
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
                FireballWeapon weapon = SystemAPI.GetComponent<FireballWeapon>(owner);

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
                        Payload = ShellPayload.Fire,
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
