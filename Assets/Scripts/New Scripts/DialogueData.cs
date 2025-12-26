using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "GameSystem/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        [Header("Dialogue Content")]
        [TextArea(2, 5)]
        public string content;

        [Header("Display Settings")]
        [Tooltip("Display duration for auto-play (seconds)")]
        public float duration = 3f;

        [Tooltip("Typing speed (seconds/character), 0 = use global setting")]
        public float customTypeSpeed = 0f;

        [Header("Audio Settings")]
        [Tooltip("Voice clip for this line (plays full sentence)")]
        public AudioClip voiceClip;

        [Tooltip("Custom typing sound (leave empty to use global)")]
        public AudioClip customTypeSound;

        [Tooltip("Voice volume (0-1)")]
        [Range(0f, 1f)]
        public float voiceVolume = 1f;
    }

    [Header("Dialogue Configuration")]
    public string dialogueID;
    public DialogueLine[] lines;

    [Header("Dialogue Type")]
    public bool isNarration = true;

    [Header("Global Audio (Optional)")]
    [Tooltip("Background music for entire dialogue")]
    public AudioClip backgroundMusic;

    [Range(0f, 1f)]
    public float musicVolume = 0.3f;

    /// <summary>
    /// Validate dialogue data
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(dialogueID))
        {
            Debug.LogWarning("[DialogueData] Dialogue has no ID!");
            return false;
        }

        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[DialogueData] Dialogue '" + dialogueID + "' has no lines!");
            return false;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i].content))
            {
                Debug.LogWarning("[DialogueData] Dialogue '" + dialogueID + "' has empty line at index " + i);
                return false;
            }
        }

        return true;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (lines != null)
        {
            foreach (var line in lines)
            {
                line.voiceVolume = Mathf.Clamp01(line.voiceVolume);

                if (line.duration <= 0)
                    line.duration = 3f;
            }
        }

        musicVolume = Mathf.Clamp01(musicVolume);
    }
#endif
}
