using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("=== UI 引用 ===")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject continuePrompt;           // "点击继续" 提示（可选）

    [Header("=== 打字机效果设置 ===")]
    [Tooltip("是否启用打字机效果")]
    public bool useTypewriterEffect = true;

    [Tooltip("打字速度（秒/字符）")]
    [Range(0.01f, 0.2f)]
    public float typeSpeed = 0.05f;

    [Tooltip("标点符号的额外停顿时间")]
    [Range(0f, 0.5f)]
    public float punctuationPause = 0.15f;

    [Tooltip("遇到这些标点符号时会额外停顿")]
    public string punctuationMarks = "。！？，；：…";

    [Header("=== 音效设置 ===")]
    [Tooltip("打字音效音源")]
    public AudioSource typeSoundSource;

    [Tooltip("配音音源")]
    public AudioSource voiceSource;

    [Tooltip("背景音乐音源")]
    public AudioSource musicSource;

    [Header("打字音效")]
    [Tooltip("默认打字音效")]
    public AudioClip defaultTypeSound;

    [Tooltip("打字音效音量")]
    [Range(0f, 1f)]
    public float typeSoundVolume = 0.3f;

    [Tooltip("打字音效音调随机范围")]
    [Range(0f, 0.5f)]
    public float pitchVariation = 0.1f;

    [Tooltip("每隔几个字符播放一次打字音（1=每个字，2=每两个字）")]
    [Range(1, 5)]
    public int soundFrequency = 1;

    [Header("特殊音效")]
    [Tooltip("对话开始音效")]
    public AudioClip dialogueStartSound;

    [Tooltip("对话结束音效")]
    public AudioClip dialogueEndSound;

    [Header("=== 调试选项 ===")]
    public bool showDebugLog = false;

    // 私有变量
    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private Action onCompleteCallback;
    private bool isTyping = false;
    private bool canProceed = false;
    private Coroutine typeCoroutine;
    private int characterCount = 0;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        // 初始化音源
        InitializeAudioSources();
    }

    void Update()
    {
        if (dialoguePanel != null && dialoguePanel.activeSelf && canProceed)
        {
            // 空格键或鼠标左键继续
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                {
                    // 跳过打字效果
                    CompleteTyping();
                }
                else
                {
                    // 下一句
                    ShowNextLine();
                }
            }
        }
    }

    /// <summary>
    /// 初始化音频源
    /// </summary>
    void InitializeAudioSources()
    {
        // 如果没有手动指定音源，自动创建
        if (typeSoundSource == null)
        {
            GameObject soundObj = new GameObject("TypeSoundSource");
            soundObj.transform.SetParent(transform);
            typeSoundSource = soundObj.AddComponent<AudioSource>();
            typeSoundSource.playOnAwake = false;
        }

        if (voiceSource == null)
        {
            GameObject voiceObj = new GameObject("VoiceSource");
            voiceObj.transform.SetParent(transform);
            voiceSource = voiceObj.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
        }

        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
    }

    /// <summary>
    /// 开始对话
    /// </summary>
    public void StartDialogue(DialogueData dialogue, Action onComplete = null)
    {
        if (dialogue == null)
        {
            Debug.LogError("[DialogueSystem] DialogueData 为空！");
            return;
        }

        if (dialogue.lines == null || dialogue.lines.Length == 0)
        {
            Debug.LogError("[DialogueSystem] 对话数据没有任何行！");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        onCompleteCallback = onComplete;
        canProceed = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // 播放对话开始音效
        PlaySound(dialogueStartSound, typeSoundSource);

        // 播放背景音乐
        if (dialogue.backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = dialogue.backgroundMusic;
            musicSource.volume = dialogue.musicVolume;
            musicSource.Play();
        }

        if (showDebugLog)
            Debug.Log($"[DialogueSystem] 开始对话: {dialogue.dialogueID}");

        ShowCurrentLine();
    }

    /// <summary>
    /// 显示当前行
    /// </summary>
    void ShowCurrentLine()
    {
        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        var line = currentDialogue.lines[currentLineIndex];

        if (showDebugLog)
            Debug.Log($"[DialogueSystem] 第 {currentLineIndex + 1} 句: {line.content.Substring(0, Mathf.Min(20, line.content.Length))}...");

        // 停止之前的配音
        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        // 播放配音
        if (line.voiceClip != null && voiceSource != null)
        {
            voiceSource.clip = line.voiceClip;
            voiceSource.volume = line.voiceVolume;
            voiceSource.Play();
        }

        // 显示文本
        if (useTypewriterEffect)
        {
            if (typeCoroutine != null)
                StopCoroutine(typeCoroutine);

            // 使用自定义打字速度或全局速度
            float speed = line.customTypeSpeed > 0 ? line.customTypeSpeed : typeSpeed;
            AudioClip typeSound = line.customTypeSound != null ? line.customTypeSound : defaultTypeSound;

            typeCoroutine = StartCoroutine(TypewriterEffect(line.content, speed, typeSound));
        }
        else
        {
            // 直接显示
            if (dialogueText != null)
                dialogueText.text = line.content;

            isTyping = false;
            ShowContinuePrompt(true);
        }
    }

    /// <summary>
    /// 打字机效果协程（增强版）
    /// </summary>
    IEnumerator TypewriterEffect(string text, float speed, AudioClip typeSound)
    {
        isTyping = true;
        characterCount = 0;

        if (dialogueText != null)
            dialogueText.text = "";

        ShowContinuePrompt(false);

        // 逐字显示
        foreach (char c in text)
        {
            if (dialogueText != null)
                dialogueText.text += c;

            characterCount++;

            // 播放打字音效
            if (typeSound != null && characterCount % soundFrequency == 0)
            {
                PlayTypingSound(typeSound);
            }

            // 基础等待时间
            float waitTime = speed;

            // 标点符号额外停顿
            if (punctuationMarks.Contains(c.ToString()))
            {
                waitTime += punctuationPause;
            }

            yield return new WaitForSeconds(waitTime);
        }

        isTyping = false;
        ShowContinuePrompt(true);

        if (showDebugLog)
            Debug.Log("[DialogueSystem] 打字完成");
    }

    /// <summary>
    /// 播放打字音效（带音调变化）
    /// </summary>
    void PlayTypingSound(AudioClip clip)
    {
        if (typeSoundSource == null || clip == null) return;

        typeSoundSource.clip = clip;
        typeSoundSource.volume = typeSoundVolume;

        // 随机音调变化
        typeSoundSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);

        typeSoundSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 播放普通音效
    /// </summary>
    void PlaySound(AudioClip clip, AudioSource source)
    {
        if (clip == null || source == null) return;

        source.PlayOneShot(clip);
    }

    /// <summary>
    /// 显示/隐藏继续提示
    /// </summary>
    void ShowContinuePrompt(bool show)
    {
        if (continuePrompt != null)
            continuePrompt.SetActive(show);
    }

    /// <summary>
    /// 跳过打字效果
    /// </summary>
    void CompleteTyping()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        // 停止打字音效
        if (typeSoundSource != null)
            typeSoundSource.Stop();

        // 立即显示完整文本
        if (currentDialogue != null && currentLineIndex < currentDialogue.lines.Length)
        {
            if (dialogueText != null)
                dialogueText.text = currentDialogue.lines[currentLineIndex].content;
        }

        isTyping = false;
        ShowContinuePrompt(true);

        if (showDebugLog)
            Debug.Log("[DialogueSystem] 跳过打字效果");
    }

    /// <summary>
    /// 显示下一行
    /// </summary>
    void ShowNextLine()
    {
        currentLineIndex++;
        ShowCurrentLine();
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    void EndDialogue()
    {
        // 播放对话结束音效
        PlaySound(dialogueEndSound, typeSoundSource);

        // 停止所有音频
        if (voiceSource != null)
            voiceSource.Stop();

        if (musicSource != null)
            musicSource.Stop();

        // 隐藏面板
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        ShowContinuePrompt(false);

        if (showDebugLog)
            Debug.Log("[DialogueSystem] 对话结束");

        // 执行回调
        onCompleteCallback?.Invoke();
        onCompleteCallback = null;

        currentDialogue = null;
        canProceed = false;
    }

    /// <summary>
    /// 强制停止对话
    /// </summary>
    public void StopDialogue()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        EndDialogue();
    }

    /// <summary>
    /// 暂停/恢复对话
    /// </summary>
    public void SetPaused(bool paused)
    {
        canProceed = !paused;

        if (voiceSource != null)
        {
            if (paused)
                voiceSource.Pause();
            else
                voiceSource.UnPause();
        }

        if (musicSource != null)
        {
            if (paused)
                musicSource.Pause();
            else
                musicSource.UnPause();
        }
    }

    /// <summary>
    /// 获取当前对话进度
    /// </summary>
    public float GetProgress()
    {
        if (currentDialogue == null || currentDialogue.lines.Length == 0)
            return 0f;

        return (float)(currentLineIndex + 1) / currentDialogue.lines.Length;
    }
}
