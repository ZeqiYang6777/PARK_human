using UnityEngine;

public class MiniPlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float gravity = -20f;

    [Header("地面检测")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("摄像机引用")]
    public Transform playerCamera;

    [Header("音效")]
    public AudioClip coinSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    private AudioSource audioSource;

    [Header("粒子")]
    public ParticleSystem jumpEffect;
    public ParticleSystem landEffect;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;
    private float timeInAir = 0f;
    private bool hasWarned = false; 

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        // 检查游戏是否结束
        if (MiniGameManager.Instance != null && MiniGameManager.Instance.IsGameEnded())
        {
            return;
        }

        // 地面检测
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // 落地检测
        if (isGrounded && !wasGrounded)
        {
            OnLand();
        }

        // 地面状态处理
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            timeInAir = 0f;
            hasWarned = false;
        }
        else if (!isGrounded)
        {
            timeInAir += Time.deltaTime;

            // 2秒时警告
            if (timeInAir >= 2f && !hasWarned)
            {
                hasWarned = true;
                Debug.Log("警告：About to fall into the void！");
                // 可以在这里触发警告UI或音效
            }

            // 3秒后游戏结束
            if (timeInAir >= 3f)
            {
                if (MiniGameManager.Instance != null)
                {
                    MiniGameManager.Instance.GameOver("into the void");
                }
            }
        }

        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 基于摄像机方向的移动
        Vector3 forward = playerCamera.forward;
        Vector3 right = playerCamera.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // 计算移动方向
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
        Vector3 movement = moveDirection * moveSpeed;

        // 应用移动
        controller.Move(movement * Time.deltaTime);

        // 跳跃
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = jumpForce;
            OnJump();
        }

        // 应用重力
        velocity.y += gravity * Time.deltaTime;

        // 应用垂直移动
        controller.Move(velocity * Time.deltaTime);
    }

    void OnJump()
    {
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }

        if (jumpEffect != null)
        {
            jumpEffect.Play();
        }
    }

    void OnLand()
    {
        if (landSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(landSound);
        }

        if (landEffect != null)
        {
            landEffect.Play();
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 检测金币
        if (hit.gameObject.CompareTag("Coin"))
        {
            if (coinSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(coinSound);
            }

            if (MiniGameManager.Instance != null)
            {
                MiniGameManager.Instance.CollectCoin();
            }

            Destroy(hit.gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
