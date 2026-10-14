using Assets.MyAssets.DOTS_Study.Scripts.Step03;
using Unity.Entities;
using UnityEngine;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step06
{
    public sealed class Step06PlayerAuthoring : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _range = 3f;
        [SerializeField] private float _secondsPerDamage = 1f;

        private sealed class Step06PlayerBaker : Baker<Step06PlayerAuthoring>
        {
            public override void Bake(Step06PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MoveSpeed
                {
                    Speed = authoring._moveSpeed,
                });
                AddComponent(entity, new Step06PlayerAttack
                {
                    Range = authoring._range,
                    DamagePerSecond = authoring._secondsPerDamage,
                });
                AddComponent<Step06PlayerTag>(entity);

            }
        }
    }
}