using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("=== UI References ===")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject continuePrompt;

    [Header("=== Typewriter Effect Settings ===")]
    [Tooltip("Enable typewriter effect")]
    public bool useTypewriterEffect = true;

    [Tooltip("Typing speed (seconds per character)")]
    [Range(0.01f, 0.2f)]
    public float typeSpeed = 0.05f;

    [Tooltip("Extra pause time for punctuation")]
    [Range(0f, 0.5f)]
    public float punctuationPause = 0.15f;

    [Tooltip("Punctuation marks that trigger extra pause")]
    public string punctuationMarks = "¡££¡£¿£¬£»£º¡­";

    [Header("=== Audio Settings ===")]
    [Tooltip("Audio source for typing sound")]
    public AudioSource typeSoundSource;

    [Tooltip("Audio source for voice")]
    public AudioSource voiceSource;

    [Tooltip("Audio source for background music")]
    public AudioSource musicSource;

    [Header("Typing Sound Effects")]
    [Tooltip("Default typing sound effect")]
    public AudioClip defaultTypeSound;

    [Tooltip("Typing sound volume")]
    [Range(0f, 1f)]
    public float typeSoundVolume = 0.3f;

    [Tooltip("Random pitch variation range")]
    [Range(0f, 0.5f)]
    public float pitchVariation = 0.1f;

    [Tooltip("Play typing sound every N characters (1=every char, 2=every 2 chars)")]
    [Range(1, 5)]
    public int soundFrequency = 1;

    [Header("Special Sound Effects")]
    [Tooltip("Dialogue start sound effect")]
    public AudioClip dialogueStartSound;

    [Tooltip("Dialogue end sound effect")]
    public AudioClip dialogueEndSound;

    [Header("=== Debug Options ===")]
    public bool showDebugLog = false;

    // Private variables
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

        // Initialize audio sources
        InitializeAudioSources();
    }

    void Update()
    {
        if (dialoguePanel != null && dialoguePanel.activeSelf && canProceed)
        {
            // Space key or left mouse button to continue
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                {
                    // Skip typing effect
                    CompleteTyping();
                }
                else
                {
                    // Next line
                    ShowNextLine();
                }
            }
        }
    }

    void InitializeAudioSources()
    {
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

    public void StartDialogue(DialogueData dialogue, Action onComplete = null)
    {
        if (dialogue == null)
        {
            Debug.LogError("[DialogueSystem] DialogueData is null!");
            return;
        }

        if (dialogue.lines == null || dialogue.lines.Length == 0)
        {
            Debug.LogError("[DialogueSystem] Dialogue data has no lines!");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        onCompleteCallback = onComplete;
        canProceed = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Play dialogue start sound
        PlaySound(dialogueStartSound, typeSoundSource);

        // Play background music
        if (dialogue.backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = dialogue.backgroundMusic;
            musicSource.volume = dialogue.musicVolume;
            musicSource.Play();
        }

        if (showDebugLog)
            Debug.Log("[DialogueSystem] Starting dialogue: " + dialogue.dialogueID);

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

        if (showDebugLog)
        {
            int previewLength = Mathf.Min(20, line.content.Length);
            string preview = line.content.Substring(0, previewLength);
            Debug.Log("[DialogueSystem] Line " + (currentLineIndex + 1) + ": " + preview + "...");
        }

        // Stop previous voice
        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        // Play voice clip
        if (line.voiceClip != null && voiceSource != null)
        {
            voiceSource.clip = line.voiceClip;
            voiceSource.volume = line.voiceVolume;
            voiceSource.Play();
        }

        // Display text
        if (useTypewriterEffect)
        {
            if (typeCoroutine != null)
                StopCoroutine(typeCoroutine);

            float speed = line.customTypeSpeed > 0 ? line.customTypeSpeed : typeSpeed;
            AudioClip typeSound = line.customTypeSound != null ? line.customTypeSound : defaultTypeSound;

            typeCoroutine = StartCoroutine(TypewriterEffect(line.content, speed, typeSound));
        }
        else
        {
            // Display immediately
            if (dialogueText != null)
                dialogueText.text = line.content;

            isTyping = false;
            ShowContinuePrompt(true);
        }
    }

    IEnumerator TypewriterEffect(string text, float speed, AudioClip typeSound)
    {
        isTyping = true;
        characterCount = 0;

        if (dialogueText != null)
            dialogueText.text = "";

        ShowContinuePrompt(false);

        // Display character by character
        foreach (char c in text)
        {
            if (dialogueText != null)
                dialogueText.text += c;

            characterCount++;

            // Play typing sound
            if (typeSound != null && characterCount % soundFrequency == 0)
            {
                PlayTypingSound(typeSound);
            }

            // Base wait time
            float waitTime = speed;

            // Extra pause for punctuation
            if (punctuationMarks.Contains(c.ToString()))
            {
                waitTime += punctuationPause;
            }

            yield return new WaitForSeconds(waitTime);
        }

        isTyping = false;
        ShowContinuePrompt(true);

        if (showDebugLog)
            Debug.Log("[DialogueSystem] Typing complete");
    }

    void PlayTypingSound(AudioClip clip)
    {
        if (typeSoundSource == null || clip == null) return;

        typeSoundSource.clip = clip;
        typeSoundSource.volume = typeSoundVolume;

        // Random pitch variation
        typeSoundSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);

        typeSoundSource.PlayOneShot(clip);
    }

    void PlaySound(AudioClip clip, AudioSource source)
    {
        if (clip == null || source == null) return;

        source.PlayOneShot(clip);
    }

    void ShowContinuePrompt(bool show)
    {
        if (continuePrompt != null)
            continuePrompt.SetActive(show);
    }

    void CompleteTyping()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        // Stop typing sound
        if (typeSoundSource != null)
            typeSoundSource.Stop();

        // Display full text immediately
        if (currentDialogue != null && currentLineIndex < currentDialogue.lines.Length)
        {
            if (dialogueText != null)
                dialogueText.text = currentDialogue.lines[currentLineIndex].content;
        }

        isTyping = false;
        ShowContinuePrompt(true);

        if (showDebugLog)
            Debug.Log("[DialogueSystem] Skipped typing effect");
    }

    void ShowNextLine()
    {
        currentLineIndex++;
        ShowCurrentLine();
    }

    void EndDialogue()
    {
        // Play dialogue end sound
        PlaySound(dialogueEndSound, typeSoundSource);

        // Stop all audio
        if (voiceSource != null)
            voiceSource.Stop();

        if (musicSource != null)
            musicSource.Stop();

        // Hide panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        ShowContinuePrompt(false);

        if (showDebugLog)
            Debug.Log("[DialogueSystem] Dialogue ended");

        // Execute callback
        onCompleteCallback?.Invoke();
        onCompleteCallback = null;

        currentDialogue = null;
        canProceed = false;
    }

    public void StopDialogue()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        EndDialogue();
    }

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

    public float GetProgress()
    {
        if (currentDialogue == null || currentDialogue.lines.Length == 0)
            return 0f;

        return (float)(currentLineIndex + 1) / currentDialogue.lines.Length;
    }
}
