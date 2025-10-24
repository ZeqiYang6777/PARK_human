using UnityEngine;

public class PipeRunnerController : MonoBehaviour
{
    [Header("移动设置")]
    public float forwardSpeed = 5f;
    public float rotationSpeed = 180f;
    public float gravity = 10f;

    [Header("跳跃设置")]
    public float jumpForce = 8f;
    public float groundCheckDistance = 0.2f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Transform currentPipe;
    private float currentAngle = 0f; // 在管道内的角度位置

    // 输入
    private float horizontalInput;
    private bool jumpInput;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        FindNearestPipe();
    }

    void Update()
    {
        GetInput();
        HandleMovement();
        HandleGravityAndJump();
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        jumpInput = Input.GetButtonDown("Jump");
    }

    void FindNearestPipe()
    {
        // 找到最近的管道段
        GameObject[] pipes = GameObject.FindGameObjectsWithTag("Pipe");
        float closestDistance = Mathf.Infinity;

        foreach (GameObject pipe in pipes)
        {
            float distance = Vector3.Distance(transform.position, pipe.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentPipe = pipe.transform;
            }
        }
    }

    void HandleMovement()
    {
        if (currentPipe == null) return;

        // 围绕管道旋转移动
        float pipeCircumference = 2f * Mathf.PI * currentPipe.localScale.x * 0.5f;
        float angularSpeed = (horizontalInput * rotationSpeed) / pipeCircumference;
        currentAngle += angularSpeed * Time.deltaTime;

        // 保持角度在0-360度范围内
        currentAngle = Mathf.Repeat(currentAngle, 360f);

        // 计算在管道内壁的位置
        Vector3 pipeCenter = currentPipe.position;
        Vector3 pipeForward = currentPipe.forward;
        Vector3 pipeRight = currentPipe.right;

        // 计算圆周位置
        Vector3 circumferentialPosition =
            Mathf.Cos(currentAngle * Mathf.Deg2Rad) * pipeRight +
            Mathf.Sin(currentAngle * Mathf.Deg2Rad) * currentPipe.up;

        // 设置角色位置（在管道内壁）
        Vector3 targetPosition = pipeCenter + circumferentialPosition * currentPipe.localScale.x * 0.5f;

        // 向前移动
        targetPosition += pipeForward * forwardSpeed * Time.deltaTime;

        // 应用移动
        controller.Move(targetPosition - transform.position);

        // 旋转角色使其面向管道前进方向并保持正确朝向
        Vector3 lookDirection = Vector3.Cross(circumferentialPosition.normalized, pipeForward).normalized;
        transform.rotation = Quaternion.LookRotation(pipeForward, lookDirection);
    }

    void HandleGravityAndJump()
    {
        // 检测是否接地
        isGrounded = Physics.Raycast(
            transform.position,
            -transform.up,
            groundCheckDistance
        );

        // 处理跳跃
        if (isGrounded)
        {
            velocity.y = -gravity * 0.1f; // 小的向下力确保接地

            if (jumpInput)
            {
                velocity.y = jumpForce;
            }
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        // 应用重力
        controller.Move(velocity * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 切换到新的管道段
        if (other.CompareTag("Pipe"))
        {
            currentPipe = other.transform;
        }

        // 碰撞障碍物
        if (other.CompareTag("Obstacle"))
        {
            HandleObstacleCollision();
        }
    }

    void HandleObstacleCollision()
    {
        // 游戏结束或生命值减少逻辑
        Debug.Log("撞到障碍物！");
        // 这里可以添加游戏结束或重置逻辑
    }
}
