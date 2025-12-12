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
        [Tooltip("播放完是否强制停止动画")]
        public bool forceStopAnimation = true;

        [Tooltip("停止时回到的状态名称")]
        public string idleStateName = "Idle";
    }

    [Header(" Cinemachine 配置 ")]
    public CinematicShot[] shots = new CinematicShot[3];
    public CinemachineVirtualCamera playerCamera;

    [Header(" 优先级设置 ")]
    public int cinematicPriority = 20;
    public int playerCameraPriority = 10;

    [Header("调试选项 ")]
    public bool showDebugLog = true;
    public bool pauseBetweenShots = false;
    public float pauseDuration = 0.5f;

    private Action onSequenceComplete;
    private bool isPlaying = false;

    void Start()
    {
        InitializeCameras();
    }

    
    void InitializeCameras()
    {
        // 禁用所有过场相机
        foreach (var shot in shots)
        {
            if (shot.camera != null)
            {
                shot.camera.Priority = 0;

                // 确保动画处于Idle状态
                if (shot.animator != null && shot.forceStopAnimation)
                {
                    shot.animator.Play(shot.idleStateName, 0, 0f);
                }
            }
            else
            {
                Debug.LogWarning($"[CinematicController] 镜头 '{shot.shotName}' 的相机未设置！");
            }
        }

        // 确保玩家相机激活
        if (playerCamera != null)
        {
            playerCamera.Priority = playerCameraPriority;
        }
        else
        {
            Debug.LogError("[CinematicController] 玩家相机 (Player Camera) 未设置！");
        }
    }

    public void PlaySequence(Action onComplete = null)
    {
        if (isPlaying)
        {
            Debug.LogWarning("[CinematicController] 序列正在播放中，忽略重复调用");
            return;
        }

        onSequenceComplete = onComplete;
        StartCoroutine(PlayCinematicSequence());
    }

    
    public void StopSequence()
    {
        if (isPlaying)
        {
            StopAllCoroutines();
            ResetToPlayerCamera();
            isPlaying = false;

            if (showDebugLog)
                Debug.Log("[CinematicController] 序列已强制停止");
        }
    }

    IEnumerator PlayCinematicSequence()
    {
        isPlaying = true;

        if (showDebugLog)
            Debug.Log("开始播放 Cinemachine 序列 ");

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
                Debug.LogWarning($"[CinematicController] 镜头 {i + 1} 的相机未设置，跳过");
                continue;
            }

            // 播放镜头
            yield return StartCoroutine(PlayShot(shot, i + 1));

            // 镜头之间的暂停
            if (pauseBetweenShots && i < shots.Length - 1)
            {
                if (showDebugLog)
                    Debug.Log($"[CinematicController] 暂停 {pauseDuration} 秒...");

                yield return new WaitForSeconds(pauseDuration);
            }
        }

        // 恢复玩家相机
        ResetToPlayerCamera();

        if (showDebugLog)
            Debug.Log(" Cinemachine 序列播放完成");

        isPlaying = false;

        // 执行回调
        onSequenceComplete?.Invoke();
        onSequenceComplete = null;
    }

    /// <summary>
    /// 播放单个镜头
    /// </summary>
    IEnumerator PlayShot(CinematicShot shot, int shotNumber)
    {
        if (showDebugLog)
            Debug.Log($"▶ 播放镜头 [{shotNumber}/{shots.Length}]: {shot.shotName}");

        // 激活当前镜头相机
        shot.camera.Priority = cinematicPriority;

        // 触发动画
        if (shot.animator != null && !string.IsNullOrEmpty(shot.animationTrigger))
        {
            // 确保先回到Idle状态（重置动画）
            if (shot.forceStopAnimation)
            {
                shot.animator.Play(shot.idleStateName, 0, 0f);
                yield return null; // 等待一帧，确保状态切换
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

        // 强制停止动画（回到Idle）
        if (shot.animator != null && shot.forceStopAnimation)
        {
            shot.animator.Play(shot.idleStateName, 0, 0f);

            if (showDebugLog)
                Debug.Log($"  └─ 动画已停止，返回: {shot.idleStateName}");
        }

        // 关闭当前镜头相机
        shot.camera.Priority = 0;
    }

    
    void ResetToPlayerCamera()
    {
        // 关闭所有过场相机
        foreach (var shot in shots)
        {
            if (shot.camera != null)
                shot.camera.Priority = 0;
        }

        // 激活玩家相机
        if (playerCamera != null)
        {
            playerCamera.Priority = playerCameraPriority;

            if (showDebugLog)
                Debug.Log("✓ 已恢复玩家相机");
        }
    }

    
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

        return total;
    }

    
    public bool ValidateSetup()
    {
        bool isValid = true;

        if (playerCamera == null)
        {
            Debug.LogError("[CinematicController] 玩家相机未设置！");
            isValid = false;
        }

        if (shots == null || shots.Length == 0)
        {
            Debug.LogError("[CinematicController] 没有配置任何镜头！");
            isValid = false;
        }

        for (int i = 0; i < shots.Length; i++)
        {
            var shot = shots[i];

            if (shot.camera == null)
            {
                Debug.LogError($"[CinematicController] 镜头 {i + 1} ({shot.shotName}) 的相机未设置！");
                isValid = false;
            }

            if (shot.animator == null)
            {
                Debug.LogWarning($"[CinematicController] 镜头 {i + 1} ({shot.shotName}) 的 Animator 未设置");
            }

            if (shot.duration <= 0)
            {
                Debug.LogWarning($"[CinematicController] 镜头 {i + 1} ({shot.shotName}) 的持续时间 <= 0");
            }
        }

        return isValid;
    }

    // 编辑器中显示信息
    void OnValidate()
    {
        // 确保至少有3个镜头槽位
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

    // 调试快捷键（可选）
    void Update()
    {
#if UNITY_EDITOR
        // 按 P 键播放序列（仅编辑器模式）
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlaySequence();
        }

        // 按 O 键停止序列（仅编辑器模式）
        if (Input.GetKeyDown(KeyCode.O))
        {
            StopSequence();
        }
#endif
    }
}
