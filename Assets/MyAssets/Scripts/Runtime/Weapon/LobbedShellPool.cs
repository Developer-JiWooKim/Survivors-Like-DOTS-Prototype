using Unity.Entities;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 착탄형 탄 풀 설정 싱글턴. 시작 시 <see cref="Capacity"/> 개를 미리 만들어 전부 꺼둔다.
    /// 기름통·화염구가 같은 풀을 나눠 쓴다 — 모양이 같고 <see cref="LobbedShell.Payload"/> 만 다르다.
    /// </summary>
    public struct LobbedShellPool : IComponentData
    {
        public Entity Prefab;
        public int Capacity;
    }
}
