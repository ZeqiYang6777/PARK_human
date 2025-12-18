using UnityEngine;

public class MiniCameraControllerFPS : MonoBehaviour
{
    [Header("玩家身体")]
    public Transform playerBody;

    [Header("鼠标灵敏度")]
    public float mouseSensitivity = 100f;

    [Header("视角限制")]
    public float minVerticalAngle = -90f;
    public float maxVerticalAngle = 90f;

    private float xRotation = 0f;

    void Start()
    {
        // 锁定并隐藏鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ESC解锁鼠标
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 点击重新锁定
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 上下视角（控制摄像机）
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 左右视角（控制玩家身体）
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
