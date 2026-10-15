using Unity.Entities;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// SubScene 에 배치하는 낙뢰 선 풀 설정.
    /// </summary>
    public sealed class LightningBoltPoolAuthoring : MonoBehaviour
    {
        [Tooltip("LightningBoltAuthoring 이 붙은 프리팹 에셋 (씬 안의 오브젝트 아님)")]
        [SerializeField] private GameObject _prefab;

        [Tooltip("미리 만들어 둘 선 수. 한 번에 최대 6 구간(물 위 연쇄) × 0.15 초라 동시에 몇 개 안 된다. 바닥나면 선만 안 그리고 피해는 그대로 준다.")]
        [SerializeField] private int _capacity = 32;

        private sealed class LightningBoltPoolBaker : Baker<LightningBoltPoolAuthoring>
        {
            public override void Bake(LightningBoltPoolAuthoring authoring)
            {
                if (authoring._prefab == null)
                {
                    return;
                }

                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new LightningBoltPool
                {
                    Prefab = GetEntity(authoring._prefab, TransformUsageFlags.Dynamic),
                    Capacity = authoring._capacity,
                });
            }
        }
    }
}
