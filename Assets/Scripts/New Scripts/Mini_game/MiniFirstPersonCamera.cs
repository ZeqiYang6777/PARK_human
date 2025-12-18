using UnityEngine;

public class MiniFirstPersonCamera : MonoBehaviour
{
    [Header("鼠标灵敏度")]
    public float mouseSensitivity = 2f;

    [Header("视角限制")]
    public float minVerticalAngle = -80f; // 向下看的最大角度
    public float maxVerticalAngle = 80f;  // 向上看的最大角度

    [Header("玩家身体")]
    public Transform playerBody; // 玩家身体对象

    private float xRotation = 0f; // 当前上下旋转角度

    void Start()
    {
        // 锁定并隐藏鼠标光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 按ESC键解锁鼠标（用于调试）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 点击鼠标左键重新锁定
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 获取鼠标移动输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 处理上下视角（摄像机自身旋转）
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

        // 应用摄像机上下旋转
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 处理左右视角（玩家身体旋转）
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
