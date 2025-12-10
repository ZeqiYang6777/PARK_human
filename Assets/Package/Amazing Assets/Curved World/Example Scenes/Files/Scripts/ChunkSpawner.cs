// Curved World <http://u3d.as/1W8h>
// Copyright (c) Amazing Assets <https://amazingassets.world>

using UnityEngine;

namespace AmazingAssets.CurvedWorld.Examples
{
    public class ChunkSpawner : MonoBehaviour
    {
        public enum Axis { XPositive, XNegative, ZPositive, ZNegative }

        public GameObject[] chunks;
        public int initialSpawnCount = 5;
        public float destroyZone = 300;

        [Space(10)]
        public Axis axis;

        [HideInInspector]
        public Vector3 moveDirection = new Vector3(-1, 0, 0);
        public float movingSpeed = 1;

        public float chunkSize = 60;
        GameObject lastChunk;

        // 障碍物和收集品设置
        [Header("Obstacle and Collectible Settings")]
        public GameObject obstaclePrefab;
        public GameObject collectiblePrefab;
        [Range(0, 1)] public float obstacleSpawnChance = 0.3f;
        [Range(0, 1)] public float collectibleSpawnChance = 0.5f;
        public int obstaclesPerChunk = 2;
        public int collectiblesPerChunk = 3;

        // 游戏状态
        [Header("Game State")]
        public int currentCollectibles = 0;
        public int requiredCollectibles = 10;
        public int currentObstacleHits = 0;
        public int maxObstacleHits = 3;

        void Awake()
        {
            if (initialSpawnCount < chunks.Length)
                initialSpawnCount = chunks.Length;

            int chunkIndex = 0;
            for (int i = 0; i < initialSpawnCount; i++)
            {
                GameObject chunk = InstantiateChunk(chunkIndex, i);
                lastChunk = chunk;

                if (++chunkIndex >= chunks.Length)
                    chunkIndex = 0;
            }
        }

        GameObject InstantiateChunk(int chunkIndex, int chunkNumber)
        {
            GameObject chunk = Instantiate(chunks[chunkIndex]);
            chunk.SetActive(true);

            RunnerChunk runnerChunk = chunk.GetComponent<RunnerChunk>();
            if (runnerChunk != null)
            {
                runnerChunk.spawner = this;
            }

            // 设置区块位置和移动方向
            switch (axis)
            {
                case Axis.XPositive:
                    chunk.transform.localPosition = new Vector3(-chunkNumber * chunkSize, 0, transform.position.z);
                    moveDirection = new Vector3(1, 0, 0);
                    break;
                case Axis.XNegative:
                    chunk.transform.localPosition = new Vector3(chunkNumber * chunkSize, 0, transform.position.z);
                    moveDirection = new Vector3(-1, 0, 0);
                    break;
                case Axis.ZPositive:
                    chunk.transform.localPosition = new Vector3(transform.position.x, 0, -chunkNumber * chunkSize);
                    moveDirection = new Vector3(0, 0, 1);
                    break;
                case Axis.ZNegative:
                    chunk.transform.localPosition = new Vector3(transform.position.x, 0, chunkNumber * chunkSize);
                    moveDirection = new Vector3(0, 0, -1);
                    break;
            }

            // 在区块中生成障碍物和收集品
            SpawnObjectsInChunk(chunk);

            return chunk;
        }

        void SpawnObjectsInChunk(GameObject chunk)
        {
            if (chunk == null) return;

            // 创建对象容器
            Transform objectsContainer = chunk.transform.Find("ObjectsContainer");
            if (objectsContainer == null)
            {
                GameObject container = new GameObject("ObjectsContainer");
                objectsContainer = container.transform;
                objectsContainer.SetParent(chunk.transform);
                objectsContainer.localPosition = Vector3.zero;
            }

            // 生成障碍物
            for (int i = 0; i < obstaclesPerChunk; i++)
            {
                if (obstaclePrefab != null && Random.value < obstacleSpawnChance)
                {
                    SpawnObstacleInChunk(chunk, objectsContainer);
                }
            }

            // 生成收集品
            for (int i = 0; i < collectiblesPerChunk; i++)
            {
                if (collectiblePrefab != null && Random.value < collectibleSpawnChance)
                {
                    SpawnCollectibleInChunk(chunk, objectsContainer);
                }
            }
        }

        void SpawnObstacleInChunk(GameObject chunk, Transform container)
        {
            GameObject obstacle = Instantiate(obstaclePrefab, container);

            Vector3 localPos = new Vector3(
                Random.Range(-chunkSize * 0.4f, chunkSize * 0.4f),
                0.5f,
                Random.Range(-chunkSize * 0.4f, chunkSize * 0.4f)
            );
            obstacle.transform.localPosition = localPos;
        }

        void SpawnCollectibleInChunk(GameObject chunk, Transform container)
        {
            GameObject collectible = Instantiate(collectiblePrefab, container);

            Vector3 localPos = new Vector3(
                Random.Range(-chunkSize * 0.4f, chunkSize * 0.4f),
                1f,
                Random.Range(-chunkSize * 0.4f, chunkSize * 0.4f)
            );
            collectible.transform.localPosition = localPos;
        }

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
                    newPos.z -= chunkSize;
                    break;
                case Axis.ZNegative:
                    newPos.z += chunkSize;
                    break;
            }

            lastChunk = thisChunk.gameObject;
            lastChunk.transform.position = newPos;

            // 回收时重新生成对象
            SpawnObjectsInChunk(lastChunk);
        }

        // 公共方法供其他组件调用
        public void CollectItem()
        {
            currentCollectibles++;
            Debug.Log("Collect Progress: " + currentCollectibles + "/" + requiredCollectibles);

            if (currentCollectibles >= requiredCollectibles)
            {
                EndGame(true);
            }
        }

        public void HitObstacle()
        {
            currentObstacleHits++;
            Debug.Log("Hit Count: " + currentObstacleHits + "/" + maxObstacleHits);

            if (currentObstacleHits >= maxObstacleHits)
            {
                EndGame(false);
            }
        }

        void EndGame(bool isWin)
        {
            if (isWin)
            {
                Debug.Log("Game Win! You collected all items!");
            }
            else
            {
                Debug.Log("Game Over! Too many obstacle hits!");
            }
        }

        public void ResetGameState()
        {
            currentCollectibles = 0;
            currentObstacleHits = 0;
        }
    }
}