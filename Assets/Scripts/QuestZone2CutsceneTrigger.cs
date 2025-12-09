using UnityEngine;
using Cinemachine;
using System.Collections;
using PixelCrushers.DialogueSystem;

public class QuestZone2CutsceneTrigger : MonoBehaviour
{
    [Header("=== Cinemachine相机设置 ===")]
    [Tooltip("镜头1的虚拟相机")]
    public CinemachineVirtualCamera vcamShot1;

    [Tooltip("镜头2的虚拟相机")]
    public CinemachineVirtualCamera vcamShot2;

    [Tooltip("镜头3的虚拟相机")]
    public CinemachineVirtualCamera vcamShot3;

    [Tooltip("玩家的虚拟相机")]
    public CinemachineVirtualCamera playerVirtualCamera;

    [Header("=== 动画控制器 ===")]
    [Tooltip("镜头1的Animator")]
    public Animator shot1Animator;

    [Tooltip("镜头2的Animator")]
    public Animator shot2Animator;

    [Tooltip("镜头3的Animator")]
    public Animator shot3Animator;

    [Header("=== 镜头时长设置 ===")]
    [Tooltip("镜头1显示时长（秒）")]
    public float shot1Duration = 5f;

    [Tooltip("镜头2显示时长（秒）")]
    public float shot2Duration = 5f;

    [Tooltip("镜头3显示时长（秒）")]
    public float shot3Duration = 5f;

    [Header("=== 玩家控制 ===")]
    [Tooltip("玩家对象")]
    public GameObject player;

    [Tooltip("玩家移动脚本")]
    public MonoBehaviour playerMovementScript;

    [Tooltip("玩家相机控制脚本")]
    public MonoBehaviour playerCameraScript;

    [Header("=== 任务系统设置 ===")]
    [Tooltip("任务UI管理器")]
    public QuestUIManager questUIManager;

    [Tooltip("过场后显示的任务索引（0-5对应任务1-6）")]
    public int questIndexAfterCutscene = 1; // 注意：索引从0开始，1代表第2个任务

    [Tooltip("过场后播放的对话")]
    public string conversationAfterCutscene = "QuestZone2_Narration";

    [Tooltip("✅ 等待旁白播放完才显示任务")]
    public bool waitForNarrationBeforeQuest = true;

    [Tooltip("🎯 旁白后自动打开任务面板")]
    public bool autoOpenQuestPanel = false;

    [Header("=== 相机过渡设置 ===")]
    [Tooltip("相机切换混合时长")]
    public float blendDuration = 1f;

    [Tooltip("使用平滑混合")]
    public bool useSmoothBlend = true;

    [Header("=== 其他设置 ===")]
    [Tooltip("只触发一次")]
    public bool triggerOnce = true;

    [Tooltip("触发后禁用碰撞器")]
    public bool disableColliderAfterTrigger = true;

    [Tooltip("显示调试信息")]
    public bool showDebugInfo = true;

    [Header("=== 音效设置 ===")]
    [Tooltip("过场开始音效")]
    public AudioClip cutsceneStartSound;

    [Tooltip("过场结束音效")]
    public AudioClip cutsceneEndSound;

    [Tooltip("音效音量")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    // 内部状态
    private bool hasTriggered = false;
    private bool isPlayingCutscene = false;
    private int originalShot1Priority;
    private int originalShot2Priority;
    private int originalShot3Priority;
    private int originalPlayerPriority;
    private AudioSource audioSource;

    private void Start()
    {
        // 验证所有必需的引用
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        // 添加AudioSource组件（如果需要）
        if (cutsceneStartSound != null || cutsceneEndSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
        }

        // 保存所有相机的原始优先级
        originalShot1Priority = vcamShot1.Priority;
        originalShot2Priority = vcamShot2.Priority;
        originalShot3Priority = vcamShot3.Priority;
        originalPlayerPriority = playerVirtualCamera.Priority;

        // 确保所有镜头相机初始优先级为0
        vcamShot1.Priority = 0;
        vcamShot2.Priority = 0;
        vcamShot3.Priority = 0;

        // 确保玩家相机优先级最高
        playerVirtualCamera.Priority = 10;

        // 禁用所有Animator，等待触发时启用
        if (shot1Animator != null) shot1Animator.enabled = false;
        if (shot2Animator != null) shot2Animator.enabled = false;
        if (shot3Animator != null) shot3Animator.enabled = false;

        if (showDebugInfo)
        {
            Debug.Log($"✅ [QuestZone_2] 初始化完成! 等待玩家触发...");
        }
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (vcamShot1 == null)
        {
            Debug.LogError("❌ 未设置镜头1的虚拟相机!", this);
            isValid = false;
        }

        if (vcamShot2 == null)
        {
            Debug.LogError("❌ 未设置镜头2的虚拟相机!", this);
            isValid = false;
        }

        if (vcamShot3 == null)
        {
            Debug.LogError("❌ 未设置镜头3的虚拟相机!", this);
            isValid = false;
        }

        if (playerVirtualCamera == null)
        {
            Debug.LogError("❌ 未设置玩家的虚拟相机!", this);
            isValid = false;
        }

        if (player == null)
        {
            Debug.LogError("❌ 未设置玩家对象!", this);
            isValid = false;
        }

        if (questUIManager == null)
        {
            Debug.LogWarning("⚠️ 未设置QuestUIManager,将无法显示任务!", this);
        }

        return isValid;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 检查是否是玩家进入
        if (other.gameObject != player) return;

        // 检查是否已经触发过
        if (triggerOnce && hasTriggered) return;

        // 检查是否正在播放过场
        if (isPlayingCutscene) return;

        // 标记已触发
        hasTriggered = true;

        // 开始播放过场动画
        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        isPlayingCutscene = true;

        if (showDebugInfo)
        {
            Debug.Log("🎬 ========== [QuestZone_2] 过场动画开始 ==========");
        }

        // 播放开始音效
        PlaySound(cutsceneStartSound);

        // 禁用玩家控制
        DisablePlayerControl();

        // ========== 播放镜头1 ==========
        yield return StartCoroutine(PlayShot(vcamShot1, shot1Animator, shot1Duration, 1));

        // ========== 播放镜头2 ==========
        yield return StartCoroutine(PlayShot(vcamShot2, shot2Animator, shot2Duration, 2));

        // ========== 播放镜头3 ==========
        yield return StartCoroutine(PlayShot(vcamShot3, shot3Animator, shot3Duration, 3));

        // ========== 所有镜头播放完毕,切回玩家相机 ==========
        if (showDebugInfo)
        {
            Debug.Log("✅ 所有镜头播放完毕,准备切回玩家相机");
        }

        // 降低所有镜头相机优先级
        vcamShot1.Priority = 0;
        vcamShot2.Priority = 0;
        vcamShot3.Priority = 0;

        // 提高玩家相机优先级
        playerVirtualCamera.Priority = 100;

        // 等待相机混合完成
        yield return new WaitForSeconds(blendDuration);

        // 恢复玩家控制
        EnablePlayerControl();

        // 播放结束音效
        PlaySound(cutsceneEndSound);

        if (showDebugInfo)
        {
            Debug.Log("✅ 过场结束,恢复游戏控制");
        }

        // ========== 现在才播放旁白! ==========
        if (!string.IsNullOrEmpty(conversationAfterCutscene))
        {
            if (showDebugInfo)
            {
                Debug.Log($"💬 准备播放旁白: {conversationAfterCutscene}");
            }

            // 播放旁白对话
            DialogueManager.StartConversation(conversationAfterCutscene);

            // 如果需要等待旁白播放完
            if (waitForNarrationBeforeQuest)
            {
                if (showDebugInfo)
                {
                    Debug.Log("⏳ 等待旁白播放完成...");
                }

                // 等待对话系统完成
                while (DialogueManager.isConversationActive)
                {
                    yield return null;
                }

                if (showDebugInfo)
                {
                    Debug.Log("✅ 旁白播放完成");
                }
            }
        }

        // ========== 显示任务（使用正确的方法） ==========
        if (questUIManager != null)
        {
            // ✅ 方案1：只更新任务图片（推荐）
            questUIManager.UpdateQuestImage(questIndexAfterCutscene);

            if (showDebugInfo)
            {
                Debug.Log($"✅ 已更新任务图片为索引: {questIndexAfterCutscene}（任务 {questIndexAfterCutscene + 1}）");
            }

            // ✅ 方案2：如果想自动打开任务面板（可选）
            if (autoOpenQuestPanel)
            {
                // 等待一小段时间，让玩家准备好
                yield return new WaitForSeconds(0.5f);

                questUIManager.OpenQuestPanel();

                if (showDebugInfo)
                {
                    Debug.Log("📋 已自动打开任务面板");
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log("💡 提示：按M键打开任务面板查看新任务");
                }
            }
        }

        // 禁用碰撞器（如果设置了）
        if (disableColliderAfterTrigger)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
                if (showDebugInfo)
                {
                    Debug.Log("🚫 已禁用触发器碰撞器");
                }
            }
        }

        isPlayingCutscene = false;

        if (showDebugInfo)
        {
            Debug.Log("🎬 ========== [QuestZone_2] 完整流程结束 ==========");
        }
    }

    private IEnumerator PlayShot(CinemachineVirtualCamera vcam, Animator animator, float duration, int shotNumber)
    {
        if (showDebugInfo)
        {
            Debug.Log($"🎥 播放镜头{shotNumber}");
        }

        // 降低其他所有相机的优先级
        vcamShot1.Priority = 0;
        vcamShot2.Priority = 0;
        vcamShot3.Priority = 0;
        playerVirtualCamera.Priority = 0;

        // 提高当前镜头相机的优先级
        vcam.Priority = 100;

        if (showDebugInfo)
        {
            Debug.Log($"📷 已激活相机: {vcam.name}, Priority={vcam.Priority}");
        }

        // 等待混合完成
        yield return new WaitForSeconds(blendDuration);

        // 启用并播放动画
        if (animator != null)
        {
            animator.enabled = true;
            animator.Play(0);

            if (showDebugInfo)
            {
                Debug.Log($"🎬 镜头{shotNumber}动画开始播放");
            }
        }

        // 等待镜头时长
        yield return new WaitForSeconds(duration);

        // 停止动画
        if (animator != null)
        {
            animator.enabled = false;

            if (showDebugInfo)
            {
                Debug.Log($"⏸️ 镜头{shotNumber}动画停止");
            }
        }
    }

    private void DisablePlayerControl()
    {
        // 禁用玩家移动脚本
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
            if (showDebugInfo) Debug.Log("🚫 已禁用玩家移动");
        }

        // 禁用玩家相机控制脚本
        if (playerCameraScript != null)
        {
            playerCameraScript.enabled = false;
            if (showDebugInfo) Debug.Log("🚫 已禁用玩家相机控制");
        }
    }

    private void EnablePlayerControl()
    {
        // 启用玩家移动脚本
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
            if (showDebugInfo) Debug.Log("✅ 已启用玩家移动");
        }

        // 启用玩家相机控制脚本
        if (playerCameraScript != null)
        {
            playerCameraScript.enabled = true;
            if (showDebugInfo) Debug.Log("✅ 已启用玩家相机控制");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }

    [ContextMenu("▶️ 测试播放完整流程")]
    private void TestPlayCutscene()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("⚠️ 请在Play模式下测试!");
            return;
        }

        if (isPlayingCutscene)
        {
            Debug.LogWarning("⚠️ 过场动画正在播放中!");
            return;
        }

        StartCoroutine(PlayCutscene());
    }

    [ContextMenu("🔄 完全重置")]
    private void CompleteReset()
    {
        if (!Application.isPlaying) return;

        StopAllCoroutines();

        vcamShot1.Priority = originalShot1Priority;
        vcamShot2.Priority = originalShot2Priority;
        vcamShot3.Priority = originalShot3Priority;
        playerVirtualCamera.Priority = originalPlayerPriority;

        if (shot1Animator != null) shot1Animator.enabled = false;
        if (shot2Animator != null) shot2Animator.enabled = false;
        if (shot3Animator != null) shot3Animator.enabled = false;

        EnablePlayerControl();

        hasTriggered = false;
        isPlayingCutscene = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Debug.Log("🔄 已完全重置,可以重新测试!");
    }
}
