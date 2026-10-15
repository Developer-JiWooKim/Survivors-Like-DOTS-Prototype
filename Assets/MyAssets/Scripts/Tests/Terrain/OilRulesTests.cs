using Assets.MyAssets.Scripts.Runtime.Terrain;
using NUnit.Framework;

namespace Assets.MyAssets.Scripts.Tests.Terrain
{
    /// <summary>
    /// 기름을 뿌렸을 때의 타일 변화 테스트 (기획서 4.2).
    ///
    /// 왜 테스트하나: 물 위 기름 규칙이 틀리면 기름통 한 번에 호수가 영구히 사라진다 (ISSUE-012).
    /// 다 타야(약 20 초) 드러나는 결함이라 눈으로 잡기 어렵다.
    /// </summary>
    public sealed class OilRulesTests
    {
        [Test]
        public void Pour_OnDirtOrGrass_BecomesOilWithFuel()
        {
            Assert.IsTrue(OilRules.TryPour(TileData.Of(TileType.Dirt), 200, out TileData onDirt));
            Assert.IsTrue(OilRules.TryPour(TileData.Of(TileType.Grass, 30), 200, out TileData onGrass));

            Assert.AreEqual(TileType.Oil, onDirt.TypeValue);
            Assert.AreEqual(TileType.Oil, onGrass.TypeValue);
            Assert.AreEqual(200, onGrass.Fuel, "풀의 남은 연료가 아니라 기름의 연료로 바뀌어야 한다");
        }

        [Test]
        public void Pour_OnWater_FloatsOnTop()
        {
            // OilOnWater 가 다 타면 물로 돌아오는 것은 BurnRulesTests 가 검증한다. 여기서는 "물 위면 OilOnWater" 까지만.
            Assert.IsTrue(OilRules.TryPour(TileData.Of(TileType.Water), 200, out TileData poured));
            Assert.AreEqual(TileType.OilOnWater, poured.TypeValue);
        }

        [Test]
        public void Pour_OnOilOnWater_StaysOnWater()
        {
            // 같은 웅덩이에 두 번 던져도 "물이었다" 는 정보가 지워지면 안 된다.
            Assert.IsTrue(OilRules.TryPour(TileData.Of(TileType.OilOnWater, 10), 200, out TileData poured));
            Assert.AreEqual(TileType.OilOnWater, poured.TypeValue);
            Assert.AreEqual(200, poured.Fuel);
        }

        [Test]
        public void Pour_OnIce_FloatsLikeWater()
        {
            // 일반 기름이 되면 다 탄 뒤 흙이 되어 물이 영구히 사라진다 (ISSUE-012 와 같은 구멍).
            TileData ice = TileStateRules.Freeze(TileData.Of(TileType.Water), 150);
            Assert.AreEqual(TileType.Ice, ice.TypeValue, "전제: Freeze 가 빙판을 만든다");

            Assert.IsTrue(OilRules.TryPour(ice, 200, out TileData poured));
            Assert.AreEqual(TileType.OilOnWater, poured.TypeValue);
            Assert.IsFalse(TileStateRules.HasTimedState(poured), "얼음은 녹은 것으로 본다 — 빙판 타이머가 남으면 안 된다");
        }

        [Test]
        public void Pour_OnBurningTile_KeepsBurning()
        {
            TileData burningGrass = TileData.Of(TileType.Grass, 5);
            burningGrass.State |= (byte)TileStateFlags.Burning;

            Assert.IsTrue(OilRules.TryPour(burningGrass, 200, out TileData poured));
            Assert.IsTrue(poured.HasState(TileStateFlags.Burning), "불 위에 기름을 부었는데 꺼지면 안 된다");
            Assert.AreEqual(200, poured.Fuel, "새 기름의 연료로 채워진다");
        }

        [Test]
        public void Pour_OnShockedWater_ClearsShock()
        {
            // 감전 외 상태는 초기화한다 (0.5 초짜리라 체감 없음). 규칙을 바꾸면 이 테스트가 알려 준다.
            TileData shocked = TileStateRules.Shock(TileData.Of(TileType.Water), 5);

            Assert.IsTrue(OilRules.TryPour(shocked, 200, out TileData poured));
            Assert.IsFalse(poured.HasState(TileStateFlags.Shocked));
        }

        [Test]
        public void Pour_OnRock_IsRejected()
        {
            TileData rock = TileData.Of(TileType.Rock);
            Assert.IsFalse(OilRules.TryPour(rock, 200, out TileData result));
            Assert.AreEqual(TileType.Rock, result.TypeValue);
        }
    }
}
