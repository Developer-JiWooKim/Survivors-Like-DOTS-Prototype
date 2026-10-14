using Assets.MyAssets.DOTS_Study.Scripts.Step05;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step06
{
    [BurstCompile]
    [UpdateAfter(typeof(Step06DamageApplySystem))]
    public partial struct Step06DeadSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Step06Health>();
            state.RequireForUpdate<Alive>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (health, alive, mat) in SystemAPI.Query<RefRO<Step06Health>, EnabledRefRW<Alive>, EnabledRefRW<MaterialMeshInfo>>())
            {
                if (health.ValueRO.CurrentHp <= 0)
                {
                    alive.ValueRW = false;
                    mat.ValueRW = false;
                }
            }
        }
    }
}