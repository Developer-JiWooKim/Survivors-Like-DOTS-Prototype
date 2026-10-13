using Unity.Entities;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step06
{
    public struct Step06PlayerAttack : IComponentData
    {
        public float Range;
        public float SecondPerDamage;
    }
}