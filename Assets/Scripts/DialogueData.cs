using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string text;
    }

    [Header("对话内容")]
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("对话设置")]
    [Tooltip("对话的唯一ID")]
    public string dialogueID;
}
