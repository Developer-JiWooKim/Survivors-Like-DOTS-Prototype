using Unity.Mathematics;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step05
{
    [BurstCompile]
    [WithDisabled(typeof(Alive), typeof(MaterialMeshInfo))]
    public partial struct AliveJob : IJobEntity
    {
        public float DeltaTime;
        public uint Seed;
        private void Execute(Entity entity, ref Life life, EnabledRefRW<Alive> alive, EnabledRefRW<MaterialMeshInfo> rendering)
        {
            life.WaitElapsed += DeltaTime;
            if (life.WaitElapsed > life.WaitTime)
            {
                uint mixed = math.hash(new uint2((uint)entity.Index, Seed));
                // Random random = Random.CreateFromIndex((uint)entity.Index);
                Random random = Random.CreateFromIndex(mixed);
                float randomValue = random.NextFloat(0, life.MaxLifeTime);
                alive.ValueRW = true;
                rendering.ValueRW = true;
                life.WaitElapsed = 0;
                life.Elapsed = randomValue;
            }
        }
    }
}
