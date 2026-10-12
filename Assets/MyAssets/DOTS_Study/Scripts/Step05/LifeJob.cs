using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step05
{
    [BurstCompile]
    public partial struct LifeJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref Life life, EnabledRefRW<Alive> alive, EnabledRefRW<MaterialMeshInfo> rendering)
        {
            life.Elapsed += DeltaTime;
            if (life.Elapsed > life.MaxLifeTime)
            {
                alive.ValueRW = false;
                rendering.ValueRW = false;
            }
        }
    }
}
