using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NarrationDialogueUI : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI narrationText;
    public GameObject continueIndicator;

    [Header("对话设置")]
    public DialogueData dialogueData;
    public KeyCode nextDialogueKey = KeyCode.Space;
    public KeyCode skipAllKey = KeyCode.Escape;

    [Header("打字机效果")]
    public bool useTypewriterEffect = true;
    public float typeSpeed = 0.05f;
    public AudioClip typeSound;
    public float typeSoundVolume = 0.3f;
    public int soundEveryNChars = 2;

    [Header("过渡动画设置")]
    public TransitionType transitionType = TransitionType.FadeAndScale;
    public float transitionInDuration = 0.6f;
    public float transitionOutDuration = 0.4f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("缩放动画")]
    public Vector3 startScale = new Vector3(0.8f, 0.8f, 1f);
    public Vector3 targetScale = Vector3.one;

    [Header("移动动画")]
    public bool useSlideIn = false;
    public Vector2 slideOffset = new Vector2(0, -100f);

    [Header("播放模式")]
    public bool playOnStart = true;
    public bool autoAdvance = false;
    public float autoAdvanceDelay = 2f;

    [Header("任务系统")]
    public QuestUIManager questUIManager;
    public int initialQuestIndex = 0;  // 初始对话结束后显示的任务索引

    [Header("回调事件")]
    public System.Action onDialogueComplete;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private AudioSource audioSource;

    private int currentLineIndex = 0;
    private bool isPlaying = false;
    private bool isTyping = false;
    private bool waitingForInput = false;
    private Coroutine typewriterCoroutine;

    private Vector2 originalPosition;

    public enum TransitionType
    {
        Fade,
        Scale,
        FadeAndScale,
        SlideAndFade,
        All
    }

    void Awake()
    {
        dialoguePanel.SetActive(false);

        canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;

        rectTransform = dialoguePanel.GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        rectTransform.localScale = startScale;

        if (narrationText != null)
        {
            narrationText.text = "";
        }

        if (typeSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = typeSoundVolume;
        }

        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
    }

    void Start()
    {
        if (playOnStart && dialogueData != null)
        {
            Invoke(nameof(StartDialogue), 0.5f);
        }
    }

    void Update()
    {
        if (!isPlaying) return;

        if (Input.GetKeyDown(nextDialogueKey))
        {
            if (isTyping)
            {
                SkipTypewriter();
            }
            else if (waitingForInput)
            {
                NextDialogue();
            }
        }

        if (Input.GetKeyDown(skipAllKey))
        {
            SkipAllDialogue();
        }
    }

    public void StartDialogue()
    {
        if (isPlaying || dialogueData == null || dialogueData.lines.Count == 0)
            return;

        isPlaying = true;
        currentLineIndex = 0;
        StartCoroutine(PlayDialogueSequence());
    }

    IEnumerator PlayDialogueSequence()
    {
        // 锁定任务系统
        if (questUIManager != null)
        {
            questUIManager.LockQuestSystem();
        }

        dialoguePanel.SetActive(true);
        narrationText.text = "";

        yield return StartCoroutine(TransitionIn());

        while (currentLineIndex < dialogueData.lines.Count)
        {
            DialogueData.DialogueLine line = dialogueData.lines[currentLineIndex];

            if (useTypewriterEffect)
            {
                typewriterCoroutine = StartCoroutine(TypeText(line.text));
                yield return typewriterCoroutine;
            }
            else
            {
                narrationText.text = line.text;
            }

            if (continueIndicator != null)
            {
                continueIndicator.SetActive(true);
            }

            if (autoAdvance)
            {
                yield return new WaitForSeconds(autoAdvanceDelay);
            }
            else
            {
                waitingForInput = true;
                yield return new WaitUntil(() => !waitingForInput);
            }

            if (continueIndicator != null)
            {
                continueIndicator.SetActive(false);
            }

            if (currentLineIndex < dialogueData.lines.Count - 1)
            {
                yield return new WaitForSeconds(0.2f);
            }

            currentLineIndex++;
        }

        yield return StartCoroutine(TransitionOut());

        dialoguePanel.SetActive(false);
        isPlaying = false;
        OnDialogueComplete();
    }

    IEnumerator TransitionIn()
    {
        float elapsed = 0f;
        canvasGroup.alpha = 0f;

        if (transitionType == TransitionType.Scale ||
            transitionType == TransitionType.FadeAndScale ||
            transitionType == TransitionType.All)
        {
            rectTransform.localScale = startScale;
        }

        if ((transitionType == TransitionType.SlideAndFade || transitionType == TransitionType.All)
            && useSlideIn)
        {
            rectTransform.anchoredPosition = originalPosition + slideOffset;
        }

        while (elapsed < transitionInDuration)
        {
            elapsed += Time.deltaTime;
            float t = transitionCurve.Evaluate(elapsed / transitionInDuration);

            if (transitionType != TransitionType.Scale)
            {
                canvasGroup.alpha = t;
            }

            if (transitionType == TransitionType.Scale ||
                transitionType == TransitionType.FadeAndScale ||
                transitionType == TransitionType.All)
            {
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            }

            if ((transitionType == TransitionType.SlideAndFade || transitionType == TransitionType.All)
                && useSlideIn)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(
                    originalPosition + slideOffset,
                    originalPosition,
                    t
                );
            }

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.localScale = targetScale;
        rectTransform.anchoredPosition = originalPosition;
    }

    IEnumerator TransitionOut()
    {
        float elapsed = 0f;
        Vector3 currentScale = rectTransform.localScale;

        while (elapsed < transitionOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionOutDuration;

            canvasGroup.alpha = 1f - t;

            if (transitionType == TransitionType.Scale ||
                transitionType == TransitionType.FadeAndScale ||
                transitionType == TransitionType.All)
            {
                rectTransform.localScale = Vector3.Lerp(currentScale, startScale * 0.9f, t);
            }

            if ((transitionType == TransitionType.SlideAndFade || transitionType == TransitionType.All)
                && useSlideIn)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(
                    originalPosition,
                    originalPosition - slideOffset,
                    t
                );
            }

            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        narrationText.text = "";

        int charCount = 0;
        foreach (char c in text)
        {
            narrationText.text += c;
            charCount++;

            if (typeSound != null && charCount % soundEveryNChars == 0)
            {
                audioSource.PlayOneShot(typeSound);
            }

            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        if (currentLineIndex < dialogueData.lines.Count)
        {
            narrationText.text = dialogueData.lines[currentLineIndex].text;
        }

        isTyping = false;
    }

    void NextDialogue()
    {
        waitingForInput = false;
    }

    public void SkipAllDialogue()
    {
        StopAllCoroutines();

        canvasGroup.alpha = 0f;
        dialoguePanel.SetActive(false);
        isPlaying = false;
        waitingForInput = false;
        isTyping = false;

        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }

        OnDialogueComplete();
    }

    void OnDialogueComplete()
    {
        Debug.Log("旁白对话播放完成");

        // 如果有回调（触发器设置的），优先执行回调
        if (onDialogueComplete != null)
        {
            onDialogueComplete.Invoke();
            onDialogueComplete = null;
        }
        // 如果是初始对话（没有回调），执行默认行为
        else if (questUIManager != null)
        {
            // 解锁任务系统
            questUIManager.UnlockQuestSystem();

            // 设置初始任务图片
            questUIManager.UpdateQuestImage(initialQuestIndex);

            Debug.Log($"✅ 初始对话完成，任务图片设置为：任务{initialQuestIndex + 1}");
        }
    }

    /// <summary>
    /// 播放指定的对话数据
    /// </summary>
    public void PlayDialogue(DialogueData dialogue, System.Action onComplete = null)
    {
        if (isPlaying || dialogue == null || dialogue.lines.Count == 0)
        {
            Debug.LogWarning("⚠️ 无法播放对话：正在播放中或对话数据为空");
            return;
        }

        // 保存当前对话数据和回调
        dialogueData = dialogue;
        onDialogueComplete = onComplete;

        // 重置索引
        currentLineIndex = 0;

        // 开始播放
        isPlaying = true;
        StartCoroutine(PlayDialogueSequence());
    }

    /// <summary>
    /// 根据对话ID播放对话
    /// </summary>
    public void PlayDialogueByID(string dialogueID, System.Action onComplete = null)
    {
        // 从资源中加载对话数据
        DialogueData dialogue = Resources.Load<DialogueData>($"Dialogues/{dialogueID}");

        if (dialogue != null)
        {
            Debug.Log($"✅ 成功加载对话：{dialogueID}");
            PlayDialogue(dialogue, onComplete);
        }
        else
        {
            Debug.LogError($"❌ 找不到对话数据：Dialogues/{dialogueID}");
        }
    }
}
