using Unity.Entities;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 화염구 (기획서 5.1 의 2번) — 포물선 착탄, **점화원.** 착탄 반경 안 적에게 피해 + 가연 타일(풀·기름) 점화.
    /// 흙·물에 떨어지면 피해만 주고 장판은 남지 않는다 (기획서 5.6 — "기름을 깔아야 불이 산다").
    /// 플레이어 엔티티에 붙는다. 수치는 기획서에 없는 M3 임시값 (2026-10-15 사용자 승인, 인스펙터에서 조정).
    ///
    /// 왜 enableable 인가는 <see cref="OilBarrelWeapon"/> 과 같다 — 레벨업으로 획득하는 무기다.
    /// </summary>
    public struct FireballWeapon : IComponentData, IEnableableComponent
    {
        /// <summary>발사 간격 (초).</summary>
        public float Interval;

        /// <summary>다음 발사까지 남은 시간 (초).</summary>
        public float Cooldown;

        /// <summary>이 거리 안의 가장 가까운 적을 노린다 (월드 유닛).</summary>
        public float Range;

        /// <summary>착탄까지 걸리는 시간 (초).</summary>
        public float FlightTime;

        /// <summary>착탄 피해 반경이자 점화 반경 (월드 유닛).</summary>
        public float Radius;

        /// <summary>착탄 반경 안 적에게 주는 피해.</summary>
        public float Damage;
    }
}
