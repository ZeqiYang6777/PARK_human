using UnityEngine;

namespace AmazingAssets.CurvedWorld.Examples
{
    public class RunnerChunk : MonoBehaviour
    {
        public ChunkSpawner spawner;

        void Update()
        {
            if (spawner == null) return;

            // 移动区块
            transform.Translate(spawner.moveDirection * spawner.movingSpeed * Time.deltaTime);

            // 检查是否超出销毁范围
            switch (spawner.axis)
            {
                case ChunkSpawner.Axis.XPositive:
                    if (transform.position.x > spawner.destroyZone)
                        spawner.DestroyChunk(this);
                    break;
                case ChunkSpawner.Axis.XNegative:
                    if (transform.position.x < -spawner.destroyZone)
                        spawner.DestroyChunk(this);
                    break;
                case ChunkSpawner.Axis.ZPositive:
                    if (transform.position.z > spawner.destroyZone)
                        spawner.DestroyChunk(this);
                    break;
                case ChunkSpawner.Axis.ZNegative:
                    if (transform.position.z < -spawner.destroyZone)
                        spawner.DestroyChunk(this);
                    break;
            }
        }
    }
}