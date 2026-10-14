using Assets.MyAssets.DOTS_Study.Scripts.Step05;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step06
{
    [BurstCompile]
    [UpdateBefore(typeof(Step06DamageApplySystem))]
    public partial struct Step06PlayerAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Step06DamageEvent>();
            state.RequireForUpdate<Step06PlayerTag>();
            state.RequireForUpdate<Alive>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            Entity player = SystemAPI.GetSingletonEntity<Step06PlayerTag>();
            float3 playerPosition = SystemAPI.GetComponent<LocalTransform>(player).Position;

            DynamicBuffer<Step06DamageEvent> events = SystemAPI.GetSingletonBuffer<Step06DamageEvent>();
            Step06PlayerAttack attack = SystemAPI.GetComponent<Step06PlayerAttack>(player);

            foreach (var (targetTransform, targetEntity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<Step06Health, Alive>().WithEntityAccess())
            {
                if (math.distancesq(playerPosition, targetTransform.ValueRO.Position) < (attack.Range * attack.Range))
                {
                    events.Add(new Step06DamageEvent { Target = targetEntity, Amount = attack.DamagePerSecond * deltaTime });
                }
            }
        }
    }
}