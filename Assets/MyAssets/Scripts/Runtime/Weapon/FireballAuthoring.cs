using Unity.Entities;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 플레이어 GameObject 에 <c>PlayerAuthoring</c> 과 함께 붙인다.
    /// 기본은 **꺼진 채**로 베이크되고 레벨업 "Fireball" 을 고르면 켜진다.
    /// </summary>
    public sealed class FireballAuthoring : MonoBehaviour
    {
        [Tooltip("발사 간격 (초)")]
        [SerializeField] private float _interval = 2f;

        [Tooltip("이 거리 안의 가장 가까운 적을 노린다 (월드 유닛)")]
        [SerializeField] private float _range = 10f;

        [Tooltip("착탄까지 걸리는 시간 (초)")]
        [SerializeField] private float _flightTime = 0.6f;

        [Tooltip("착탄 피해 반경 = 점화 반경 (월드 유닛)")]
        [SerializeField] private float _radius = 1.5f;

        [Tooltip("착탄 반경 안 적에게 주는 피해")]
        [SerializeField] private float _damage = 20f;

        [Tooltip("켜면 레벨업 없이 처음부터 들고 시작한다 (테스트용)")]
        [SerializeField] private bool _unlockedAtStart;

        private sealed class FireballBaker : Baker<FireballAuthoring>
        {
            public override void Bake(FireballAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new FireballWeapon
                {
                    Interval = Mathf.Max(0.05f, authoring._interval),
                    Cooldown = 0f,
                    Range = authoring._range,
                    FlightTime = Mathf.Max(0.05f, authoring._flightTime),
                    Radius = Mathf.Max(0f, authoring._radius),
                    Damage = authoring._damage,
                });
                SetComponentEnabled<FireballWeapon>(entity, authoring._unlockedAtStart);
            }
        }
    }
}
