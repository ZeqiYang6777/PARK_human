using System.Collections;
using UnityEngine;
using Cinemachine;
using PixelCrushers.DialogueSystem;

public class QuestZone2CutsceneTrigger : MonoBehaviour
{
    [Header("=== 相机设置 ===")]
    [Tooltip("玩家的虚拟相机")]
    public CinemachineVirtualCamera playerVirtualCamera;

    [Tooltip("第一个镜头")]
    public CinemachineVirtualCamera vcamShot1;

    [Tooltip("第二个镜头")]
    public CinemachineVirtualCamera vcamShot2;

    [Tooltip("第三个镜头")]
    public CinemachineVirtualCamera vcamShot3;

    [Header("=== 时间设置 ===")]
    [Tooltip("第一个镜头持续时间（秒）")]
    public float shot1Duration = 2f;

    [Tooltip("第二个镜头持续时间（秒）")]
    public float shot2Duration = 3f;

    [Tooltip("第三个镜头持续时间（秒）")]
    public float shot3Duration = 2f;

    [Tooltip("相机切换混合时间（秒）")]
    public float blendDuration = 1f;

    [Header("=== 旁白设置 ===")]
    [Tooltip("过场期间播放的对话名称（旁白2）")]
    public string conversationDuringCutscene = "Zone1_Dialogue";

    [Header("=== 任务设置 ===")]
    [Tooltip("任务UI管理器")]
    public QuestUIManager questUIManager;

    [Tooltip("过场结束后显示的任务索引")]
    public int questIndexAfterCutscene = 1;

    [Tooltip("是否自动打开任务面板")]
    public bool autoOpenQuestPanel = true;

    [Header("=== 玩家控制 ===")]
    [Tooltip("玩家控制器（用于禁用输入）")]
    public MonoBehaviour playerController;

    [Tooltip("玩家移动脚本（用于禁用移动）")]
    public MonoBehaviour playerMovement;

    [Header("=== 音效设置 ===")]
    [Tooltip("过场开始音效")]
    public AudioClip cutsceneStartSound;

    [Tooltip("过场结束音效")]
    public AudioClip cutsceneEndSound;

    [Tooltip("音效播放器")]
    public AudioSource audioSource;

    [Header("=== 调试设置 ===")]
    [Tooltip("显示详细调试信息")]
    public bool showDebugInfo = true;

    // 私有变量
    private bool hasTriggered = false;
    private bool isPlayingCutscene = false;

    private void Start()
    {
        // 初始化：确保所有过场镜头优先级为0
        if (vcamShot1 != null) vcamShot1.Priority = 0;
        if (vcamShot2 != null) vcamShot2.Priority = 0;
        if (vcamShot3 != null) vcamShot3.Priority = 0;

        // 确保玩家相机优先级最高
        if (playerVirtualCamera != null)
        {
            playerVirtualCamera.Priority = 100;
        }

        if (showDebugInfo)
        {
            Debug.Log("✅ QuestZone2CutsceneTrigger 初始化完成");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 检查是否是玩家触发
        if (other.CompareTag("Player") && !hasTriggered)
        {
            if (showDebugInfo)
            {
                Debug.Log("🎬 玩家进入触发区域，准备播放过场动画");
            }

            hasTriggered = true;
            StartCoroutine(PlayCutscene());
        }
    }

    private IEnumerator PlayCutscene()
    {
        if (isPlayingCutscene)
        {
            if (showDebugInfo)
            {
                Debug.Log("⚠️ 过场动画已在播放中，跳过");
            }
            yield break;
        }

        isPlayingCutscene = true;

        if (showDebugInfo)
        {
            Debug.Log("🎬 ========== 开始播放过场动画 ==========");
        }

        // ========== 禁用玩家控制 ==========
        DisablePlayerControl();

        // ========== 播放开始音效 ==========
        PlaySound(cutsceneStartSound);

        // ========== 🆕 立即播放旁白（与画面同时播放） ==========
        if (!string.IsNullOrEmpty(conversationDuringCutscene))
        {
            if (showDebugInfo)
            {
                Debug.Log($"💬 [旁白2] 开始播放: {conversationDuringCutscene}");
            }

            DialogueManager.StartConversation(conversationDuringCutscene);
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log("ℹ️ [旁白2] 未设置旁白对话");
            }
        }

        // ========== 播放三个镜头（旁白会在背景中播放） ==========

        // 镜头1
        if (vcamShot1 != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"📹 切换到镜头1，持续 {shot1Duration} 秒");
            }

            vcamShot1.Priority = 200;
            yield return new WaitForSeconds(shot1Duration);
            vcamShot1.Priority = 0;

            if (showDebugInfo)
            {
                Debug.Log("✅ 镜头1 播放完毕");
            }
        }

        // 镜头2
        if (vcamShot2 != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"📹 切换到镜头2，持续 {shot2Duration} 秒");
            }

            vcamShot2.Priority = 200;
            yield return new WaitForSeconds(shot2Duration);
            vcamShot2.Priority = 0;

            if (showDebugInfo)
            {
                Debug.Log("✅ 镜头2 播放完毕");
            }
        }

        // 镜头3
        if (vcamShot3 != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"📹 切换到镜头3，持续 {shot3Duration} 秒");
            }

            vcamShot3.Priority = 200;
            yield return new WaitForSeconds(shot3Duration);
            vcamShot3.Priority = 0;

            if (showDebugInfo)
            {
                Debug.Log("✅ 镜头3 播放完毕");
            }
        }

        // ========== 所有镜头播放完毕 ==========
        if (showDebugInfo)
        {
            Debug.Log("✅ 所有镜头播放完毕,准备切回玩家相机");
        }

        // ========== 等待旁白播放完毕（如果还在播放） ==========
        if (!string.IsNullOrEmpty(conversationDuringCutscene))
        {
            float timeout = 10f;
            float elapsed = 0f;

            while (DialogueManager.isConversationActive && elapsed < timeout)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"⏳ [旁白2] 等待对话播放完成... ({elapsed:F1}秒)");
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= timeout)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning("⚠️ [旁白2] 等待超时，强制继续");
                }
            }
            else if (showDebugInfo)
            {
                Debug.Log($"✅ [旁白2] 对话播放完成");
            }
        }

        // ========== 切回玩家相机 ==========
        if (vcamShot1 != null) vcamShot1.Priority = 0;
        if (vcamShot2 != null) vcamShot2.Priority = 0;
        if (vcamShot3 != null) vcamShot3.Priority = 0;

        if (playerVirtualCamera != null)
        {
            playerVirtualCamera.Priority = 100;
        }

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

        // ========== 显示任务 ==========
        if (questUIManager != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"📋 更新任务图片为索引: {questIndexAfterCutscene}");
            }

            questUIManager.UpdateQuestImage(questIndexAfterCutscene);

            if (showDebugInfo)
            {
                Debug.Log($"✅ 已更新任务图片（任务 {questIndexAfterCutscene + 1}）");
            }

            if (autoOpenQuestPanel)
            {
                yield return new WaitForSeconds(0.5f);
                questUIManager.OpenQuestPanel();

                if (showDebugInfo)
                {
                    Debug.Log("📋 已自动打开任务面板");
                }
            }
        }

        if (showDebugInfo)
        {
            Debug.Log("🏁 ========== 过场动画完成 ==========");
        }

        isPlayingCutscene = false;
    }

    // ========== 辅助方法 ==========

    private void DisablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
            if (showDebugInfo)
            {
                Debug.Log("🚫 已禁用玩家控制器");
            }
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            if (showDebugInfo)
            {
                Debug.Log("🚫 已禁用玩家移动");
            }
        }
    }

    private void EnablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
            if (showDebugInfo)
            {
                Debug.Log("✅ 已启用玩家控制器");
            }
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            if (showDebugInfo)
            {
                Debug.Log("✅ 已启用玩家移动");
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
            if (showDebugInfo)
            {
                Debug.Log($"🔊 播放音效: {clip.name}");
            }
        }
    }

    // ========== 调试工具方法 ==========

    [ContextMenu("🎤 测试播放旁白")]
    private void TestPlayNarration()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("⚠️ 请在 Play 模式下测试！");
            return;
        }

        if (string.IsNullOrEmpty(conversationDuringCutscene))
        {
            Debug.LogError("❌ conversationDuringCutscene 未设置！");
            return;
        }

        Debug.Log($"🎤 测试播放旁白: {conversationDuringCutscene}");
        DialogueManager.StartConversation(conversationDuringCutscene);
    }

    [ContextMenu("🎬 重置触发器")]
    private void ResetTrigger()
    {
        hasTriggered = false;
        isPlayingCutscene = false;
        Debug.Log("✅ 触发器已重置，可以重新触发过场动画");
    }

    [ContextMenu("📋 列出所有对话")]
    private void ListAllConversations()
    {
        if (DialogueManager.masterDatabase == null)
        {
            Debug.LogError("❌ Dialogue Database 未加载！");
            return;
        }

        Debug.Log("📋 数据库中的所有对话：");
        foreach (var conv in DialogueManager.masterDatabase.conversations)
        {
            Debug.Log($"  ✓ {conv.Title}");
        }
    }
}
