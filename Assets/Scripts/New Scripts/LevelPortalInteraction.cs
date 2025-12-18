using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LevelPortalInteraction : MonoBehaviour
{
    [Header("===== 场景设置 =====")]
    [Tooltip("下一个场景的名称")]
    public string nextSceneName = "Zone2";

    [Header("===== 交互设置 =====")]
    [Tooltip("交互按键")]
    public KeyCode interactKey = KeyCode.E;

    [Header("===== UI 提示 =====")]
    [Tooltip("拖入提示文本对象")]
    public GameObject promptUI;

    [Tooltip("提示文本组件 (可选,用于动态修改文字)")]
    public TextMeshProUGUI promptText;
    public GameObject loadingCircle;
    [Tooltip("提示内容")]
    public string promptMessage = "Press E to interact.";

    [Header("===== 可选设置 =====")]
    [Tooltip("传送前的延迟时间")]
    public float teleportDelay = 0.3f;

    [Tooltip("传送音效")]
    public AudioClip teleportSound;

    [Header("===== 调试信息 =====")]
    [Tooltip("显示调试日志")]
    public bool showDebugLogs = true;

    // 私有变量
    private bool playerInRange = false;
    private bool isActivated = false;
    private AudioSource audioSource;

    void Start()
    {
        // 初始化音效
        if (teleportSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 确保提示一开始是隐藏的
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

        // 设置提示文字
        if (promptText != null)
        {
            promptText.text = promptMessage;
        }

        if (showDebugLogs)
        {
            Debug.Log(" LevelPortalInteraction 已初始化\n" +
                     "目标场景: {nextSceneName}\n" +
                     "交互按键: {interactKey}");
        }
    }

    void Update()
    {
        
        if (playerInRange && !isActivated)
        {
            if (Input.GetKeyDown(interactKey))
            {
                if (showDebugLogs)
                {
                    Debug.Log(" 玩家按下 {interactKey} 键,准备传送!");
                }

                StartCoroutine(TeleportToNextLevel());
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            playerInRange = true;

            // 显示提示
            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }

            // 显示加载圈
            if (loadingCircle != null)
            {
                loadingCircle.SetActive(true);
            }

            if (showDebugLogs)
            {
                Debug.Log(" 玩家进入传送门范围,显示提示 UI");
            }
        }
    }


    // 玩家离开触发区域
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // 隐藏提示
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }

            // 隐藏加载圈
            if (loadingCircle != null)
            {
                loadingCircle.SetActive(false);
            }

            if (showDebugLogs)
            {
                Debug.Log(" 玩家离开传送门范围,隐藏提示 UI");
            }
        }
    }

    // 传送协程
    IEnumerator TeleportToNextLevel()
    {
        isActivated = true;

        // 隐藏提示
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
        // 隐藏加载圈
        if (loadingCircle != null)
        {
            loadingCircle.SetActive(false);
        }

        // 播放音效
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }

        // 延迟
        if (teleportDelay > 0)
        {
            if (showDebugLogs)
            {
                Debug.Log(" 等待 {teleportDelay} 秒后传送...");
            }
            yield return new WaitForSeconds(teleportDelay);
        }

        // 加载场景
        if (showDebugLogs)
        {
            Debug.Log(" 正在加载场景: {nextSceneName}");
        }

        SceneManager.LoadScene(nextSceneName);
    }

    
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);
        }
    }
}
