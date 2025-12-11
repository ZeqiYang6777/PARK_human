using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("UIÒýÓÃ")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("ÉèÖÃ")]
    public float typeSpeed = 0.05f;
    public bool useTypewriterEffect = true;

    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private Action onCompleteCallback;
    private bool isTyping = false;
    private Coroutine typeCoroutine;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                {
                    StopTypewriter();
                }
                else
                {
                    ShowNextLine();
                }
            }
        }
    }

    public void StartDialogue(DialogueData dialogue, Action onComplete = null)
    {
        if (dialogue == null)
        {
            Debug.LogError("DialogueData is null!");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        onCompleteCallback = onComplete;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        var line = currentDialogue.lines[currentLineIndex];

        if (useTypewriterEffect)
        {
            if (typeCoroutine != null)
                StopCoroutine(typeCoroutine);
            typeCoroutine = StartCoroutine(TypewriterEffect(line.content));
        }
        else
        {
            if (dialogueText != null)
                dialogueText.text = line.content;
        }
    }

    IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    void StopTypewriter()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            if (currentDialogue != null && currentLineIndex < currentDialogue.lines.Length)
            {
                dialogueText.text = currentDialogue.lines[currentLineIndex].content;
            }
            isTyping = false;
        }
    }

    void ShowNextLine()
    {
        currentLineIndex++;
        ShowCurrentLine();
    }

    void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        onCompleteCallback?.Invoke();
        onCompleteCallback = null;
    }
}
