using Assets.MyAssets.DOTS_Study.Scripts.Step05;
using Assets.MyAssets.DOTS_Study.Scripts.Step06;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step07
{
    [BurstCompile]
    public partial struct Step07MappingSystem : ISystem
    {
        private NativeParallelMultiHashMap<int, Entity> _map;
        private const float CellSize = 1f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _map = new(1024, Allocator.Persistent);

            state.RequireForUpdate<Step07PlayerTag>();
            state.RequireForUpdate<Step06DamageEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _map.Clear();
            foreach (var (targetTransform, targetEntity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<Step06Health, Alive>().WithEntityAccess())
            {
                float2 targetPos = new float2(targetTransform.ValueRO.Position.x, targetTransform.ValueRO.Position.y);
                int2 cell = (int2)math.floor(targetPos / CellSize);
                _map.Add((int)math.hash(cell), targetEntity);
            }

            Entity player = SystemAPI.GetSingletonEntity<Step07PlayerTag>();
            float3 playerPos = SystemAPI.GetComponent<LocalTransform>(player).Position;
            DynamicBuffer<Step06DamageEvent> events = SystemAPI.GetSingletonBuffer<Step06DamageEvent>();
            Step06PlayerAttack attack = SystemAPI.GetComponent<Step06PlayerAttack>(player);

            int cellRange = (int)math.ceil(attack.Range / CellSize);
            float2 playerPosition = new(playerPos.x, playerPos.y);
            int2 playerCell = (int2)math.floor(playerPosition / CellSize);

            for (int dy = -cellRange; dy <= cellRange; dy++)
            {
                for (int dx = -cellRange; dx <= cellRange; dx++)
                {
                    int2 cell = playerCell + new int2(dx, dy);
                    int key = (int)math.hash(cell);
                }
            }


            // UnityEngine.Debug.Log(_map.Count());
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _map.Dispose();
        }
    }
}
