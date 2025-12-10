using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        [Header("说话者")]
        public string speakerName = "旁白";

        [Header("对话内容")]
        [TextArea(3, 10)]
        public string text;

        [Header("可选设置")]
        public float displayDuration = 3f;
    }

    [Header("对话列表")]
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("对话设置")]
    public bool autoPlay = true;
    public float autoPlayDelay = 0.5f;
}
