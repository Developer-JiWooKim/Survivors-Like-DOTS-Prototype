using Unity.Entities;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// SubScene 에 배치하는 착탄형 탄 풀 설정.
    /// </summary>
    public sealed class LobbedShellPoolAuthoring : MonoBehaviour
    {
        [Tooltip("LobbedShellAuthoring 이 붙은 프리팹 에셋 (씬 안의 오브젝트 아님)")]
        [SerializeField] private GameObject _prefab;

        [Tooltip("미리 만들어 둘 탄 수. 쿨다운 2~3 초 × 비행 0.6 초라 동시에 몇 발 안 된다. 바닥나면 발사를 건너뛴다.")]
        [SerializeField] private int _capacity = 32;

        private sealed class LobbedShellPoolBaker : Baker<LobbedShellPoolAuthoring>
        {
            public override void Bake(LobbedShellPoolAuthoring authoring)
            {
                if (authoring._prefab == null)
                {
                    return;
                }

                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new LobbedShellPool
                {
                    Prefab = GetEntity(authoring._prefab, TransformUsageFlags.Dynamic),
                    Capacity = authoring._capacity,
                });
            }
        }
    }
}
