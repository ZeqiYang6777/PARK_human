using UnityEngine;
using Cinemachine;
using System;
using System.Collections;

public class CinematicController : MonoBehaviour
{
    [System.Serializable]
    public class CinematicShot
    {
        public CinemachineVirtualCamera camera;
        public Animator animator;
        public string animationTrigger = "Play";
        public float duration = 5f;
    }

    [Header("Cinemachine配置")]
    public CinematicShot[] shots = new CinematicShot[3];
    public CinemachineVirtualCamera playerCamera;

    [Header("优先级设置")]
    public int cinematicPriority = 20;
    public int playerCameraPriority = 10;

    private Action onSequenceComplete;

    void Start()
    {
        foreach (var shot in shots)
        {
            if (shot.camera != null)
            {
                shot.camera.Priority = 0;
            }
        }

        if (playerCamera != null)
        {
            playerCamera.Priority = playerCameraPriority;
        }
    }

    public void PlaySequence(Action onComplete = null)
    {
        onSequenceComplete = onComplete;
        StartCoroutine(PlayCinematicSequence());
    }

    IEnumerator PlayCinematicSequence()
    {
        Debug.Log("开始播放Cinemachine序列");

        if (playerCamera != null)
            playerCamera.Priority = 0;

        for (int i = 0; i < shots.Length; i++)
        {
            var shot = shots[i];

            if (shot.camera == null)
            {
                Debug.LogWarning($"镜头 {i} 的相机未设置！");
                continue;
            }

            Debug.Log($"播放镜头 {i + 1}/{shots.Length}");

            shot.camera.Priority = cinematicPriority;

            if (shot.animator != null && !string.IsNullOrEmpty(shot.animationTrigger))
            {
                shot.animator.SetTrigger(shot.animationTrigger);
            }

            yield return new WaitForSeconds(shot.duration);

            shot.camera.Priority = 0;
        }

        if (playerCamera != null)
            playerCamera.Priority = playerCameraPriority;

        Debug.Log("Cinemachine序列播放完成");

        onSequenceComplete?.Invoke();
    }
}
