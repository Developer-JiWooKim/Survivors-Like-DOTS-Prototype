using Unity.Entities;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 기름통 (기획서 5.1 의 8번) — 피해는 낮고 **기름 타일을 만든다.** 콤보 재료 1순위.
    /// 플레이어 엔티티에 붙는다. 수치는 기획서에 없는 M3 임시값 (2026-10-15 사용자 승인, 인스펙터에서 조정).
    ///
    /// 왜 enableable 인가: 레벨업으로 **획득**하는 무기다 (2026-10-15 결정).
    /// 획득할 때 컴포넌트를 붙이면 플레이어가 다른 아키타입으로 옮겨가는 구조적 변경이 된다 (기획서 8.4).
    /// 처음부터 꺼진 채로 붙여 두고, 얻으면 켠다. 꺼져 있는 동안은 발사 시스템 쿼리에 잡히지 않는다.
    /// </summary>
    public struct OilBarrelWeapon : IComponentData, IEnableableComponent
    {
        /// <summary>발사 간격 (초).</summary>
        public float Interval;

        /// <summary>다음 발사까지 남은 시간 (초).</summary>
        public float Cooldown;

        /// <summary>이 거리 안의 가장 가까운 적을 노린다 (월드 유닛).</summary>
        public float Range;

        /// <summary>착탄까지 걸리는 시간 (초).</summary>
        public float FlightTime;

        /// <summary>기름을 뿌리는 반경이자 착탄 피해 반경 (월드 유닛).</summary>
        public float Radius;

        /// <summary>착탄 반경 안 적에게 주는 피해. 기획서 "피해 낮음".</summary>
        public float Damage;
    }
}
