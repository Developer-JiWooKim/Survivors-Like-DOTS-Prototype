using Assets.MyAssets.DOTS_Study.Scripts.Step05;
using Assets.MyAssets.DOTS_Study.Scripts.Step06;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step07
{
    [BurstCompile]
    [WithAll(typeof(Step06Health), typeof(Alive))]
    public partial struct TargetMappingJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int, Entity>.ParallelWriter Writer;
        public float CellSize;

        private void Execute(Entity entity, in LocalTransform transform)
        {
            float2 targetPos = new float2(transform.Position.x, transform.Position.y);
            int2 cell = (int2)math.floor(targetPos / CellSize);
            Writer.Add((int)math.hash(cell), entity);
        }
    }
}
