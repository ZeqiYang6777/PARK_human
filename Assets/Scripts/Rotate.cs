using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotationSpeed = 50f; // 旋转速度

    void Update()
    {
        // 绕Y轴旋转
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
