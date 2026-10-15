using Assets.MyAssets.Scripts.Runtime.Pooling;
using Assets.MyAssets.Scripts.Runtime.Run;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 낙뢰 선의 남은 시간을 줄이고, 다 되면 풀로 돌려보낸다.
    ///
    /// 메인 스레드인 이유: 선은 동시에 몇 개뿐이다 (1.5 초에 한 번, 최대 6 구간 × 0.15 초).
    ///
    /// <see cref="ChainLightningSystem"/> **앞**에 도는 이유: 이번 프레임에 띄운 선이 같은 프레임에
    /// dt 만큼 깎이지 않고 설정한 시간을 온전히 보이게.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(ChainLightningSystem))]
    public partial struct LightningBoltSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LightningBolt>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            var expired = new NativeList<Entity>(Allocator.Temp);

            foreach ((RefRW<LightningBolt> bolt, Entity entity) in
                     SystemAPI.Query<RefRW<LightningBolt>>()
                         .WithAll<Active>()
                         .WithEntityAccess())
            {
                bolt.ValueRW.Remaining -= deltaTime;
                if (bolt.ValueRO.Remaining <= 0f)
                {
                    expired.Add(entity);
                }
            }

            // 순회가 끝난 뒤에 끈다 (LobbedShellSystem 과 같은 이유).
            for (int i = 0; i < expired.Length; i++)
            {
                PoolUtility.SetAlive(state.EntityManager, expired[i], false);
            }

            expired.Dispose();
        }
    }
}
