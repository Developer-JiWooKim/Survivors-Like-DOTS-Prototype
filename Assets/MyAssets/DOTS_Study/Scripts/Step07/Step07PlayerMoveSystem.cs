using Assets.MyAssets.DOTS_Study.Scripts.Step03;
using Assets.MyAssets.DOTS_Study.Scripts.Step06;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step07
{
    [BurstCompile]
    [UpdateAfter(typeof(Step06StudyInputSystem))]
    public partial struct Step07PlayerMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Step07PlayerTag>();
            state.RequireForUpdate<Step06StudyInputState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            float2 moveDir = SystemAPI.GetSingleton<Step06StudyInputState>().Move;

            foreach (var (transform, speed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveSpeed>>().WithAll<Step07PlayerTag>())
            {
                transform.ValueRW.Position.x += moveDir.x * speed.ValueRO.Speed * deltaTime;
                transform.ValueRW.Position.y += moveDir.y * speed.ValueRO.Speed * deltaTime;
            }
        }
    }
}
