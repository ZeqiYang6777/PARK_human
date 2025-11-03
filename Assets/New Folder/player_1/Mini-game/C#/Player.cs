// Curved World <http://u3d.as/1W8h>
// Copyright (c) Amazing Assets <https://amazingassets.world>

using UnityEngine;

#if USE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


namespace AmazingAssets.CurvedWorld.Examples
{
    public class RunnerPlayer : MonoBehaviour
    {
        public enum Side { Left, Right }


        Vector3 initialPosition;
        Vector3 newPos;
        Side side;

        // 跳跃相关变量
        private bool 正在跳跃 = false;
        private float 跳跃计时器 = 0f;
        public float 跳跃高度 = 2f;
        public float 跳跃持续时间 = 0.5f;
        private Vector3 跳跃初始位置;


#if USE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
        Key moveLeftKey = Key.A;
        Key moveRightKey = Key.D;
        Key jumpKey = Key.Space;
#else
        KeyCode moveLeftKey = KeyCode.A;
        KeyCode moveRightKey = KeyCode.D;
        KeyCode jumpKey = KeyCode.Space;
#endif

        Animation animationComp;
        public AnimationClip moveLeftAnimation;
        public AnimationClip moveRightAnimation;

        float translateOffset = 3.5f;


        void Start()
        {
            initialPosition = transform.position;

            side = Side.Left;
            newPos = transform.localPosition + new Vector3(0, 0, translateOffset);

            animationComp = GetComponent<Animation>();

            // 记录初始Y位置用于跳跃
            跳跃初始位置 = transform.position;
        }

        void Update()
        {
            // 处理左右移动（修改：移除跳跃状态检查，允许跳跃时移动）
            if (ExampleInput.GetKeyDown(moveLeftKey))
            {
                if (side == Side.Right)
                {
                    newPos = initialPosition + new Vector3(0, 0, translateOffset);
                    side = Side.Left;

                    if (animationComp != null && moveLeftAnimation != null)
                        animationComp.Play(moveLeftAnimation.name);
                }
            }
            else if (ExampleInput.GetKeyDown(moveRightKey))
            {
                if (side == Side.Left)
                {
                    newPos = initialPosition + new Vector3(0, 0, -translateOffset);
                    side = Side.Right;

                    if (animationComp != null && moveRightAnimation != null)
                        animationComp.Play(moveRightAnimation.name);
                }
            }

            // 处理跳跃输入（新增代码）
            if (ExampleInput.GetKeyDown(jumpKey) && !正在跳跃)
            {
                开始跳跃();
            }

            // 处理跳跃过程（新增代码）
            if (正在跳跃)
            {
                处理跳跃();
            }

            // 应用移动（修改：简化位置计算）
            应用移动();
        }

        // 跳跃方法
        void 开始跳跃()
        {
            正在跳跃 = true;
            跳跃计时器 = 0f;
        }

        // 跳跃处理逻辑
        void 处理跳跃()
        {
            跳跃计时器 += Time.deltaTime;

            // 计算跳跃曲线（正弦曲线，平滑的跳跃）
            float 跳跃进度 = 跳跃计时器 / 跳跃持续时间;
            float 跳跃高度比例 = Mathf.Sin(跳跃进度 * Mathf.PI);

            // 应用Y轴位置
            Vector3 当前位置 = transform.localPosition;
            当前位置.y = 跳跃初始位置.y + 跳跃高度比例 * 跳跃高度;
            transform.localPosition = 当前位置;

            // 跳跃结束
            if (跳跃计时器 >= 跳跃持续时间)
            {
                正在跳跃 = false;
                跳跃计时器 = 0f;

                // 确保回到准确的高度
                Vector3 结束位置 = transform.localPosition;
                结束位置.y = 跳跃初始位置.y;
                transform.localPosition = 结束位置;
            }
        }

        // 新增：应用移动的方法
        void 应用移动()
        {
            Vector3 目标位置 = newPos;

            // 如果正在跳跃，保持当前的Y轴位置
            if (正在跳跃)
            {
                目标位置.y = transform.localPosition.y;
            }
            else
            {
                目标位置.y = 跳跃初始位置.y;
            }

            // 应用移动（X和Z轴始终可以移动，Y轴根据跳跃状态决定）
            transform.localPosition = Vector3.Lerp(transform.localPosition, 目标位置, Time.deltaTime * 10);
        }
    }
}