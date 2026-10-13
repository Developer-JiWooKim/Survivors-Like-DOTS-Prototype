using Unity.Entities;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step06
{
    public struct Step06Health : IComponentData
    {
        public float CurrentHp;
        public float MaxHp;
    }
}