using UnityEngine;
using Cinemachine;
using System;
using System.Collections;

public class CinematicController : MonoBehaviour
{
    [System.Serializable]
    public class CinematicShot
    {
        public string shotName = "镜头";
        public CinemachineVirtualCamera camera;
        public Animator animator;
        public string animationTrigger = "Play";
        public float duration = 5f;

        [Header("动画控制")]
        [Tooltip("切换到该镜头时才播放动画")]
        public bool playOnlyWhenActive = true;

        [Tooltip("播放完是否强制停止动画")]
        public bool forceStopAnimation = true;

        [Tooltip("停止时回到的状态名称")]
        public string idleStateName = "Idle";
    }

    [Header("🎥 Cinemachine 配置")]
    public CinematicShot[] shots = new CinematicShot[3];
    public CinemachineVirtualCamera playerCamera;

    [Header("⚙️ 优先级设置")]
    public int cinematicPriority = 20;
    public int playerCameraPriority = 10;

    [Header("🎬 混合设置")]
    [Tooltip("相机切换的混合时间（秒）")]
    [Range(0.1f, 3f)]
    public float blendTime = 1f;

    [Tooltip("等待混合完成再播放动画")]
    public bool waitForBlendBeforeAnimation = true;

    [Header("🐛 调试选项")]
    public bool showDebugLog = true;
    public bool pauseBetweenShots = false;
    public float pauseDuration = 0.5f;

    private Action onSequenceComplete;
    private bool isPlaying = false;
    private CinemachineBrain cinemachineBrain;

    void Start()
    {
        // 获取 Cinemachine Brain
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

        if (cinemachineBrain == null)
        {
            Debug.LogError("❌ Main Camera 上没有 Cinemachine Brain！");
        }

        InitializeCameras();
    }

    /// <summary>
    /// 初始化所有相机和动画
    /// </summary>
    void InitializeCameras()
    {
        // 禁用所有过场相机
        foreach (var shot in shots)
        {
            if (shot.camera != null)
            {
                shot.camera.Priority = 0;

                // 🔧 修复：禁用 Animator，防止自动播放
                if (shot.animator != null)
                {
                    shot.animator.enabled = false;  // ← 关键修复

                    if (showDebugLog)
                        Debug.Log($"✓ 已禁用 Animator: {shot.shotName}");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ 镜头 '{shot.shotName}' 的相机未设置！");
            }
        }

        // 确保玩家相机激活
        if (playerCamera != null)
        {
            playerCamera.Priority = playerCameraPriority;
        }
        else
        {
            Debug.LogError("❌ 玩家相机 (Player Camera) 未设置！");
        }
    }

    /// <summary>
    /// 播放过场动画序列
    /// </summary>
    public void PlaySequence(Action onComplete = null)
    {
        if (isPlaying)
        {
            Debug.LogWarning("⚠️ 序列正在播放中，忽略重复调用");
            return;
        }

        onSequenceComplete = onComplete;
        StartCoroutine(PlayCinematicSequence());
    }

    /// <summary>
    /// 强制停止序列
    /// </summary>
    public void StopSequence()
    {
        if (isPlaying)
        {
            StopAllCoroutines();
            ResetToPlayerCamera();
            isPlaying = false;

            if (showDebugLog)
                Debug.Log("🛑 序列已强制停止");
        }
    }

    /// <summary>
    /// 播放完整的过场动画序列
    /// </summary>
    IEnumerator PlayCinematicSequence()
    {
        isPlaying = true;

        if (showDebugLog)
            Debug.Log("🎬 开始播放 Cinemachine 序列");

        // 禁用玩家相机
        if (playerCamera != null)
            playerCamera.Priority = 0;

        // 依次播放所有镜头
        for (int i = 0; i < shots.Length; i++)
        {
            var shot = shots[i];

            // 检查镜头是否有效
            if (shot.camera == null)
            {
                Debug.LogWarning($"⚠️ 镜头 {i + 1} 的相机未设置，跳过");
                continue;
            }

            // 播放镜头
            yield return StartCoroutine(PlayShot(shot, i + 1));

            // 镜头之间的暂停
            if (pauseBetweenShots && i < shots.Length - 1)
            {
                if (showDebugLog)
                    Debug.Log($"⏸️ 暂停 {pauseDuration} 秒...");

                yield return new WaitForSeconds(pauseDuration);
            }
        }

        // 恢复玩家相机
        ResetToPlayerCamera();

        if (showDebugLog)
            Debug.Log("✅ Cinemachine 序列播放完成");

        isPlaying = false;

        // 执行回调
        onSequenceComplete?.Invoke();
        onSequenceComplete = null;
    }

    /// <summary>
    /// 播放单个镜头（修复版）
    /// </summary>
    IEnumerator PlayShot(CinematicShot shot, int shotNumber)
    {
        if (showDebugLog)
            Debug.Log($"▶️ 播放镜头 [{shotNumber}/{shots.Length}]: {shot.shotName}");

        // 🔧 修复 1：先激活相机
        shot.camera.Priority = cinematicPriority;

        // 🔧 修复 2：等待相机混合完成
        if (waitForBlendBeforeAnimation && cinemachineBrain != null)
        {
            float blendWaitTime = Mathf.Min(blendTime, 0.5f);  // 最多等待0.5秒
            yield return new WaitForSeconds(blendWaitTime);

            if (showDebugLog)
                Debug.Log($"  ├─ 相机混合完成 ({blendWaitTime}s)");
        }

        // 🔧 修复 3：启用 Animator 并触发动画
        if (shot.animator != null && !string.IsNullOrEmpty(shot.animationTrigger))
        {
            // 启用 Animator
            shot.animator.enabled = true;

            // 重置到初始状态
            if (shot.forceStopAnimation)
            {
                shot.animator.Play(shot.idleStateName, 0, 0f);
                yield return null;  // 等待一帧
            }

            // 触发动画
            shot.animator.SetTrigger(shot.animationTrigger);

            if (showDebugLog)
                Debug.Log($"  ├─ 触发动画: {shot.animationTrigger}");
        }
        else
        {
            if (shot.animator == null && showDebugLog)
                Debug.LogWarning($"  ├─ 镜头 '{shot.shotName}' 没有 Animator");
        }

        // 等待镜头持续时间
        if (showDebugLog)
            Debug.Log($"  └─ 持续时间: {shot.duration} 秒");

        yield return new WaitForSeconds(shot.duration);

        // 🔧 修复 4：停止动画并禁用 Animator
        if (shot.animator != null)
        {
            if (shot.forceStopAnimation)
            {
                shot.animator.Play(shot.idleStateName, 0, 0f);

                if (showDebugLog)
                    Debug.Log($"  └─ 动画已停止，返回: {shot.idleStateName}");
            }

            // 禁用 Animator，防止继续播放
            shot.animator.enabled = false;
        }

        // 🔧 修复 5：等待一小段时间再关闭相机（避免突然切换）
        yield return new WaitForSeconds(0.1f);

        // 关闭当前镜头相机
        shot.camera.Priority = 0;
    }

    /// <summary>
    /// 恢复玩家相机
    /// </summary>
    void ResetToPlayerCamera()
    {
        // 关闭所有过场相机
        foreach (var shot in shots)
        {
            if (shot.camera != null)
            {
                shot.camera.Priority = 0;
            }

            // 禁用所有 Animator
            if (shot.animator != null)
            {
                shot.animator.enabled = false;
            }
        }

        // 激活玩家相机
        if (playerCamera != null)
        {
            playerCamera.Priority = playerCameraPriority;

            if (showDebugLog)
                Debug.Log("✅ 已恢复玩家相机");
        }
    }

    /// <summary>
    /// 获取序列总时长
    /// </summary>
    public float GetTotalDuration()
    {
        float total = 0f;

        foreach (var shot in shots)
        {
            total += shot.duration;
        }

        if (pauseBetweenShots)
        {
            total += pauseDuration * (shots.Length - 1);
        }

        // 加上混合时间
        if (waitForBlendBeforeAnimation)
        {
            total += blendTime * shots.Length;
        }

        return total;
    }

    /// <summary>
    /// 验证设置是否正确
    /// </summary>
    public bool ValidateSetup()
    {
        bool isValid = true;

        if (playerCamera == null)
        {
            Debug.LogError("❌ 玩家相机未设置！");
            isValid = false;
        }

        if (shots == null || shots.Length == 0)
        {
            Debug.LogError("❌ 没有配置任何镜头！");
            isValid = false;
        }

        for (int i = 0; i < shots.Length; i++)
        {
            var shot = shots[i];

            if (shot.camera == null)
            {
                Debug.LogError($"❌ 镜头 {i + 1} ({shot.shotName}) 的相机未设置！");
                isValid = false;
            }

            if (shot.animator == null)
            {
                Debug.LogWarning($"⚠️ 镜头 {i + 1} ({shot.shotName}) 的 Animator 未设置");
            }

            if (shot.duration <= 0)
            {
                Debug.LogWarning($"⚠️ 镜头 {i + 1} ({shot.shotName}) 的持续时间 <= 0");
            }
        }

        return isValid;
    }

    void OnValidate()
    {
        if (shots == null || shots.Length < 3)
        {
            System.Array.Resize(ref shots, 3);
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        foreach (var shot in shots)
        {
            if (shot.camera != null && shot.camera.Priority > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(shot.camera.transform.position, 0.5f);
            }
        }

        if (playerCamera != null && playerCamera.Priority > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerCamera.transform.position, 0.5f);
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlaySequence();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            StopSequence();
        }
    }
#endif
}
