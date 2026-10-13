using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace Assets.MyAssets.DOTS_Study.Scripts.Step06
{
    public partial class Step06StudyInputSystem : SystemBase
    {
        protected override void OnCreate()
        {
            EntityManager.CreateSingleton<Step06StudyInputState>();
        }

        protected override void OnUpdate()
        {
            float2 dir = float2.zero;

            Keyboard keyboard = Keyboard.current;
            if (keyboard is null)
            {
                SystemAPI.SetSingleton(new Step06StudyInputState { Move = dir });
                return;
            }

            if (keyboard.wKey.isPressed)
            {
                dir += new float2(0f, 1f);
            }
            if (keyboard.sKey.isPressed)
            {
                dir += new float2(0f, -1f);
            }
            if (keyboard.aKey.isPressed)
            {
                dir += new float2(-1f, 0f);
            }
            if (keyboard.dKey.isPressed)
            {
                dir += new float2(1f, 0f);
            }

            dir = math.normalizesafe(dir);

            SystemAPI.SetSingleton(new Step06StudyInputState { Move = dir });
        }
    }
}