using Unity.Entities;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step06
{
    public struct DamageEvent : IBufferElementData
    {
        public Entity Target;
        public float Amount;
    }
}