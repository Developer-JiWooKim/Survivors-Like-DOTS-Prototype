namespace Assets.MyAssets.Scripts.Runtime.Terrain
{
    /// <summary>
    /// 기름을 뿌렸을 때 타일이 어떻게 바뀌는가 (기획서 4.2 — 기름 / 물 위 기름).
    ///
    /// 디버그 G 키가 들고 있던 규칙을 기름통 무기와 함께 쓰려고 꺼냈다.
    /// 같은 규칙이 두 곳에 있으면 한쪽만 고쳐져 "디버그로는 되는데 무기로는 안 된다" 가 생긴다.
    /// <see cref="BurnRules"/> 와 같은 이유로 순수 함수만 둔다 — EditMode 테스트로 검증하기 위해서.
    /// </summary>
    public static class OilRules
    {
        /// <summary>
        /// 뿌린 기름의 연료량 (틱 수). 디버그 G 키에서 쓰던 값 그대로 — 10Hz 에서 약 20 초 탄다.
        /// 기획서에 없는 임시값.
        /// </summary>
        public const byte DefaultFuel = 200;

        /// <summary>
        /// 기름을 덮을 수 있으면 덮은 결과를 <paramref name="poured"/> 에 담고 true.
        ///
        /// - 바위는 덮지 않는다. 벽에 기름이 발리면 확산 경계가 헷갈린다.
        /// - 물 위에서는 기름이 뜬다 — 다 타도 물이 남는다 (ISSUE-012).
        /// - **빙판도 물로 친다** (2026-10-15 결정). 일반 기름으로 덮으면 다 탄 뒤 흙이 되어
        ///   물이 영구히 사라진다 — ISSUE-012 와 같은 구멍이다. 얼음은 기름 아래에서 녹은 것으로 보고
        ///   빙판 타이머도 함께 버린다 (물 위 기름은 얼지 않은 물 위에 있어야 감전이 통한다, 기획서 4.3).
        /// - **타고 있던 칸은 계속 탄다** (같은 날 결정). 불 위에 기름을 부었는데 꺼지면 직관과 반대다.
        ///   연료는 새 기름의 양으로 채워지므로 오히려 더 오래 탄다.
        /// - 그 밖의 상태(감전 등)는 초기화한다. 감전은 0.5 초짜리라 잃어도 체감이 없다.
        /// </summary>
        public static bool TryPour(in TileData tile, byte fuel, out TileData poured)
        {
            TileType existing = tile.TypeValue;
            if (existing == TileType.Rock)
            {
                poured = tile;
                return false;
            }

            bool onWater = TileTypes.IsWaterLike(existing) || existing == TileType.Ice;
            poured = TileData.Of(onWater ? TileType.OilOnWater : TileType.Oil, fuel);

            if (tile.HasState(TileStateFlags.Burning))
            {
                poured.State |= (byte)TileStateFlags.Burning;
            }

            return true;
        }
    }
}
