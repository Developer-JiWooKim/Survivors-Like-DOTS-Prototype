using Assets.MyAssets.DOTS_Study.Scripts.Step02;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step05
{
    [BurstCompile]
    public partial struct CubeASystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CubeA>();
            state.RequireForUpdate<Life>();
            state.RequireForUpdate<Spawner>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var spawner = SystemAPI.GetSingleton<Spawner>();

            Random random = Random.CreateFromIndex(spawner.Seed);

            foreach (var (life, transform, entity) in SystemAPI.Query<RefRW<Life>, RefRO<LocalTransform>>().WithAll<CubeA>().WithEntityAccess())
            {
                life.ValueRW.Elapsed += deltaTime;
                if (life.ValueRO.Elapsed > life.ValueRO.MaxLifeTime)
                {
                    ecb.DestroyEntity(entity);
                    RefRW<Life> newLife = life;
                    newLife.ValueRW.Elapsed = random.NextFloat(0, life.ValueRO.MaxLifeTime);
                    Entity newEntity = ecb.Instantiate(spawner.Prefab);
                    ecb.SetComponent(newEntity, transform.ValueRO);
                    ecb.SetComponent(newEntity, newLife.ValueRO);
                }
            }
        }
    }
}
