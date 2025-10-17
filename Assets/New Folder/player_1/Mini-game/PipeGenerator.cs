using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeMeshGenerator : MonoBehaviour
{
    public float pipeRadius = 1f;
    public int pipeSegments = 8; // 环形上的顶点数
    public int segmentsCount = 10; // 路径段数
    public float segmentLength = 2f; // 每段的长度

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        GeneratePipe();
    }

    void GeneratePipe()
    {
        // 生成路径点（这里用直线路径作为示例）
        Vector3[] pathPoints = new Vector3[segmentsCount + 1];
        for (int i = 0; i <= segmentsCount; i++)
        {
            pathPoints[i] = new Vector3(0, 0, i * segmentLength);
        }

        Mesh pipeMesh = CreatePipeMesh(pathPoints);
        meshFilter.mesh = pipeMesh;
    }

    Mesh CreatePipeMesh(Vector3[] pathPoints)
    {
        Mesh mesh = new Mesh();
        int numPoints = pathPoints.Length;
        int numVerticesPerRing = pipeSegments + 1; // 每个环的顶点数，+1是为了闭合环形

        // 计算每个路径点的方向和法线
        Vector3[] directions = new Vector3[numPoints];
        Vector3[] normals = new Vector3[numPoints];
        Vector3[] binormals = new Vector3[numPoints];

        // 计算切线和法线
        for (int i = 0; i < numPoints; i++)
        {
            if (i == 0)
                directions[i] = (pathPoints[i + 1] - pathPoints[i]).normalized;
            else if (i == numPoints - 1)
                directions[i] = (pathPoints[i] - pathPoints[i - 1]).normalized;
            else
                directions[i] = (pathPoints[i + 1] - pathPoints[i - 1]).normalized;

            // 初始法线，可以任意选择，但需要与切线垂直
            normals[i] = Vector3.Cross(directions[i], Vector3.up).normalized;
            if (normals[i].magnitude == 0)
            {
                normals[i] = Vector3.Cross(directions[i], Vector3.forward).normalized;
            }
            binormals[i] = Vector3.Cross(directions[i], normals[i]).normalized;
        }

        // 创建顶点和UV
        Vector3[] vertices = new Vector3[numPoints * numVerticesPerRing];
        Vector2[] uvs = new Vector2[numPoints * numVerticesPerRing];

        for (int i = 0; i < numPoints; i++)
        {
            for (int j = 0; j <= pipeSegments; j++)
            {
                float angle = 2 * Mathf.PI * j / pipeSegments;
                Vector3 circlePoint = normals[i] * Mathf.Cos(angle) + binormals[i] * Mathf.Sin(angle);
                vertices[i * numVerticesPerRing + j] = pathPoints[i] + circlePoint * pipeRadius;
                uvs[i * numVerticesPerRing + j] = new Vector2((float)j / pipeSegments, (float)i / (numPoints - 1));
            }
        }

        // 创建三角形
        int numTriangles = (numPoints - 1) * pipeSegments * 2 * 3; // 每个四边形2个三角形，每个三角形3个顶点
        int[] triangles = new int[numTriangles];
        int triIndex = 0;

        for (int i = 0; i < numPoints - 1; i++)
        {
            for (int j = 0; j < pipeSegments; j++)
            {
                int currentRingIndex = i * numVerticesPerRing;
                int nextRingIndex = (i + 1) * numVerticesPerRing;

                // 第一个三角形
                triangles[triIndex++] = currentRingIndex + j;
                triangles[triIndex++] = nextRingIndex + j;
                triangles[triIndex++] = currentRingIndex + j + 1;

                // 第二个三角形
                triangles[triIndex++] = currentRingIndex + j + 1;
                triangles[triIndex++] = nextRingIndex + j;
                triangles[triIndex++] = nextRingIndex + j + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}