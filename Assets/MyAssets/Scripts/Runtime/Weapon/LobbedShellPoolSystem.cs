using Assets.MyAssets.Scripts.Runtime.Pooling;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 시작 시 착탄형 탄을 풀 용량만큼 한 번에 만들고 전부 비활성으로 둔다 (<see cref="ProjectilePoolSystem"/> 과 같은 방식).
    ///
    /// 풀 시스템을 제네릭 하나로 합치지 않은 이유: 풀마다 다른 것은 싱글턴 타입 하나뿐이고 본문은 열 줄 남짓이다.
    /// ISystem 은 제네릭으로 만들면 시스템마다 구체 타입을 따로 등록해야 해서, 합쳐도 줄어드는 코드가 거의 없다.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct LobbedShellPoolSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LobbedShellPool>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            LobbedShellPool pool = SystemAPI.GetSingleton<LobbedShellPool>();
            if (pool.Capacity <= 0 || pool.Prefab == Entity.Null)
            {
                return;
            }

            EntityManager entityManager = state.EntityManager;
            NativeArray<Entity> instances = entityManager.Instantiate(pool.Prefab, pool.Capacity, Allocator.Temp);

            for (int i = 0; i < instances.Length; i++)
            {
                PoolUtility.SetAlive(entityManager, instances[i], false);
            }

            instances.Dispose();
        }
    }
}
