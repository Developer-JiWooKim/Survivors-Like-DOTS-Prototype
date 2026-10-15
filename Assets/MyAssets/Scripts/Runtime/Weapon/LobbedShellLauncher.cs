using Assets.MyAssets.Scripts.Runtime.Pooling;
using Assets.MyAssets.Scripts.Runtime.Spatial;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Assets.MyAssets.Scripts.Runtime.Weapon
{
    /// <summary>
    /// 착탄형 무기(기름통·화염구)의 발사 공통부 — 조준, 풀에서 꺼내기, 쏘기. 메인 스레드 전용.
    ///
    /// 두 무기는 수치와 <see cref="ShellPayload"/> 만 다르고 발사 절차가 같다.
    /// 무기마다 컴포넌트·시스템은 따로 두되(획득 여부를 무기별 enableable 로 켜고 끄려고) 절차는 여기 한 벌만 둔다.
    /// </summary>
    public static class LobbedShellLauncher
    {
        /// <summary>
        /// <paramref name="range"/> 안의 가장 가까운 적 자리로 탄 하나를 쏜다.
        /// 대상이 없거나 풀이 바닥났으면 false — 호출자는 쿨다운을 0 에 묶어 두고 다음 프레임에 다시 시도한다.
        /// </summary>
        /// <param name="hash">재구축 잡이 끝난(<c>BuildHandle.Complete()</c>) 해시여야 한다.</param>
        /// <param name="shell">From·To·Elapsed 를 뺀 탄 설정. 이 함수가 채운다.</param>
        public static bool TryFireAtNearest(
            EntityManager entityManager, in EnemySpatialHash hash, EntityQuery freeShellQuery,
            float2 origin, float range, LobbedShell shell)
        {
            var nearest = new FixedList512Bytes<float2>();
            SpatialQueries.FindNearest(hash.Map, origin, range, 1, ref nearest);
            if (nearest.Length == 0 || freeShellQuery.IsEmpty)
            {
                return false;
            }

            NativeArray<Entity> free = freeShellQuery.ToEntityArray(Allocator.Temp);
            Entity entity = free[0];
            free.Dispose();

            shell.From = origin;
            shell.To = nearest[0];
            Launch(entityManager, entity, shell);
            return true;
        }

        private static void Launch(EntityManager entityManager, Entity entity, LobbedShell shell)
        {
            // 프리팹의 스케일·Z 를 보존하고 XY 만 옮긴다.
            LocalTransform transform = entityManager.GetComponentData<LocalTransform>(entity);
            transform.Position = new float3(shell.From, transform.Position.z);
            entityManager.SetComponentData(entity, transform);

            shell.Elapsed = 0f; // 풀에서 재사용되므로 지난 생애의 경과 시간을 반드시 지운다.
            entityManager.SetComponentData(entity, shell);

            // 한 풀을 두 무기가 나눠 쓰므로 색은 쏠 때 정한다. 무엇이 떨어지는지 보고 피하거나 기다릴 수 있게.
            entityManager.SetComponentData(entity, new URPMaterialPropertyBaseColor { Value = ColorOf(shell.Payload) });

            PoolUtility.SetAlive(entityManager, entity, true);
        }

        /// <summary>
        /// 탄 색. 타일 색(<c>TileColors</c>)의 기름(검갈색)·불(주황)과 겹치지 않게, 바닥 위에서 잘 보이는 밝은 색.
        /// </summary>
        private static float4 ColorOf(ShellPayload payload)
        {
            return payload switch
            {
                ShellPayload.Fire => new float4(1f, 0.3f, 0.15f, 1f), // 주홍
                _ => new float4(1f, 0.85f, 0.2f, 1f), // 진한 노랑 (기름)
            };
        }
    }
}
