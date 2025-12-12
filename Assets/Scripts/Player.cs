using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("地面检测")]
    public Transform checkGround;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.5f;
    [Tooltip("启用调试信息")]
    public bool showDebugInfo = true; // ✅ 添加调试开关

    [Header("UI组件")]
    public Image failPanel;
    public Image winPanel;
    public Text jewelText;
    public Text countdownText;

    [Header("游戏设置")]
    public float countdownTime = 120f;
    public float fallThreshold = -2f;

    private bool isGrounded = false;
    private Animator animator;
    private bool isCanMove = true;
    private AudioSource audioSource;
    private float currentTime;
    private int getJewelCount = 0;
    private int allJewelCount;
    private bool gameEnded = false;
    private Rigidbody rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        // ✅ 检查 Rigidbody 设置
        if (rb != null)
        {
            rb.freezeRotation = true; // 防止旋转翻倒
            Debug.Log("✅ Rigidbody 设置完成");
        }
        else
        {
            Debug.LogError("❌ Player 缺少 Rigidbody 组件！请添加 Rigidbody");
        }
    }

    private void Start()
    {
        SetupGroundCheck();
        ValidateUIComponents();

        if (failPanel != null) failPanel.gameObject.SetActive(false);
        if (winPanel != null) winPanel.gameObject.SetActive(false);

        currentTime = countdownTime;
        allJewelCount = GameObject.FindGameObjectsWithTag("Jewel").Length;

        UpdateJewelUI();
        UpdateCountdownUI();

        // ✅ 检查 Layer 设置
        CheckLayerSetup();
    }

    // ✅ 检查 Layer 配置
    private void CheckLayerSetup()
    {
        if (groundLayer == 0)
        {
            Debug.LogWarning("⚠️ Ground Layer 未设置！将使用默认检测方式");
            // 使用默认的 Default 层
            groundLayer = LayerMask.GetMask("Default");
        }
        else
        {
            Debug.Log($"✅ Ground Layer 已设置: {LayerMask.LayerToName((int)Mathf.Log(groundLayer.value, 2))}");
        }
    }

    private void SetupGroundCheck()
    {
        if (checkGround == null)
        {
            Transform existingCheck = transform.Find("GroundCheck");

            if (existingCheck != null)
            {
                checkGround = existingCheck;
            }
            else
            {
                GameObject groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.SetParent(transform);
                // ✅ 调整位置到角色底部（根据碰撞体大小）
                Collider col = GetComponent<Collider>();
                float yOffset = col != null ? -col.bounds.extents.y : -1f;
                groundCheckObj.transform.localPosition = new Vector3(0, yOffset, 0);
                checkGround = groundCheckObj.transform;
                Debug.Log($"✅ 已自动创建 GroundCheck，位置: {yOffset}");
            }
        }
    }

    private void ValidateUIComponents()
    {
        if (failPanel == null) Debug.LogWarning("⚠️ failPanel 未赋值");
        if (winPanel == null) Debug.LogWarning("⚠️ winPanel 未赋值");
        if (jewelText == null) Debug.LogWarning("⚠️ jewelText 未赋值");
        if (countdownText == null) Debug.LogWarning("⚠️ countdownText 未赋值");
    }

    void Update()
    {
        if (!isCanMove) return;

        HandleMovement();
        HandleJump();
        UpdateCountdown();
        CheckFall();
    }

    private void HandleMovement()
    {
        if (animator == null) return;

        Transform cameraTransform = Camera.main.transform;
        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        if (inputDirection != Vector3.zero)
        {
            Vector3 movementDirection = cameraTransform.forward * inputDirection.z + cameraTransform.right * inputDirection.x;
            movementDirection.y = 0;

            transform.rotation = Quaternion.LookRotation(movementDirection);
            transform.position += movementDirection * moveSpeed * Time.deltaTime;

            animator.SetFloat("MoveSpeed", inputDirection.magnitude);
        }
        else
        {
            animator.SetFloat("MoveSpeed", 0f);
        }
    }

    private void HandleJump()
    {
        if (checkGround == null || rb == null) return;

        // ✅ 地面检测（使用两种方式增加可靠性）
        isGrounded = Physics.CheckSphere(checkGround.position, groundCheckRadius, groundLayer);

        // ✅ 备用检测方式（射线检测）
        if (!isGrounded)
        {
            RaycastHit hit;
            isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckRadius + 0.1f, groundLayer);

            if (showDebugInfo && isGrounded)
            {
                Debug.Log($"🟢 射线检测到地面: {hit.collider.name}");
            }
        }

        // ✅ 显示调试信息
        if (showDebugInfo && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"🔵 跳跃尝试 - 是否在地面: {isGrounded}, CheckGround位置: {checkGround.position}");

            // 显示周围的碰撞体
            Collider[] colliders = Physics.OverlapSphere(checkGround.position, groundCheckRadius);
            Debug.Log($"检测范围内的碰撞体数量: {colliders.Length}");
            foreach (var col in colliders)
            {
                Debug.Log($"  - {col.name}, Layer: {LayerMask.LayerToName(col.gameObject.layer)}");
            }
        }

        // ✅ 执行跳跃
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // 重置垂直速度（防止累积）
            Vector3 velocity = rb.velocity;
            velocity.y = 0;
            rb.velocity = velocity;

            // 施加跳跃力
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }

            if (showDebugInfo)
            {
                Debug.Log($"✅ 跳跃执行！力度: {jumpForce}");
            }
        }
    }

    private void UpdateCountdown()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateCountdownUI();
        }
        else if (!gameEnded)
        {
            GameOver();
        }
    }

    private void CheckFall()
    {
        if (transform.position.y < fallThreshold && !gameEnded)
        {
            GameOver();
        }
    }

    private void UpdateJewelUI()
    {
        if (jewelText != null)
        {
            jewelText.text = $"已收集宝石：{getJewelCount}/{allJewelCount}";
        }
    }

    private void UpdateCountdownUI()
    {
        if (countdownText != null)
        {
            int timeLeft = Mathf.Max(0, Mathf.FloorToInt(currentTime));
            countdownText.text = $"倒计时：{timeLeft}";
        }
    }

    private void GameOver()
    {
        gameEnded = true;
        if (failPanel != null)
        {
            failPanel.gameObject.SetActive(true);
        }
        isCanMove = false;
        Debug.Log("游戏结束 - 失败");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Jewel"))
        {
            Destroy(other.gameObject);

            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.PlayOneShot(audioSource.clip);
            }

            getJewelCount++;
            UpdateJewelUI();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Goal"))
        {
            if (getJewelCount >= allJewelCount)
            {
                if (winPanel != null)
                {
                    winPanel.gameObject.SetActive(true);
                }
                isCanMove = false;
                gameEnded = true;
                Debug.Log("游戏结束 - 胜利！");
            }
        }
    }

    // ✅ 在 Scene 视图中显示地面检测范围
    private void OnDrawGizmos()
    {
        if (checkGround != null)
        {
            // 绿色 = 在地面，红色 = 在空中
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkGround.position, groundCheckRadius);

            // 绘制射线
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (groundCheckRadius + 0.1f));
        }
    }
}
