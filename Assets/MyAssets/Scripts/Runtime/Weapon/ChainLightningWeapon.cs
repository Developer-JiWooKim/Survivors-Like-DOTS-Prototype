using Unity.Entities;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 전격 사슬 (기획서 5.1 의 4번) — 가장 가까운 적에게 낙뢰, 이어서 가까운 적으로 연쇄.
    /// **물·빙판 위 적은 연쇄 예산을 절반만 쓴다** (= 연쇄 2 배), 물 위 적을 맞히면 물 덩어리 전체 감전.
    /// 플레이어 엔티티에 붙는다. 수치는 기획서에 없는 M3 임시값 (2026-10-15 사용자 승인, 인스펙터에서 조정).
    ///
    /// 왜 enableable 인가는 <see cref="OilBarrelWeapon"/> 과 같다 — 레벨업으로 획득하는 무기다.
    /// </summary>
    public struct ChainLightningWeapon : IComponentData, IEnableableComponent
    {
        /// <summary>발사 간격 (초).</summary>
        public float Interval;

        /// <summary>다음 발사까지 남은 시간 (초).</summary>
        public float Cooldown;

        /// <summary>첫 타겟을 찾는 거리 (월드 유닛).</summary>
        public float Range;

        /// <summary>한 적에서 다음 적으로 튈 수 있는 거리 (월드 유닛).</summary>
        public float ChainRange;

        /// <summary>연쇄 예산. 마른 땅 위 적 하나 = 1. 기획서 "3 연쇄".</summary>
        public float ChainBudget;

        /// <summary>물·빙판 위 적을 때릴 때 쓰는 예산. 0.5 = 연쇄 2 배.</summary>
        public float WetChainCost;

        /// <summary>타격당 피해.</summary>
        public float Damage;

        /// <summary>낙뢰 선이 화면에 남는 시간 (초). 순수 연출값.</summary>
        public float BoltDuration;
    }
}
