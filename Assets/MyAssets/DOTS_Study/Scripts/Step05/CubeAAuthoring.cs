using Unity.Entities;
using UnityEngine;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step05
{
    public sealed class CubeAAuthoring : MonoBehaviour
    {
        [SerializeField] private float _maxLifeTime;
        [SerializeField] private float _waitTime;
        private sealed class CubeABaker : Baker<CubeAAuthoring>
        {
            public override void Bake(CubeAAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<CubeA>(entity);
                AddComponent(entity, new Life
                {
                    Elapsed = 0f,
                    MaxLifeTime = authoring._maxLifeTime,
                    WaitElapsed = 0f,
                    WaitTime = authoring._waitTime,
                });
            }
        }
    }
}