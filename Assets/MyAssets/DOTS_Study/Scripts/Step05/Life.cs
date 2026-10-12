using Unity.Entities;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step05
{
    public struct Life : IComponentData
    {
        public float Elapsed;
        public float MaxLifeTime;
        public float WaitTime;
        public float WaitElapsed;
    }
}
