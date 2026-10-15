using Assets.MyAssets.Scripts.Runtime.Terrain;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 한 번의 전격 사슬이 남긴 연쇄 예산. 시스템의 연쇄 루프와 테스트가 같은 규칙을 쓰도록 꺼냈다.
    /// </summary>
    public struct ChainBudget
    {
        /// <summary>
        /// 0.5 단위 뺄셈은 float 에서 정확하지만, 인스펙터에서 0.3 같은 값을 넣으면 0 근처에 오차가 남는다.
        /// 그때 "0.0000001 남아서 한 번 더 친다" 를 막는 여유.
        /// </summary>
        private const float Epsilon = 1e-4f;

        public float Remaining;

        public ChainBudget(float total)
        {
            Remaining = total;
        }

        public bool CanStrike => Remaining > Epsilon;

        /// <summary>발밑 타일이 <paramref name="tileUnderTarget"/> 인 적을 때린 만큼 예산을 쓴다.</summary>
        public void Spend(TileType tileUnderTarget, float wetCost)
        {
            Remaining -= ChainLightningRules.BudgetCost(tileUnderTarget, wetCost);
        }
    }

    /// <summary>
    /// 전격 사슬이 맞힌 적의 발밑 타일에 따라 정해지는 것들 (기획서 4.2·4.3·5.6).
    /// <see cref="BurnRules"/> 와 같은 이유로 순수 함수만 둔다 — "물 위에서 2 배" 는 눈으로 세기 어렵다.
    /// </summary>
    public static class ChainLightningRules
    {
        /// <summary>
        /// 물·빙판 위 적 연쇄 2 배 (기획서 5.1) — 젖은 칸 위 적을 때리면 예산을 <paramref name="wetCost"/> 만 쓴다.
        /// "첫 타겟이 젖었으면 전부 2 배" 가 아니라 타격마다 따지는 해석이다 (2026-10-15 사용자 결정).
        /// 빙판도 젖은 칸이다 — 감전은 안 통하지만(기획서 4.3) 연쇄 2 배는 받는다.
        /// </summary>
        public static float BudgetCost(TileType tileUnderTarget, float wetCost)
        {
            return IsWet(tileUnderTarget) ? wetCost : 1f;
        }

        /// <summary>
        /// 이 칸에서 맞으면 물 덩어리 감전을 일으키는가 (기획서 4.2 — 물 + 전격 → 전체 감전).
        /// 물 위 기름은 통하고 빙판은 통하지 않는다 (기획서 4.3, 2026-09-18 결정).
        /// </summary>
        public static bool TriggersShock(TileType tileUnderTarget)
        {
            return TileTypes.IsWaterLike(tileUnderTarget);
        }

        public static bool IsWet(TileType type)
        {
            return TileTypes.IsWaterLike(type) || type == TileType.Ice;
        }
    }
}
