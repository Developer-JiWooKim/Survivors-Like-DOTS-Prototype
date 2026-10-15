using Unity.Entities;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 플레이어 GameObject 에 <c>PlayerAuthoring</c> 과 함께 붙인다.
    /// 기본은 **꺼진 채**로 베이크되고 레벨업 "Chain Lightning" 을 고르면 켜진다.
    /// </summary>
    public sealed class ChainLightningAuthoring : MonoBehaviour
    {
        [Tooltip("발사 간격 (초)")]
        [SerializeField] private float _interval = 1.5f;

        [Tooltip("첫 타겟을 찾는 거리 (월드 유닛)")]
        [SerializeField] private float _range = 8f;

        [Tooltip("한 적에서 다음 적으로 튈 수 있는 거리 (월드 유닛)")]
        [SerializeField] private float _chainRange = 4f;

        [Tooltip("연쇄 예산. 마른 땅 위 적 하나 = 1")]
        [SerializeField] private float _chainBudget = 3f;

        [Tooltip("물·빙판 위 적을 때릴 때 쓰는 예산. 0.5 = 연쇄 2 배")]
        [SerializeField] private float _wetChainCost = 0.5f;

        [Tooltip("타격당 피해")]
        [SerializeField] private float _damage = 15f;

        [Tooltip("낙뢰 선이 화면에 남는 시간 (초)")]
        [SerializeField] private float _boltDuration = 0.15f;

        [Tooltip("켜면 레벨업 없이 처음부터 들고 시작한다 (테스트용)")]
        [SerializeField] private bool _unlockedAtStart;

        private sealed class ChainLightningBaker : Baker<ChainLightningAuthoring>
        {
            public override void Bake(ChainLightningAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new ChainLightningWeapon
                {
                    Interval = Mathf.Max(0.05f, authoring._interval),
                    Cooldown = 0f,
                    Range = authoring._range,
                    ChainRange = authoring._chainRange,
                    ChainBudget = Mathf.Max(0f, authoring._chainBudget),

                    // 0 이면 젖은 칸에서 무한 연쇄가 된다. 하한을 둬서 막는다 (제외 목록 15 칸도 넘친다).
                    WetChainCost = Mathf.Max(0.1f, authoring._wetChainCost),
                    Damage = authoring._damage,
                    BoltDuration = Mathf.Max(0.01f, authoring._boltDuration),
                });
                SetComponentEnabled<ChainLightningWeapon>(entity, authoring._unlockedAtStart);
            }
        }
    }
}
