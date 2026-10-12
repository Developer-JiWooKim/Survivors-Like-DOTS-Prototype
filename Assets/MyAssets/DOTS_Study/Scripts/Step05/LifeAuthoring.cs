using Unity.Entities;
using UnityEngine;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step05
{
    public sealed class LifeAuthoring : MonoBehaviour
    {
        [SerializeField] private float _maxLifeTime;
        [SerializeField] private float _waitTime;

        private sealed class LifeBaker : Baker<LifeAuthoring>
        {
            public override void Bake(LifeAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Life()
                {
                    Elapsed = 0f,
                    MaxLifeTime = authoring._maxLifeTime,
                    WaitElapsed = 0f,
                    WaitTime = authoring._waitTime,
                });

                AddComponent<Alive>(entity);
            }
        }
    }
}