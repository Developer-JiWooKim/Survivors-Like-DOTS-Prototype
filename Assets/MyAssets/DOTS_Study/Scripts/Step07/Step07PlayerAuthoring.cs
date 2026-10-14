using Assets.MyAssets.DOTS_Study.Scripts.Step03;
using Assets.MyAssets.DOTS_Study.Scripts.Step06;
using Unity.Entities;
using UnityEngine;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step07
{
    public sealed class Step07PlayerAuthoring : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _range = 3f;
        [SerializeField] private float _damagePerSecond = 1f;

        private sealed class Step07PlayerBaker : Baker<Step07PlayerAuthoring>
        {
            public override void Bake(Step07PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MoveSpeed
                {
                    Speed = authoring._moveSpeed,
                });
                AddComponent(entity, new Step06PlayerAttack
                {
                    Range = authoring._range,
                    DamagePerSecond = authoring._damagePerSecond,
                });
                AddComponent<Step07PlayerTag>(entity);

            }
        }
    }
}