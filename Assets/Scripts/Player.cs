using UnityEngine;

public class Player : MonoBehaviour
{
    // ========== 移动参数 ==========
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;

    // ========== 跳跃参数 ==========
    [Header("跳跃设置")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    // ========== 重力参数 ==========
    [Header("重力设置")]
    [SerializeField] private float gravityMultiplier = 2.5f;
    [SerializeField] private float maxFallSpeed = 20f;

    // ========== 地面检测 ==========
    [Header("地面检测")]
    [SerializeField] private Transform checkGround;
    [SerializeField] private LayerMask groupLayer;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float cornerCheckDistance = 0.35f;
    [SerializeField] private bool showGroundDebug = true;

    // ========== 组件引用 ==========
    private Rigidbody rb;

    // ========== 状态变量 ==========
    private bool isGrounded;
    private bool wasGrounded;
    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    private bool isJumping;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 🆕 检查 Rigidbody
        if (rb == null)
        {
            Debug.LogError("❌ Player 缺少 Rigidbody 组件！");
            return;
        }

        Debug.Log($"✅ Rigidbody 已找到: isKinematic={rb.isKinematic}, mass={rb.mass}");

        // 自动创建 CheckGround
        if (checkGround == null)
        {
            GameObject checkGroundObj = new GameObject("CheckGround");
            checkGroundObj.transform.parent = transform;
            checkGroundObj.transform.localPosition = new Vector3(0, -0.95f, 0);
            checkGround = checkGroundObj.transform;

            Debug.Log("✅ 自动创建了 CheckGround 子物体");
        }

        // Rigidbody 配置
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = false;  // 🆕 确保不是 Kinematic

        Debug.Log($"✅ 参数设置: moveSpeed={moveSpeed}, jumpForce={jumpForce}");
    }

    void Update()
    {
        DetectGround();
        HandleMovement();
        HandleJumpInput();
    }

    void FixedUpdate()
    {
        ApplyCustomGravity();
    }

    void DetectGround()
    {
        wasGrounded = isGrounded;

        if (checkGround == null)
        {
            Debug.LogError("⚠️ CheckGround 未设置！");
            return;
        }

        bool mainCheck = Physics.CheckSphere(checkGround.position, groundCheckRadius, groupLayer);

        Vector3[] cornerOffsets = new Vector3[]
        {
            new Vector3(cornerCheckDistance, 0, 0),
            new Vector3(-cornerCheckDistance, 0, 0),
            new Vector3(0, 0, cornerCheckDistance),
            new Vector3(0, 0, -cornerCheckDistance)
        };

        bool cornerCheck = false;
        foreach (Vector3 offset in cornerOffsets)
        {
            if (Physics.CheckSphere(checkGround.position + offset, groundCheckRadius * 0.8f, groupLayer))
            {
                cornerCheck = true;
                break;
            }
        }

        isGrounded = mainCheck || cornerCheck;

        if (isGrounded && !wasGrounded)
        {
            OnLanding();
        }

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (showGroundDebug)
        {
            Debug.DrawRay(checkGround.position, Vector3.down * 0.2f, isGrounded ? Color.green : Color.red);
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 🆕 详细调试信息
        if (horizontal != 0 || vertical != 0)
        {
            Debug.Log($"🎮 输入: H={horizontal:F2}, V={vertical:F2}, moveSpeed={moveSpeed}");
        }

        Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;

        if (movement.magnitude > 0.1f)
        {
            Vector3 targetVelocity = movement * moveSpeed;
            targetVelocity.y = rb.velocity.y;

            // 🆕 显示速度变化
            Debug.Log($"🏃 设置速度: {targetVelocity}, 当前速度: {rb.velocity}");

            rb.velocity = targetVelocity;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(movement),
                Time.deltaTime * 10f
            );
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
            Debug.Log("⌨️ 按下空格！");
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !isJumping)
        {
            PerformJump();
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * jumpCutMultiplier, rb.velocity.z);
        }

        if (isGrounded)
        {
            isJumping = false;
        }
    }

    void PerformJump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;
        isJumping = true;

        Debug.Log($"✅ 跳跃! 力: {jumpForce}");
    }

    void ApplyCustomGravity()
    {
        if (!isGrounded)
        {
            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (gravityMultiplier - 1f) * Time.fixedDeltaTime;
            }

            if (rb.velocity.y < -maxFallSpeed)
            {
                rb.velocity = new Vector3(rb.velocity.x, -maxFallSpeed, rb.velocity.z);
            }
        }
    }

    void OnLanding()
    {
        Debug.Log($"✅ 落地! 速度: {rb.velocity.y:F2}");
    }

    void OnDrawGizmosSelected()
    {
        if (checkGround == null || !showGroundDebug) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkGround.position, groundCheckRadius);

        Gizmos.color = Color.yellow;
        Vector3[] cornerOffsets = new Vector3[]
        {
            new Vector3(cornerCheckDistance, 0, 0),
            new Vector3(-cornerCheckDistance, 0, 0),
            new Vector3(0, 0, cornerCheckDistance),
            new Vector3(0, 0, -cornerCheckDistance)
        };

        foreach (Vector3 offset in cornerOffsets)
        {
            Gizmos.DrawWireSphere(checkGround.position + offset, groundCheckRadius * 0.8f);
        }
    }
}
