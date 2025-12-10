using PixelCrushers.DialogueSystem;
using UnityEngine;

public class OpeningDialogue : MonoBehaviour
{
    [Header("对话设置")]
    [Tooltip("要播放的对话名称")]
    public string conversationTitle = "Zone1_Dialogue";

    [Header("触发方式")]
    [Tooltip("勾选后在Start时自动播放")]
    public bool playOnStart = true;

    void Start()
    {
        if (playOnStart)
        {
            PlayDialogue();
        }
    }

    /// <summary>
    /// 播放对话（可以从外部调用）
    /// </summary>
    public void PlayDialogue()
    {
        if (!string.IsNullOrEmpty(conversationTitle))
        {
            Debug.Log($"🎤 开始播放对话: {conversationTitle}");
            DialogueManager.StartConversation(conversationTitle);
        }
        else
        {
            Debug.LogError("❌ 对话名称为空！请在 Inspector 中设置 Conversation Title");
        }
    }

    /// <summary>
    /// 播放指定的对话（可以从外部调用）
    /// </summary>
    public void PlayDialogue(string dialogueName)
    {
        if (!string.IsNullOrEmpty(dialogueName))
        {
            Debug.Log($"🎤 开始播放对话: {dialogueName}");
            DialogueManager.StartConversation(dialogueName);
        }
    }
}
