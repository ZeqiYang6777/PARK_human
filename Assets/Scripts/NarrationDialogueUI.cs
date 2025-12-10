using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NarrationDialogueUI : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button nextButton;

    [Header("对话数据")]
    [SerializeField] private DialogueData currentDialogue;

    [Header("打字机效果")]
    [SerializeField] private bool useTypewriterEffect = true;
    [SerializeField] private float typewriterSpeed = 0.05f;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;

    void Start()
    {
        // 初始化
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // 绑定按钮事件
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    /// <summary>
    /// 播放对话
    /// </summary>
    public void PlayDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.lines.Count == 0)
        {
            Debug.LogError("❌ 对话数据为空或没有对话内容！");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowLine(currentLineIndex);
    }

    /// <summary>
    /// 显示指定行的对话
    /// </summary>
    void ShowLine(int index)
    {
        if (currentDialogue == null || index >= currentDialogue.lines.Count)
        {
            EndDialogue();
            return;
        }

        var line = currentDialogue.lines[index];

        // 设置说话者名称
        if (speakerNameText != null)
            speakerNameText.text = line.speakerName;

        // 显示对话文本
        if (useTypewriterEffect)
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);

            typewriterCoroutine = StartCoroutine(TypewriterEffect(line.text));
        }
        else
        {
            if (dialogueText != null)
                dialogueText.text = line.text;
        }

        Debug.Log($"💬 显示对话: [{line.speakerName}] {line.text}");
    }

    /// <summary>
    /// 打字机效果协程
    /// </summary>
    IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;

        // 如果启用自动播放
        if (currentDialogue.autoPlay)
        {
            yield return new WaitForSeconds(currentDialogue.autoPlayDelay);
            NextLine();
        }
    }

    /// <summary>
    /// 下一句按钮点击
    /// </summary>
    void OnNextButtonClicked()
    {
        if (isTyping)
        {
            // 如果正在打字，直接显示完整文本
            StopCoroutine(typewriterCoroutine);
            dialogueText.text = currentDialogue.lines[currentLineIndex].text;
            isTyping = false;
        }
        else
        {
            NextLine();
        }
    }

    /// <summary>
    /// 显示下一句对话
    /// </summary>
    void NextLine()
    {
        currentLineIndex++;
        ShowLine(currentLineIndex);
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    void EndDialogue()
    {
        Debug.Log("✅ 对话结束");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        currentDialogue = null;
        currentLineIndex = 0;
    }

    /// <summary>
    /// 外部调用：立即结束对话
    /// </summary>
    public void StopDialogue()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        EndDialogue();
    }
}
