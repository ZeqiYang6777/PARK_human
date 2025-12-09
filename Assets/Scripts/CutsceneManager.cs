using UnityEngine;
using System.Collections;
using Cinemachine;
using PixelCrushers.DialogueSystem;

public class QuestZone_2 : MonoBehaviour
{
    [Header("=== 核心引用 ===")]
    [Tooltip("任务UI管理器")]
    public QuestUIManager questUIManager;

    [Header("=== Cinemachine虚拟相机 ===")]
    [Tooltip("镜头1虚拟相机")]
    public CinemachineVirtualCamera vcamShot1;

    [Tooltip("镜头2虚拟相机")]
    public CinemachineVirtualCamera vcamShot2;

    [Tooltip("镜头3虚拟相机")]
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

    [Header("=== 时间设置 ===")]
    [Tooltip("镜头1持续时间（秒）")]
    public float shot1Duration = 3f;

    [Tooltip("镜头2持续时间（秒）")]
    public float shot2Duration = 3f;

    [Tooltip("镜头3持续时间（秒）")]
    public float shot3Duration = 3f;

    [Tooltip("相机切换混合时长（秒）")]
    public float blendDuration = 1.5f;

    [Header("=== 音效设置 ===")]
    [Tooltip("过场开始音效")]
    public AudioClip cutsceneStartSound;

    [Tooltip("过场结束音效")]
    public AudioClip cutsceneEndSound;

    [Tooltip("音效播放器")]
    public AudioSource audioSource;

    [Header("=== 对话系统 ===")]
    [Tooltip("过场后播放的旁白对话名称")]
    public string conversationAfterCutscene;

    [Tooltip("等待旁白播放完成后再显示任务")]
    public bool waitForNarrationBeforeQuest = true;

    [Header("=== 任务设置 ===")]
    [Tooltip("过场后显示的任务索引")]
    public int questIndexAfterCutscene = 1;

    [Tooltip("过场后自动打开任务面板")]
    public bool autoOpenQuestPanel = true;

    [Header("=== 玩家控制 ===")]
    [Tooltip("玩家角色的Transform")]
    public Transform playerTransform;

    [Tooltip("玩家移动脚本类型名称")]
    public string playerMovementScriptName = "PlayerMovement";

    [Header("=== 其他设置 ===")]
    [Tooltip("触发后禁用碰撞器")]
    public bool disableColliderAfterTrigger = true;

    [Tooltip("显示详细调试信息")]
    public bool showDebugInfo = true;

    // 私有变量
    private bool isPlayingCutscene = false;
    private MonoBehaviour playerMovementScript;

    private void Start()
    {
        // 自动查找引用
        AutoSetup();

        // 确保所有镜头相机初始优先级为0
        if (vcamShot1 != null) vcamShot1.Priority = 0;
        if (vcamShot2 != null) vcamShot2.Priority = 0;
        if (vcamShot3 != null) vcamShot3.Priority = 0;

        if (showDebugInfo)
        {
            Debug.Log("✅ [QuestZone_2] 初始化完成");
        }
    }

    private void AutoSetup()
    {
        // 自动查找 QuestUIManager
        if (questUIManager == null)
        {
            questUIManager = FindObjectOfType<QuestUIManager>();
            if (questUIManager == null && showDebugInfo)
            {
                Debug.LogWarning("⚠️ 未找到 QuestUIManager");
            }
        }

        // 自动查找玩家
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else if (showDebugInfo)
            {
                Debug.LogWarning("⚠️ 未找到 Player 标签的游戏对象");
            }
        }

        // 查找玩家移动脚本
        if (playerTransform != null)
        {
            playerMovementScript = playerTransform.GetComponent(playerMovementScriptName) as MonoBehaviour;
            if (playerMovementScript == null && showDebugInfo)
            {
                Debug.LogWarning($"⚠️ 未找到 {playerMovementScriptName} 脚本");
            }
        }

        // 自动查找玩家虚拟相机
        if (playerVirtualCamera == null)
        {
            CinemachineVirtualCamera[] vcams = FindObjectsOfType<CinemachineVirtualCamera>();
            foreach (var vcam in vcams)
            {
                if (vcam.gameObject.name.ToLower().Contains("player") ||
                    vcam.gameObject.name.ToLower().Contains("main"))
                {
                    playerVirtualCamera = vcam;
                    break;
                }
            }

            if (playerVirtualCamera == null && showDebugInfo)
            {
                Debug.LogWarning("⚠️ 未找到玩家虚拟相机");
            }
        }

        // 自动查找 AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayingCutscene)
        {
            if (showDebugInfo)
            {
                Debug.Log("🎯 玩家触发过场区域");
            }

            StartCoroutine(PlayCutscene());
        }
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
        if (showDebugInfo)
        {
            Debug.Log("📹 开始播放镜头1");
        }
        yield return StartCoroutine(PlayShot(vcamShot1, shot1Animator, shot1Duration, 1));

        // ========== 播放镜头2 ==========
        if (showDebugInfo)
        {
            Debug.Log("📹 开始播放镜头2");
        }
        yield return StartCoroutine(PlayShot(vcamShot2, shot2Animator, shot2Duration, 2));

        // ========== 播放镜头3 ==========
        if (showDebugInfo)
        {
            Debug.Log("📹 开始播放镜头3");
        }
        yield return StartCoroutine(PlayShot(vcamShot3, shot3Animator, shot3Duration, 3));

        // ========== 所有镜头播放完毕 ==========
        if (showDebugInfo)
        {
            Debug.Log("✅ 所有镜头播放完毕，准备切回玩家相机");
        }

        // 降低所有镜头相机优先级
        vcamShot1.Priority = 0;
        vcamShot2.Priority = 0;
        vcamShot3.Priority = 0;

        // 提高玩家相机优先级
        playerVirtualCamera.Priority = 100;

        if (showDebugInfo)
        {
            Debug.Log($"🎥 已设置玩家相机优先级 = {playerVirtualCamera.Priority}");
        }

        // ⭐⭐⭐ 关键修复：等待相机切换完成 ⭐⭐⭐
        float cameraBlendWaitTime = blendDuration + 0.8f; // 混合时间 + 额外缓冲

        if (showDebugInfo)
        {
            Debug.Log($"⏳ 等待相机切换完成... ({cameraBlendWaitTime} 秒)");
        }

        yield return new WaitForSeconds(cameraBlendWaitTime);

        if (showDebugInfo)
        {
            Debug.Log("✅ 相机已切换到玩家视角");
        }

        // 恢复玩家控制
        EnablePlayerControl();

        // 播放结束音效
        PlaySound(cutsceneEndSound);

        if (showDebugInfo)
        {
            Debug.Log("✅ 过场动画结束，玩家控制已恢复");
        }

        // ⭐⭐⭐ 额外稳定等待 ⭐⭐⭐
        yield return new WaitForSeconds(0.3f);

        // ========== 现在才播放旁白对话! ==========
        if (!string.IsNullOrEmpty(conversationAfterCutscene))
        {
            if (showDebugInfo)
            {
                Debug.Log($"💬 开始播放旁白对话: {conversationAfterCutscene}");

                // 显示当前活跃的相机（用于调试）
                var activeCam = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
                if (activeCam != null)
                {
                    Debug.Log($"📷 当前活跃相机: {activeCam.Name}");
                }
            }

            // 播放旁白对话
            DialogueManager.StartConversation(conversationAfterCutscene);

            // 如果需要等待旁白播放完成
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

        // ========== 显示任务 ==========
        if (questUIManager != null)
        {
            questUIManager.UpdateQuestImage(questIndexAfterCutscene);

            if (showDebugInfo)
            {
                Debug.Log($"✅ 已更新任务图片为索引: {questIndexAfterCutscene}");
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

        // 禁用碰撞器
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
        if (vcam == null)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"⚠️ 镜头{shotNumber}的虚拟相机未设置");
            }
            yield break;
        }

        // 设置相机优先级
        vcam.Priority = 100;

        if (showDebugInfo)
        {
            Debug.Log($"🎥 镜头{shotNumber} 相机优先级 = {vcam.Priority}");
        }

        // 播放动画（如果有）
        if (animator != null)
        {
            animator.SetTrigger("Play");
            if (showDebugInfo)
            {
                Debug.Log($"🎬 镜头{shotNumber} 动画已触发");
            }
        }

        // 等待镜头持续时间
        yield return new WaitForSeconds(duration);

        // 降低相机优先级
        vcam.Priority = 0;

        if (showDebugInfo)
        {
            Debug.Log($"✅ 镜头{shotNumber} 播放完成");
        }
    }

    private void DisablePlayerControl()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
            if (showDebugInfo)
            {
                Debug.Log("🚫 已禁用玩家移动控制");
            }
        }

        // 停止玩家移动
        if (playerTransform != null)
        {
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void EnablePlayerControl()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
            if (showDebugInfo)
            {
                Debug.Log("✅ 已恢复玩家移动控制");
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
}
