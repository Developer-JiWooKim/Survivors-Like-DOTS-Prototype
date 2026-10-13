using Assets.MyAssets.DOTS_Study.Scripts.Step05;
using Unity.Entities;
using UnityEngine;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step06
{
    public sealed class Step06TargetAuthoring : MonoBehaviour
    {
        [SerializeField] private float _maxHp = 100;

        private sealed class Step06TargetBaker : Baker<Step06TargetAuthoring>
        {
            public override void Bake(Step06TargetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Step06Health
                {
                    CurrentHp = authoring._maxHp,
                    MaxHp = authoring._maxHp,
                });
                AddComponent<Alive>(entity);
            }
        }
    }
}