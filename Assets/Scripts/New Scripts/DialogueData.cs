using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Game/Dialogue")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 5)]
        public string content;
        public float duration = 3f;
    }

    public DialogueLine[] lines;
    public bool isNarration = true;
}
