using Assets.MyAssets.Scripts.Runtime.Spatial;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.MyAssets.Scripts.Runtime.Combat
{
    /// <summary>
    /// 원 안의 적 전부에게 피해 이벤트를 쓴다. 파편탄 폭발과 착탄형 무기(기름통·화염구)가 함께 쓴다.
    ///
    /// 원래 <c>ProjectileHitJob.Explode</c> 안에 있던 루프를 꺼냈다. 착탄 피해도 똑같이
    /// "해시에서 주변 셀을 훑고, 키 충돌로 섞여 든 적을 거르고, 반경으로 자른다" 라서
    /// 두 벌로 두면 ISSUE-011(키 충돌 중복 집계) 같은 수정이 한쪽에만 들어갈 수 있다.
    ///
    /// Burst 호환 static 이라 잡과 메인 스레드 양쪽에서 부를 수 있다.
    /// </summary>
    public static class AreaDamage
    {
        /// <param name="exclude">피해에서 뺄 적 (폭발의 직격 대상). 없으면 <see cref="Entity.Null"/>.</param>
        public static void Write(
            in NativeParallelMultiHashMap<int, AgentRef> enemies, float2 center, float radius, float damage,
            Entity exclude, ref NativeStream.Writer writer)
        {
            int range = EnemySpatialHash.CellRangeFor(radius);
            int2 centerCell = EnemySpatialHash.CellOf(center);

            for (int y = -range; y <= range; y++)
            {
                for (int x = -range; x <= range; x++)
                {
                    int2 cell = centerCell + new int2(x, y);
                    if (!enemies.TryGetFirstValue(EnemySpatialHash.KeyOf(cell), out AgentRef enemy, out NativeParallelMultiHashMapIterator<int> iterator))
                    {
                        continue;
                    }

                    do
                    {
                        // 키 충돌로 섞여 든 다른 셀의 적을 거른다 — 안 그러면 한 적이 피해를 두 번 받는다 (ISSUE-011).
                        if (enemy.Entity == exclude || !EnemySpatialHash.IsInCell(enemy, cell))
                        {
                            continue;
                        }

                        float reach = radius + enemy.Radius;
                        if (math.distancesq(center, enemy.Position) <= reach * reach)
                        {
                            writer.Write(new DamageEvent
                            {
                                Target = enemy.Entity,
                                Amount = damage,
                            });
                        }
                    }
                    while (enemies.TryGetNextValue(out enemy, ref iterator));
                }
            }
        }
    }
}
