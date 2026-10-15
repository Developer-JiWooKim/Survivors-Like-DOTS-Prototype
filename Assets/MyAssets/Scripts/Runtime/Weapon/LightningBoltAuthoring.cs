using Assets.MyAssets.Scripts.Runtime.Pooling;
using Unity.Entities;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 낙뢰 선 프리팹에 붙이는 Authoring.
    /// </summary>
    public sealed class LightningBoltAuthoring : MonoBehaviour
    {
        private sealed class LightningBoltBaker : Baker<LightningBoltAuthoring>
        {
            public override void Bake(LightningBoltAuthoring authoring)
            {
                // NonUniformScale: 선을 길이 방향으로만 늘리려면 PostTransformMatrix 가 있어야 한다.
                // 이 플래그가 없으면 프리팹 스케일이 균일할 때 PostTransformMatrix 가 붙지 않는다.
                Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.NonUniformScale);

                AddComponent(entity, new LightningBolt());

                // 켜진 채로 두고 풀 생성 시스템이 전부 끈다. 이유는 ProjectileAuthoring 주석 참조.
                AddComponent<Active>(entity);
            }
        }
    }
}
