using UnityEngine;
using System.Collections.Generic;

public class PipeGenerator : MonoBehaviour
{
    [Header("管道设置")]
    public GameObject pipeSegmentPrefab;
    public int segmentsCount = 50;
    public float pipeRadius = 2f;
    public float segmentLength = 3f;
    public float curveIntensity = 1f;

    [Header("障碍物")]
    public GameObject[] obstaclePrefabs;
    public float obstacleSpawnChance = 0.3f;

    private List<Transform> pipeSegments = new List<Transform>();
    private Vector3 currentPosition;
    private Vector3 currentDirection;

    void Start()
    {
        currentPosition = Vector3.zero;
        currentDirection = Vector3.forward;
        GeneratePipe();
    }

    void GeneratePipe()
    {
        for (int i = 0; i < segmentsCount; i++)
        {
            // 创建管道段
            GameObject segment = Instantiate(pipeSegmentPrefab, transform);
            segment.transform.position = currentPosition;
            segment.transform.rotation = Quaternion.LookRotation(currentDirection);

            // 设置管道段大小
            segment.transform.localScale = new Vector3(pipeRadius * 2, pipeRadius * 2, segmentLength);

            pipeSegments.Add(segment.transform);

            // 随机生成障碍物
            if (i > 5 && Random.value < obstacleSpawnChance && obstaclePrefabs.Length > 0)
            {
                SpawnObstacle(segment.transform);
            }

            // 更新位置和方向（添加随机弯曲）
            currentPosition += currentDirection * segmentLength;
            currentDirection = GetNextDirection(currentDirection, i);
        }
    }

    Vector3 GetNextDirection(Vector3 currentDir, int segmentIndex)
    {
        // 添加随机弯曲，但保持平滑过渡
        float curveFactor = Mathf.Sin(segmentIndex * 0.2f) * curveIntensity;
        Vector3 newDirection = Quaternion.Euler(
            Random.Range(-curveFactor, curveFactor),
            Random.Range(-curveFactor * 2, curveFactor * 2),
            0
        ) * currentDir;

        return newDirection.normalized;
    }

    void SpawnObstacle(Transform segment)
    {
        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        GameObject obstacle = Instantiate(obstaclePrefab, segment);

        // 在管道内壁随机位置生成障碍物
        float angle = Random.Range(0, 360f);
        Vector3 localPos = Quaternion.Euler(0, 0, angle) * Vector3.right * pipeRadius * 0.8f;
        obstacle.transform.localPosition = localPos;
        obstacle.transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
