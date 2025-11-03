using UnityEngine;

namespace AmazingAssets.CurvedWorld.Examples
{
    public class ChunkSpawner : MonoBehaviour
    {
        public enum Axis { XPositive, XNegative, ZPositive, ZNegative }


        public GameObject[] chunks;
        public int initialSpawnCount = 5;
        public float destoryZone = 300;

        [Space(10)]
        public Axis axis;

        [HideInInspector]
        public Vector3 moveDirection = new Vector3(-1, 0, 0);
        public float movingSpeed = 1;


        public float chunkSize = 60;
        GameObject lastChunk;


        // 收集物和障碍物相关配置
        [Header("收集物与障碍物设置")]
        public GameObject[] collectibles; // 收集物预制体数组
        public GameObject[] obstacles;    // 障碍物预制体数组
        [Range(0, 1)] public float collectibleSpawnRate = 0.5f; // 收集物生成概率
        [Range(0, 1)] public float obstacleSpawnRate = 0.3f;    // 障碍物生成概率
        public float itemYOffset = 1f; // 物品Y轴偏移（避免嵌入地面）


        void Awake()
        {
            // 原有逻辑：保持不变
            initialSpawnCount = initialSpawnCount > chunks.Length ? initialSpawnCount : chunks.Length;

            int chunkIndex = 0;
            for (int i = 0; i < initialSpawnCount; i++)
            {
                GameObject chunk = (GameObject)Instantiate(chunks[chunkIndex]);
                chunk.SetActive(true);

                chunk.GetComponent<RunnerChunk>().spawner = this;

                switch (axis)
                {
                    case Axis.XPositive:
                        chunk.transform.localPosition = new Vector3(-i * chunkSize, 0, transform.position.z);
                        moveDirection = new Vector3(1, 0, 0);
                        break;

                    case Axis.XNegative:
                        chunk.transform.localPosition = new Vector3(i * chunkSize, 0, transform.position.z);
                        moveDirection = new Vector3(-1, 0, 0);
                        break;

                    case Axis.ZPositive:
                        chunk.transform.localPosition = new Vector3(i * chunkSize, 0, transform.position.z);
                        break;

                    case Axis.ZNegative:
                        chunk.transform.localPosition = new Vector3(i * chunkSize, 0, transform.position.z);
                        break;
                }


                lastChunk = chunk;

                // 在初始生成的chunk上生成收集物和障碍物
                SpawnItemsOnChunk(chunk);

                if (++chunkIndex >= chunks.Length)
                    chunkIndex = 0;
            }
        }


        //chunk移动逻辑
        void Update()
        {
            float moveStep = movingSpeed * Time.deltaTime;
            // 遍历所有chunk并移动
            foreach (var chunk in FindObjectsOfType<RunnerChunk>())
            {
                chunk.transform.position += moveDirection * moveStep;
            }
        }


        // 在chunk上生成收集物和障碍物的方法
        void SpawnItemsOnChunk(GameObject chunk)
        {
            // 清除chunk上已有的旧物品（避免回收时重复）
            ClearOldItems(chunk);

            // 生成概率收集物
            if (collectibles.Length > 0 && Random.value < collectibleSpawnRate)
            {
                int randomIdx = Random.Range(0, collectibles.Length);
                GameObject collectible = Instantiate(collectibles[randomIdx], chunk.transform);
                // Chunk随机位置
                collectible.transform.localPosition = GetRandomItemPosition();
            }

            // 概率生成障碍物
            if (obstacles.Length > 0 && Random.value < obstacleSpawnRate)
            {
                int randomIdx = Random.Range(0, obstacles.Length);
                GameObject obstacle = Instantiate(obstacles[randomIdx], chunk.transform);
                // 随机位置（在chunk范围内）
                obstacle.transform.localPosition = GetRandomItemPosition();
            }
        }


        // 运算物品在chunk内的随机位置
        Vector3 GetRandomItemPosition()
        {
            // 根据轴方向限制物品生成范围
            float xRange = chunkSize / 2;
            float zRange = chunkSize / 2;

            return new Vector3(
                Random.Range(-xRange, xRange),
                itemYOffset,
                Random.Range(-zRange, zRange)
            );
        }


        // 清除chunk上的旧物品（回收时调用）
        void ClearOldItems(GameObject chunk)
        {
            // 遍历子物体，删除带有Collectible或Obstacle组件的物体
            foreach (Transform child in chunk.transform)
            {
                if (child.GetComponent<Collectible>() != null || child.GetComponent<Obstacle>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }


        // DestroyChunk方法保持不变，仅在最后添加物品生成
        public void DestroyChunk(RunnerChunk thisChunk)
        {
            Vector3 newPos = lastChunk.transform.position;
            switch (axis)
            {
                case Axis.XPositive:
                    newPos.x -= chunkSize;
                    break;

                case Axis.XNegative:
                    newPos.x += chunkSize;
                    break;

                case Axis.ZPositive:
                    break;

                case Axis.ZNegative:
                    break;
            }

            lastChunk = thisChunk.gameObject;
            lastChunk.transform.position = newPos;

            // 回收chunk时重新生成物品
            SpawnItemsOnChunk(lastChunk);
        }
    }
}