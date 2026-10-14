using Unity.Burst;
using Unity.Entities;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step06
{
    [BurstCompile]
    public partial struct Step06DamageApplySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<Step06DamageEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            DynamicBuffer<Step06DamageEvent> events = SystemAPI.GetSingletonBuffer<Step06DamageEvent>();

            for (int i = 0; i < events.Length; i++)
            {
                RefRW<Step06Health> health = SystemAPI.GetComponentRW<Step06Health>(events[i].Target);
                health.ValueRW.CurrentHp -= events[i].Amount;
            }
            events.Clear();
        }
    }
}