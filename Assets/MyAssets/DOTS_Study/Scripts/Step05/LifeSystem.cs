using Unity.Burst;
using Unity.Entities;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step05
{
    [BurstCompile]
    public partial struct LifeSystem : ISystem
    {
        private const float SeedMul = 1000f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Alive>();
            state.RequireForUpdate<Life>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Job으로 분리 전
            {
                // foreach (var (life, alive, rendering) in SystemAPI.Query<RefRW<Life>, EnabledRefRW<Alive>, EnabledRefRW<MaterialMeshInfo>>().WithAll<Alive>())
                // {
                //     life.ValueRW.Elapsed += deltaTime;
                //     if (life.ValueRO.Elapsed > life.ValueRO.MaxLifeTime)
                //     {
                //         alive.ValueRW = false;
                //         rendering.ValueRW = false;
                //     }
                // }
            }

            // Job으로 분리 후
            // new LifeJob { DeltaTime = deltaTime }.Run();
            // new LifeJob { DeltaTime = deltaTime }.Schedule();
            new LifeJob { DeltaTime = deltaTime }.ScheduleParallel();
            new AliveJob { DeltaTime = deltaTime, Seed = (uint)(SystemAPI.Time.ElapsedTime * SeedMul) }.ScheduleParallel();
        }
    }
}
