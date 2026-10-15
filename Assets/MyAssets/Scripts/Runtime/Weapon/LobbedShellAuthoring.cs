using Assets.MyAssets.Scripts.Runtime.Pooling;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 착탄형 탄 프리팹에 붙이는 Authoring. 출발점·착탄점·반경·피해는 발사하는 무기가 채운다
    /// (<see cref="ProjectileAuthoring"/> 과 같은 이유 — 한 모양을 여러 무기가 다른 수치로 쓴다).
    /// </summary>
    public sealed class LobbedShellAuthoring : MonoBehaviour
    {
        private sealed class LobbedShellBaker : Baker<LobbedShellAuthoring>
        {
            public override void Bake(LobbedShellAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new LobbedShell());

                // 기름통·화염구가 한 풀을 나눠 써서 색을 인스턴스마다 덮어쓴다 (TileViewAuthoring 과 같은 방식).
                // 머티리얼을 둘로 나누면 프리팹과 풀도 둘이 되어야 한다.
                AddComponent(entity, new URPMaterialPropertyBaseColor { Value = new float4(1f, 1f, 1f, 1f) });

                // 켜진 채로 두고 풀 생성 시스템이 전부 끈다. 이유는 ProjectileAuthoring 주석 참조.
                AddComponent<Active>(entity);
            }
        }
    }
}
