using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    [Header("设置")]
    public QuestUIManager questUIManager;    // 任务管理器引用
    [Range(0, 5)]
    public int targetQuestIndex = 1;         // 要更新到的任务索引（0-5）
    public KeyCode interactKey = KeyCode.E;  // 交互按键

    [Header("提示")]
    public GameObject interactHint;          // 交互提示UI（可选）

    [Header("调试")]
    public bool showDebugInfo = true;

    private bool playerInRange = false;
    private bool hasTriggered = false;       // 防止重复触发

    void Start()
    {
        if (interactHint != null)
        {
            interactHint.SetActive(false);
        }

        // 自动查找任务管理器
        if (questUIManager == null)
        {
            questUIManager = FindObjectOfType<QuestUIManager>();
            if (questUIManager == null)
            {
                Debug.LogError($"[{gameObject.name}] 未找到任务管理器！");
            }
        }
    }

    void Update()
    {
        // 玩家在范围内且按下交互键
        if (playerInRange && !hasTriggered && Input.GetKeyDown(interactKey))
        {
            TriggerQuestUpdate();
        }
    }

    void TriggerQuestUpdate()
    {
        if (questUIManager != null)
        {
            // 更新任务图片
            questUIManager.UpdateQuestImage(targetQuestIndex);

            hasTriggered = true;

            if (interactHint != null)
            {
                interactHint.SetActive(false);
            }

            if (showDebugInfo)
                Debug.Log($"✅ 任务已更新到索引：{targetQuestIndex + 1}/6");
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] 任务管理器未设置！");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            playerInRange = true;

            if (interactHint != null)
            {
                interactHint.SetActive(true);
            }

            if (showDebugInfo)
                Debug.Log($"[{gameObject.name}] 玩家进入范围，按 {interactKey} 键交互");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactHint != null)
            {
                interactHint.SetActive(false);
            }

            if (showDebugInfo)
                Debug.Log($"[{gameObject.name}] 玩家离开范围");
        }
    }

    // Scene视图显示触发范围
    void OnDrawGizmos()
    {
        Color gizmoColor = hasTriggered ? new Color(0.5f, 0.5f, 0.5f, 0.3f) : new Color(0f, 1f, 0f, 0.3f);
        Gizmos.color = gizmoColor;

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            Gizmos.color = hasTriggered ? Color.gray : Color.green;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            Gizmos.DrawSphere(transform.position + sphereCollider.center, sphereCollider.radius);
            Gizmos.color = hasTriggered ? Color.gray : Color.green;
            Gizmos.DrawWireSphere(transform.position + sphereCollider.center, sphereCollider.radius);
        }
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = hasTriggered ? Color.gray : Color.green;

        string statusText = hasTriggered ? "✓ 已触发" : $"○ 按 {interactKey} 交互";
        string questText = $"任务 {targetQuestIndex + 1}/6";
        string infoText = $"{gameObject.name}\n{statusText}\n{questText}";

        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, infoText, style);
#endif
    }
}
