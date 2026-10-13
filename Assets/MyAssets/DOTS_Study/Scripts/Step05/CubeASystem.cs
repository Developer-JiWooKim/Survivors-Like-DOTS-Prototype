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
        private const float SeedMul = 1000f;

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

            uint mixed = math.hash(new uint2((uint)(SystemAPI.Time.ElapsedTime * SeedMul), spawner.Seed));
            Random random = Random.CreateFromIndex(mixed);

            foreach (var (life, transform, entity) in SystemAPI.Query<RefRW<Life>, RefRO<LocalTransform>>().WithAll<CubeA>().WithEntityAccess())
            {
                life.ValueRW.Elapsed += deltaTime;
                if (life.ValueRO.Elapsed > life.ValueRO.MaxLifeTime)
                {
                    ecb.DestroyEntity(entity);
                    Life newLife = life.ValueRO;
                    newLife.Elapsed = random.NextFloat(0, life.ValueRO.MaxLifeTime);
                    Entity newEntity = ecb.Instantiate(spawner.Prefab);
                    ecb.SetComponent(newEntity, transform.ValueRO);
                    ecb.SetComponent(newEntity, newLife);
                }
            }
        }
    }
}
