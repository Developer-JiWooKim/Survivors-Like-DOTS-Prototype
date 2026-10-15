using Unity.Entities;
using Unity.Mathematics;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>착탄했을 때 지형에 무엇을 남기는가.</summary>
    public enum ShellPayload : byte
    {
        /// <summary>기름통 — 반경 안에 기름 타일 (기획서 5.1 의 8번).</summary>
        Oil = 0,

        /// <summary>화염구 — 반경 안의 가연 타일 점화 (기획서 5.1 의 2번).</summary>
        Fire = 1,
    }

    /// <summary>
    /// 포물선을 그리며 정해진 지점에 떨어지는 탄 (기름통·화염구). 날아가는 동안은 아무것도 맞히지 않고,
    /// <see cref="FlightTime"/> 이 지나 착탄하는 순간에만 범위 피해와 지형 효과를 낸다.
    ///
    /// 왜 <see cref="Projectile"/> 을 재사용하지 않고 따로 두나 (2026-10-15 사용자 결정):
    /// 파편탄은 날아가며 매 프레임 충돌 검사(병렬 잡)를 하고, 착탄형은 충돌 검사가 없다.
    /// 한 컴포넌트에 합치면 히트 잡에 분기가 생기고, 착탄형에는 필요 없는
    /// <c>HitHistory</c>(128 byte)가 따라붙는다 — 기획서 8.4 "컴포넌트 최소화" 에 어긋난다.
    ///
    /// 왜 위치를 매 프레임 적분하지 않고 출발점·착탄점·경과 시간으로 들고 있나:
    /// 착탄점이 **발사 순간 정해진다** (플레이어가 보고 피하거나 기다릴 수 있도록 — 같은 날 결정).
    /// 경과 비율 하나로 위치가 정해지니 오차가 쌓이지 않고, 정확히 착탄점에 떨어진다.
    /// </summary>
    public struct LobbedShell : IComponentData
    {
        public float2 From;
        public float2 To;

        /// <summary>발사 후 지난 시간 (초).</summary>
        public float Elapsed;

        /// <summary>착탄까지 걸리는 시간 (초).</summary>
        public float FlightTime;

        /// <summary>착탄 피해·지형 효과 반경 (월드 유닛).</summary>
        public float Radius;

        /// <summary>착탄 반경 안 적에게 한 번 주는 피해.</summary>
        public float Damage;

        public ShellPayload Payload;
    }
}
