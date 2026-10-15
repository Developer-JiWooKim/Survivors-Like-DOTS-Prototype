using Unity.Entities;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 전격 사슬의 연쇄 한 구간을 보여 주는 선 (2026-10-15 결정 — "짧게 보이는 선").
    /// 판정은 없고 순수 연출이다. 피해·감전은 <see cref="ChainLightningSystem"/> 이 쏘는 순간 다 처리한다.
    ///
    /// 선 = 쿼드를 길이 방향으로 늘린 것. 균일 스케일만 담는 LocalTransform 으로는 늘릴 수 없어서
    /// 프리팹이 <c>PostTransformMatrix</c> 를 갖도록 베이크한다 (<see cref="LightningBoltAuthoring"/>).
    /// </summary>
    public struct LightningBolt : IComponentData
    {
        /// <summary>화면에서 사라질 때까지 남은 시간 (초).</summary>
        public float Remaining;
    }
}
