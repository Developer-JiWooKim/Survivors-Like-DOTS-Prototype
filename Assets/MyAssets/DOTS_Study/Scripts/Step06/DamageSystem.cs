using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step06
{
    [BurstCompile]
    public partial struct DamageSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<DamageEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            Entity player = SystemAPI.GetSingletonEntity<Step06PlayerTag>();
            float3 targetPosition = SystemAPI.GetComponent<LocalTransform>(player).Position;

            DynamicBuffer<DamageEvent> events = SystemAPI.GetSingletonBuffer<DamageEvent>();
            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<Step06Health>().WithEntityAccess())
            {
                // events.Add(new DamageEvent {Target = entity})
            }
        }
    }
}