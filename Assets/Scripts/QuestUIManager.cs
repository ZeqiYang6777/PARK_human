using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class QuestUIManager : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject questPanel;
    public Image questImage;
    public Button questButton;
    // ❌ 已移除：public Image tvFlashImage;

    [Header("任务图片列表")]
    public List<Sprite> questSprites = new List<Sprite>();

    [Header("按键设置")]
    public KeyCode toggleKey = KeyCode.M;

    [Header("动画设置")]
    public AnimationType animationType = AnimationType.ScaleRotateFade;
    public float fadeInDuration = 0.6f;
    public float fadeOutDuration = 0.3f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("缩放动画")]
    public Vector3 startScale = new Vector3(0.3f, 0.3f, 1f);
    public Vector3 targetScale = Vector3.one;
    public Vector3 overshootScale = new Vector3(1.15f, 1.15f, 1f);
    public bool useOvershoot = true;

    [Header("旋转动画")]
    public bool enableRotation = true;
    public float startRotationZ = 180f;
    public float targetRotationZ = 0f;
    public int rotationLoops = 0;
    public RotationDirection rotationDirection = RotationDirection.Clockwise;

    [Header("📺 电视关闭效果")]
    public bool useTVShutdownEffect = true;
    public float tvShutdownDuration = 0.5f;
    public AnimationCurve tvShutdownCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    // ❌ 已移除：public bool enableTVFlash = true;
    // ❌ 已移除：public Color tvFlashColor = Color.white;
    // ❌ 已移除：public float tvFlashDuration = 0.1f;

    [Header("🔊 音效设置")]
    public AudioClip questUnlockSound;
    public AudioClip questUpdateSound;
    public AudioClip panelOpenSound;
    public AudioClip panelCloseSound;
    public AudioClip tvShutdownSound;

    [Range(0f, 1f)]
    public float questUnlockVolume = 0.8f;
    [Range(0f, 1f)]
    public float questUpdateVolume = 0.7f;
    [Range(0f, 1f)]
    public float panelOpenVolume = 0.6f;
    [Range(0f, 1f)]
    public float panelCloseVolume = 0.5f;
    [Range(0f, 1f)]
    public float tvShutdownVolume = 0.6f;

    [Header("调试")]
    public bool showDebugInfo = true;
    public int currentQuestIndex = 0;

    private CanvasGroup panelCanvasGroup;
    private RectTransform panelRectTransform;
    private AudioSource audioSource;
    // ❌ 已移除：private CanvasGroup flashCanvasGroup;
    private bool isPanelOpen = false;
    private bool isQuestLocked = false;
    private bool isAnimating = false;

    public enum AnimationType
    {
        Fade,
        Scale,
        FadeAndScale,
        Rotate,
        ScaleRotate,
        ScaleRotateFade,
        Bounce,
        Elastic
    }

    public enum RotationDirection
    {
        Clockwise,
        CounterClockwise
    }

    void Start()
    {
        questPanel.SetActive(false);

        // 获取或添加CanvasGroup
        panelCanvasGroup = questPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = questPanel.AddComponent<CanvasGroup>();
            Debug.Log("✅ 已自动添加CanvasGroup组件");
        }
        panelCanvasGroup.alpha = 0f;

        // 获取RectTransform
        panelRectTransform = questPanel.GetComponent<RectTransform>();

        // 获取或添加AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            Debug.Log("✅ 已自动添加AudioSource组件");
        }

        // ❌ 已移除：闪光效果初始化代码块

        ValidateQuestSprites();

        // 隐藏任务按钮
        if (questButton != null)
        {
            questButton.gameObject.SetActive(false);
        }
    }

    // ❌ 已移除：CreateTVFlashImage() 方法

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isAnimating)
            {
                if (showDebugInfo)
                    Debug.Log("⏸️ 动画播放中，请稍候...");
                return;
            }

            if (isQuestLocked)
            {
                if (showDebugInfo)
                    Debug.Log("🔒 任务系统尚未解锁，请完成对话");
                return;
            }

            if (isPanelOpen)
            {
                CloseQuestPanel();
            }
            else
            {
                OpenQuestPanel();
            }
        }
    }

    void ValidateQuestSprites()
    {
        while (questSprites.Count < 6)
        {
            questSprites.Add(null);
        }

        for (int i = 0; i < questSprites.Count; i++)
        {
            if (questSprites[i] == null)
            {
                Debug.LogWarning($"⚠️ 任务图片 {i + 1} 未设置！");
            }
        }
    }

    public void OpenQuestPanel()
    {
        if (isQuestLocked || isAnimating) return;

        isPanelOpen = true;
        questPanel.SetActive(true);

        // 🔊 播放打开音效
        PlaySound(panelOpenSound, panelOpenVolume, "打开面板");

        // 隐藏按钮
        if (questButton != null)
        {
            questButton.gameObject.SetActive(false);
        }

        StartCoroutine(AnimatePanel(true));

        if (showDebugInfo)
            Debug.Log($"📋 打开任务面板，当前任务：{currentQuestIndex + 1}/6");
    }

    public void CloseQuestPanel()
    {
        if (isAnimating) return;

        isPanelOpen = false;

        // 根据设置选择关闭效果
        if (useTVShutdownEffect)
        {
            // 🔊 播放电视关闭音效
            PlaySound(tvShutdownSound, tvShutdownVolume, "电视关闭");
            StartCoroutine(TVShutdownEffect());
        }
        else
        {
            // 🔊 播放普通关闭音效
            PlaySound(panelCloseSound, panelCloseVolume, "关闭面板");
            StartCoroutine(AnimatePanel(false));
        }

        // 显示按钮
        if (questButton != null)
        {
            questButton.gameObject.SetActive(true);
        }

        if (showDebugInfo)
            Debug.Log("📺 关闭任务面板（电视关闭效果）");
    }

    IEnumerator AnimatePanel(bool isOpening)
    {
        isAnimating = true;

        float duration = isOpening ? fadeInDuration : fadeOutDuration;
        float startAlpha = isOpening ? 0f : 1f;
        float targetAlpha = isOpening ? 1f : 0f;

        Vector3 currentScale = panelRectTransform.localScale;
        Vector3 finalScale = isOpening ? targetScale : startScale;

        float currentRotation = isOpening ? startRotationZ : targetRotationZ;
        float targetRotation = isOpening ? targetRotationZ : startRotationZ;

        float totalRotation = targetRotation - currentRotation;
        if (rotationLoops > 0)
        {
            totalRotation += 360f * rotationLoops * (rotationDirection == RotationDirection.Clockwise ? 1 : -1);
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / duration);

            // 淡入淡出
            if (animationType == AnimationType.Fade ||
                animationType == AnimationType.FadeAndScale ||
                animationType == AnimationType.ScaleRotateFade)
            {
                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            }

            // 缩放动画
            if (animationType == AnimationType.Scale ||
                animationType == AnimationType.FadeAndScale ||
                animationType == AnimationType.ScaleRotate ||
                animationType == AnimationType.ScaleRotateFade ||
                animationType == AnimationType.Bounce ||
                animationType == AnimationType.Elastic)
            {
                Vector3 targetScaleValue = finalScale;

                if (animationType == AnimationType.Bounce && isOpening)
                {
                    float bounceT = Mathf.Sin(t * Mathf.PI);
                    targetScaleValue = Vector3.Lerp(startScale, targetScale, t) * (1f + bounceT * 0.3f);
                }
                else if ((animationType == AnimationType.Elastic || useOvershoot) && isOpening)
                {
                    if (t < 0.7f)
                    {
                        targetScaleValue = Vector3.Lerp(startScale, overshootScale, t / 0.7f);
                    }
                    else
                    {
                        targetScaleValue = Vector3.Lerp(overshootScale, targetScale, (t - 0.7f) / 0.3f);
                    }
                }
                else
                {
                    targetScaleValue = Vector3.Lerp(currentScale, finalScale, t);
                }

                panelRectTransform.localScale = targetScaleValue;
            }

            // 旋转动画
            if (enableRotation && (
                animationType == AnimationType.Rotate ||
                animationType == AnimationType.ScaleRotate ||
                animationType == AnimationType.ScaleRotateFade))
            {
                float rotationZ = currentRotation + totalRotation * t;
                panelRectTransform.localEulerAngles = new Vector3(0, 0, rotationZ);
            }

            yield return null;
        }

        panelCanvasGroup.alpha = targetAlpha;
        panelRectTransform.localScale = finalScale;
        panelRectTransform.localEulerAngles = new Vector3(0, 0, targetRotation);

        if (!isOpening)
        {
            questPanel.SetActive(false);
        }

        isAnimating = false;
    }

    
    IEnumerator TVShutdownEffect()
    {
        isAnimating = true;

        float elapsed = 0f;
        Vector3 originalScale = panelRectTransform.localScale;

        // 阶段1：Y轴压扁成横线（0-60%）
        float phase1Duration = tvShutdownDuration * 0.6f;
        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            float t = tvShutdownCurve.Evaluate(elapsed / phase1Duration);

            // Y轴快速压扁，X轴保持
            float scaleY = Mathf.Lerp(1f, 0.02f, t);
            float scaleX = 1f;

            panelRectTransform.localScale = new Vector3(
                originalScale.x * scaleX,
                originalScale.y * scaleY,
                1f
            );

            // 轻微淡出
            panelCanvasGroup.alpha = Mathf.Lerp(1f, 0.8f, t);

            yield return null;
        }

        // 阶段2：X轴收缩成点（60-100%）
        float phase2Start = elapsed;
        float phase2Duration = tvShutdownDuration * 0.4f;

        while (elapsed < tvShutdownDuration)
        {
            elapsed += Time.deltaTime;
            float t = (elapsed - phase2Start) / phase2Duration;
            t = Mathf.Clamp01(t);

            // X轴收缩成点
            float scaleX = Mathf.Lerp(1f, 0f, t);
            float scaleY = 0.02f;

            panelRectTransform.localScale = new Vector3(
                originalScale.x * scaleX,
                originalScale.y * scaleY,
                1f
            );

            // 快速淡出
            panelCanvasGroup.alpha = Mathf.Lerp(0.8f, 0f, t);

            yield return null;
        }

        // 确保完全缩小
        panelRectTransform.localScale = Vector3.zero;
        panelCanvasGroup.alpha = 0f;

     

        // 恢复原始状态
        panelRectTransform.localScale = originalScale;
        questPanel.SetActive(false);

        isAnimating = false;

        if (showDebugInfo)
            Debug.Log("📺 电视关闭动画完成");
    }

    // ❌ 已移除：TVFlashEffect() 协程方法

    public void LockQuestSystem()
    {
        isQuestLocked = true;

        if (showDebugInfo)
            Debug.Log("🔒 任务系统已锁定");
    }

    public void UnlockQuestSystem()
    {
        isQuestLocked = false;

        // 🔊 播放任务解锁音效
        PlaySound(questUnlockSound, questUnlockVolume, "任务解锁");

        if (questButton != null)
        {
            questButton.gameObject.SetActive(true);
        }

        if (showDebugInfo)
            Debug.Log("🔓 任务系统已解锁，按M键打开任务列表");
    }

    public void UpdateQuestImage(int index)
    {
        if (index < 0 || index >= questSprites.Count)
        {
            Debug.LogError($"❌ 任务索引超出范围：{index}（有效范围：0-{questSprites.Count - 1}）");
            return;
        }

        if (questImage == null)
        {
            Debug.LogError("❌ 任务图片组件未设置！");
            return;
        }

        if (questSprites[index] == null)
        {
            Debug.LogError($"❌ 任务图片 {index + 1} 未设置！");
            return;
        }

        questImage.sprite = questSprites[index];
        currentQuestIndex = index;

        // 🔊 播放任务更新音效
        PlaySound(questUpdateSound, questUpdateVolume, "任务更新");

        if (showDebugInfo)
            Debug.Log($"✅ 任务图片已更新为：任务{index + 1} ({questSprites[index].name})");
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    void PlaySound(AudioClip clip, float volume, string soundName)
    {
        if (clip == null)
        {
            if (showDebugInfo)
                Debug.LogWarning($"⚠️ {soundName}音效未设置");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogError("❌ AudioSource组件缺失！");
            return;
        }

        audioSource.PlayOneShot(clip, volume);

        if (showDebugInfo)
            Debug.Log($"🔊 播放音效：{soundName} ({clip.name})");
    }

    public bool IsUnlocked()
    {
        return !isQuestLocked;
    }

    public int GetCurrentQuestIndex()
    {
        return currentQuestIndex;
    }
}
