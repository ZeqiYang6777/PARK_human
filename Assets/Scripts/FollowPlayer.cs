using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target; // 跟随目标
    public float rotateSpeed = 5f; // 视野旋转速度

    private Vector3 offset; // 相机与目标的偏移量

    void Start()
    {
        offset = transform.position - target.position; // 计算相机与目标的初始偏移量
    }

    void Update()
    {
        transform.position = target.position + offset; // 更新相机位置，使其始终保持在目标的偏移位置上

        float mouseX = Input.GetAxis("Mouse X"); // 获取水平方向上鼠标的移动距离
        float rotation = mouseX * rotateSpeed; // 根据鼠标移动距离计算旋转角度
        transform.RotateAround(target.position, Vector3.up, rotation); // 绕目标点旋转相机

        offset = transform.position - target.position; // 更新偏移量，保持相机与目标的相对位置不变
    }
}
