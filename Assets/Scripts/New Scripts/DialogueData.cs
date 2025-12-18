using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "GameSystem/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        [Header("对话内容")]
        [TextArea(2, 5)]
        public string content;

        [Header("显示设置")]
        [Tooltip("自动播放时的显示时长（秒）")]
        public float duration = 3f;

        [Tooltip("打字速度（秒/字符），0表示使用全局设置")]
        public float customTypeSpeed = 0f;

        [Header("音效设置")]
        [Tooltip("这句话的配音（整句播放）")]
        public AudioClip voiceClip;

        [Tooltip("自定义打字音效（留空使用全局设置）")]
        public AudioClip customTypeSound;

        [Tooltip("配音音量（0-1）")]
        [Range(0f, 1f)]
        public float voiceVolume = 1f;
    }

    [Header("对话配置")]
    public string dialogueID;
    public DialogueLine[] lines;

    [Header("对话类型")]
    public bool isNarration = true;

    [Header("全局音效（可选）")]
    [Tooltip("整段对话的背景音乐")]
    public AudioClip backgroundMusic;

    [Range(0f, 1f)]
    public float musicVolume = 0.3f;
}
