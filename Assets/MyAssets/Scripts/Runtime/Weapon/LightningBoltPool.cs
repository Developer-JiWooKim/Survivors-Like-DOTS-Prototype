using Unity.Entities;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 낙뢰 선 풀 설정 싱글턴. 시작 시 <see cref="Capacity"/> 개를 미리 만들어 전부 꺼둔다.
    /// </summary>
    public struct LightningBoltPool : IComponentData
    {
        public Entity Prefab;
        public int Capacity;
    }
}
