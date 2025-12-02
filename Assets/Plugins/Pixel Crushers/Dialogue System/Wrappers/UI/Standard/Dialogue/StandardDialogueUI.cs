// NarratorManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NarratorManager : MonoBehaviour
{
    // 单例模式，方便全局访问
    private static NarratorManager _instance;
    public static NarratorManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NarratorManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("NarratorManager");
                    _instance = go.AddComponent<NarratorManager>();
                }
            }
            return _instance;
        }
    }

    [Header("UI引用 - 拖拽赋值")]
    [SerializeField] private GameObject narratorCanvas; // 整个UI画布
    [SerializeField] private TextMeshProUGUI characterNameText; // 角色名文本
    [SerializeField] private TextMeshProUGUI contentText; // 内容文本
    [SerializeField] private GameObject continueHint; // 继续提示
    [SerializeField] private Image background; // 背景图

    [Header("设置")]
    [SerializeField] private float typingSpeed = 0.05f; // 打字速度
    [SerializeField] private bool autoHideContinueHint = true; // 打字时自动隐藏提示

    private Queue<string> dialogueQueue = new Queue<string>(); // 文本队列
    private bool isTyping = false; // 是否正在打字
    private Coroutine typingCoroutine; // 打字协程
    private Action onDialogueComplete; // 对话完成的回调

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // 确保在Awake中初始化，不要在其他脚本的Start中调用
        InitializeUI();
    }

    // 初始化UI状态
    private void InitializeUI()
    {
        if (narratorCanvas != null)
        {
            narratorCanvas.SetActive(false);
        }
    }

    // 开始显示旁白（单条）
    public void ShowNarration(string content, string characterName = "旁白", Action onComplete = null)
    {
        ShowNarration(new string[] { content }, characterName, onComplete);
    }

    // 开始显示旁白（多条）
    public void ShowNarration(string[] contents, string characterName = "旁白", Action onComplete = null)
    {
        // 如果已经在显示，先停止
        if (narratorCanvas.activeSelf)
        {
            StopCurrentDialogue();
        }

        // 设置角色名
        if (characterNameText != null)
        {
            characterNameText.text = characterName;
        }

        // 清空队列并添加新内容
        dialogueQueue.Clear();
        foreach (string content in contents)
        {
            dialogueQueue.Enqueue(content);
        }

        // 设置完成回调
        onDialogueComplete = onComplete;

        // 显示UI并开始第一句
        narratorCanvas.SetActive(true);
        ShowNextLine();
    }

    // 显示下一行文本
    private void ShowNextLine()
    {
        // 如果正在打字，立即完成当前打字
        if (isTyping)
        {
            CompleteCurrentTyping();
            return;
        }

        // 检查队列是否为空
        if (dialogueQueue.Count == 0)
        {
            CloseNarrator();
            return;
        }

        // 获取下一行文本
        string nextLine = dialogueQueue.Dequeue();

        // 开始打字效果
        typingCoroutine = StartCoroutine(TypeTextCoroutine(nextLine));
    }

    // 打字机效果协程
    private IEnumerator TypeTextCoroutine(string text)
    {
        isTyping = true;

        // 打字时隐藏继续提示
        if (autoHideContinueHint && continueHint != null)
        {
            continueHint.SetActive(false);
        }

        // 清空文本
        contentText.text = "";

        // 逐字显示
        foreach (char letter in text.ToCharArray())
        {
            contentText.text += letter;

            // 可以在这里添加打字音效
            // AudioManager.Instance.PlayTypingSound();

            yield return new WaitForSeconds(typingSpeed);
        }

        // 打字完成
        isTyping = false;

        // 显示继续提示
        if (continueHint != null)
        {
            continueHint.SetActive(true);
        }
    }

    // 立即完成当前打字
    private void CompleteCurrentTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        isTyping = false;
        if (continueHint != null)
        {
            continueHint.SetActive(true);
        }
    }

    // 停止当前对话
    private void StopCurrentDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        dialogueQueue.Clear();
        isTyping = false;
    }

    // 关闭旁白UI
    private void CloseNarrator()
    {
        narratorCanvas.SetActive(false);

        // 触发完成回调
        onDialogueComplete?.Invoke();
        onDialogueComplete = null;
    }

    // 提供给UI按钮调用的继续方法
    public void OnContinueButtonClicked()
    {
        ShowNextLine();
    }

    // 跳过所有旁白
    public void SkipAll()
    {
        StopCurrentDialogue();
        CloseNarrator();
    }

    // 设置打字速度
    public void SetTypingSpeed(float speed)
    {
        typingSpeed = Mathf.Max(0.01f, speed);
    }
}