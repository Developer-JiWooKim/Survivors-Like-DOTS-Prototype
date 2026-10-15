using Assets.MyAssets.Scripts.Runtime.Terrain;
using Assets.MyAssets.Scripts.Runtime.Weapon;
using NUnit.Framework;

namespace Assets.MyAssets.Scripts.Tests.Weapon
{
    /// <summary>
    /// 전격 사슬 연쇄 규칙 테스트 (기획서 5.1 "물/빙판 위 적 연쇄 2배", 5.6 해석).
    ///
    /// 왜 테스트하나: 연쇄는 0.15 초 안에 끝나 몇 번 튀었는지 눈으로 세기 어렵고,
    /// 예산 계산이 한 번만 어긋나도 "2 배" 가 "1.5 배" 나 "무한" 이 된다.
    /// </summary>
    public sealed class ChainLightningRulesTests
    {
        private const float Budget = 3f;
        private const float WetCost = 0.5f;

        [Test]
        public void AllDry_StrikesThreeTimes()
        {
            Assert.AreEqual(3, CountStrikes(TileType.Dirt));
        }

        [TestCase(TileType.Water)]
        [TestCase(TileType.OilOnWater)]
        [TestCase(TileType.Ice)]
        public void AllWet_StrikesSixTimes(TileType wet)
        {
            Assert.AreEqual(6, CountStrikes(wet));
        }

        [Test]
        public void Mixed_CountsEachStrikeByItsOwnTile()
        {
            // 물 → 물 → 흙 → 흙 = 0.5 + 0.5 + 1 + 1 = 3 → 4 타.
            // "첫 타겟이 물이면 전부 2 배" 해석이었다면 6 타가 된다 (채택하지 않은 안).
            TileType[] sequence = { TileType.Water, TileType.Water, TileType.Dirt, TileType.Dirt, TileType.Dirt };
            Assert.AreEqual(4, CountStrikes(sequence));
        }

        [Test]
        public void LastStrikeAllowedEvenIfItOverspends()
        {
            // 0.5 남았을 때 흙 위 적(1)을 만나도 친다 — 남은 예산이 있으면 한 번은 친다.
            // 흙 ×2 (2) → 물 (0.5) → 0.5 남음 → 흙 (1) 침 → -0.5 → 끝. = 4 타.
            TileType[] sequence = { TileType.Dirt, TileType.Dirt, TileType.Water, TileType.Dirt, TileType.Dirt };
            Assert.AreEqual(4, CountStrikes(sequence));
        }

        [Test]
        public void Shock_OnlyOnWaterLike_NotIce()
        {
            Assert.IsTrue(ChainLightningRules.TriggersShock(TileType.Water));
            Assert.IsTrue(ChainLightningRules.TriggersShock(TileType.OilOnWater), "물 위 기름은 전기가 통한다 (기획서 4.3)");
            Assert.IsFalse(ChainLightningRules.TriggersShock(TileType.Ice), "빙판은 전기가 통하지 않는다 (기획서 4.3)");
            Assert.IsFalse(ChainLightningRules.TriggersShock(TileType.Dirt));
        }

        private static int CountStrikes(TileType everyTile)
        {
            var budget = new ChainBudget(Budget);
            int strikes = 0;
            while (budget.CanStrike && strikes < 100)
            {
                budget.Spend(everyTile, WetCost);
                strikes++;
            }

            return strikes;
        }

        private static int CountStrikes(TileType[] sequence)
        {
            var budget = new ChainBudget(Budget);
            int strikes = 0;
            while (budget.CanStrike && strikes < sequence.Length)
            {
                budget.Spend(sequence[strikes], WetCost);
                strikes++;
            }

            return strikes;
        }
    }
}
