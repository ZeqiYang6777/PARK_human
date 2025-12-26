using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MissionUIManager : MonoBehaviour
{
    [Header("=== UI References ===")]
    public Button missionButton;
    public CanvasGroup missionButtonCanvasGroup;
    public GameObject missionListPanel;
    public RectTransform missionListRect;
    public Image missionImage;
    public TextMeshProUGUI missionTitle;
    public TextMeshProUGUI missionDesc;

    [Header("=== Settings ===")]
    public KeyCode toggleKey = KeyCode.M;

    [Header("=== Animation Settings ===")]
    [Range(0.1f, 2f)]
    public float openDuration = 0.5f;
    [Range(0.1f, 2f)]
    public float closeDuration = 0.4f;
    [Range(0f, 360f)]
    public float openRotation = 180f;
    [Range(0f, 360f)]
    public float closeRotation = 90f;
    public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve closeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Vector2 closeOffset = new Vector2(400f, -300f);

    [Header("=== Button Animation Settings ===")]
    public bool hideButtonOnOpen = true;
    [Range(0.1f, 1f)]
    public float buttonFadeOutDuration = 0.2f;
    [Range(0.1f, 1f)]
    public float buttonFadeInDuration = 0.3f;
    public bool shrinkButtonOnFade = true;
    [Range(0f, 1f)]
    public float buttonShrinkScale = 0.5f;

    [Header("=== Audio ===")]
    public AudioSource uiAudioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    [Header("=== Button Audio ===")]
    [Tooltip("Button click sound effect")]
    public AudioClip buttonClickSound;

    [Tooltip("Button hover sound effect (optional)")]
    public AudioClip buttonHoverSound;

    [Tooltip("Button sound volume")]
    [Range(0f, 1f)]
    public float buttonSoundVolume = 1f;

    private bool isListVisible = false;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 buttonOriginalScale;

    void Start()
    {
        if (MissionSystem.Instance != null)
        {
            MissionSystem.Instance.OnMissionChanged += UpdateMissionDisplay;
        }

        if (missionButton != null)
        {
            missionButton.onClick.AddListener(PlayButtonClickSound);
            missionButton.onClick.AddListener(ToggleMissionList);

            buttonOriginalScale = missionButton.transform.localScale;

            if (missionButtonCanvasGroup == null)
            {
                missionButtonCanvasGroup = missionButton.GetComponent<CanvasGroup>();
                if (missionButtonCanvasGroup == null)
                {
                    missionButtonCanvasGroup = missionButton.gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        if (missionListPanel != null && missionListRect == null)
        {
            missionListRect = missionListPanel.GetComponent<RectTransform>();
        }

        if (missionListRect != null)
        {
            originalScale = missionListRect.localScale;
            originalPosition = missionListRect.localPosition;
            originalRotation = missionListRect.localRotation;
        }

        // Setup audio source
        if (uiAudioSource == null)
        {
            GameObject audioObj = new GameObject("UIAudioSource");
            audioObj.transform.SetParent(transform);
            uiAudioSource = audioObj.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
            uiAudioSource.spatialBlend = 0f; // 2D audio
        }

        HideAllUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (missionButton != null && missionButton.gameObject.activeSelf && !isAnimating)
            {
                PlayButtonClickSound();
                ToggleMissionList();
            }
        }
    }

    void PlayButtonClickSound()
    {
        if (buttonClickSound != null && uiAudioSource != null)
        {
            uiAudioSource.volume = buttonSoundVolume;
            uiAudioSource.PlayOneShot(buttonClickSound);

            Debug.Log("Playing button click sound");
        }
        else
        {
            if (buttonClickSound == null)
                Debug.LogWarning("Button click sound not set");
            if (uiAudioSource == null)
                Debug.LogWarning("UI audio source not set");
        }
    }

    public void ShowMissionButton(bool show)
    {
        if (missionButton != null)
            missionButton.gameObject.SetActive(show);
    }

    public void ToggleMissionList()
    {
        if (isAnimating) return;

        if (isListVisible)
        {
            CloseMissionList();
        }
        else
        {
            OpenMissionList();
        }
    }

    public void OpenMissionList()
    {
        if (isAnimating || isListVisible) return;
        StartCoroutine(OpenAnimation());
    }

    public void CloseMissionList()
    {
        if (isAnimating || !isListVisible) return;
        StartCoroutine(CloseAnimation());
    }

    IEnumerator OpenAnimation()
    {
        isAnimating = true;
        isListVisible = true;

        if (hideButtonOnOpen && missionButton != null)
        {
            StartCoroutine(FadeOutButton());
        }

        if (missionListPanel != null)
            missionListPanel.SetActive(true);

        PlaySound(openSound);

        if (missionListRect == null)
        {
            isAnimating = false;
            yield break;
        }

        Vector3 startPos = originalPosition + new Vector3(closeOffset.x, closeOffset.y, 0);
        Vector3 startScale = Vector3.zero;
        Quaternion startRotation = Quaternion.Euler(0, 0, openRotation);

        Vector3 endPos = originalPosition;
        Vector3 endScale = originalScale;
        Quaternion endRotation = originalRotation;

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / openDuration;
            float curveT = openCurve.Evaluate(t);

            missionListRect.localPosition = Vector3.Lerp(startPos, endPos, curveT);
            missionListRect.localScale = Vector3.Lerp(startScale, endScale, curveT);
            missionListRect.localRotation = Quaternion.Lerp(startRotation, endRotation, curveT);

            yield return null;
        }

        missionListRect.localPosition = endPos;
        missionListRect.localScale = endScale;
        missionListRect.localRotation = endRotation;

        isAnimating = false;
    }

    IEnumerator CloseAnimation()
    {
        isAnimating = true;

        PlaySound(closeSound);

        if (missionListRect == null)
        {
            if (missionListPanel != null)
                missionListPanel.SetActive(false);

            isListVisible = false;
            isAnimating = false;
            yield break;
        }

        Vector3 startPos = originalPosition;
        Vector3 startScale = originalScale;
        Quaternion startRotation = originalRotation;

        Vector3 endPos = originalPosition + new Vector3(closeOffset.x, closeOffset.y, 0);
        Vector3 endScale = Vector3.zero;
        Quaternion endRotation = Quaternion.Euler(0, 0, closeRotation);

        float elapsed = 0f;

        while (elapsed < closeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / closeDuration;
            float curveT = closeCurve.Evaluate(t);

            missionListRect.localPosition = Vector3.Lerp(startPos, endPos, curveT);
            missionListRect.localScale = Vector3.Lerp(startScale, endScale, curveT);
            missionListRect.localRotation = Quaternion.Lerp(startRotation, endRotation, curveT);

            yield return null;
        }

        missionListRect.localPosition = endPos;
        missionListRect.localScale = endScale;
        missionListRect.localRotation = endRotation;

        if (missionListPanel != null)
            missionListPanel.SetActive(false);

        missionListRect.localPosition = originalPosition;
        missionListRect.localScale = originalScale;
        missionListRect.localRotation = originalRotation;

        isListVisible = false;
        isAnimating = false;

        if (hideButtonOnOpen && missionButton != null)
        {
            StartCoroutine(FadeInButton());
        }
    }

    IEnumerator FadeOutButton()
    {
        if (missionButtonCanvasGroup == null) yield break;

        float elapsed = 0f;
        float startAlpha = missionButtonCanvasGroup.alpha;
        Vector3 startScale = missionButton.transform.localScale;
        Vector3 targetScale = shrinkButtonOnFade ? buttonOriginalScale * buttonShrinkScale : buttonOriginalScale;

        while (elapsed < buttonFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / buttonFadeOutDuration;

            missionButtonCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            if (shrinkButtonOnFade)
            {
                missionButton.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            }

            yield return null;
        }

        missionButtonCanvasGroup.alpha = 0f;
        if (shrinkButtonOnFade)
        {
            missionButton.transform.localScale = targetScale;
        }

        missionButtonCanvasGroup.interactable = false;
        missionButtonCanvasGroup.blocksRaycasts = false;
    }

    IEnumerator FadeInButton()
    {
        if (missionButtonCanvasGroup == null) yield break;

        missionButtonCanvasGroup.interactable = true;
        missionButtonCanvasGroup.blocksRaycasts = true;

        float elapsed = 0f;
        float startAlpha = missionButtonCanvasGroup.alpha;
        Vector3 startScale = missionButton.transform.localScale;

        while (elapsed < buttonFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / buttonFadeInDuration;

            missionButtonCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);

            if (shrinkButtonOnFade)
            {
                missionButton.transform.localScale = Vector3.Lerp(startScale, buttonOriginalScale, t);
            }

            yield return null;
        }

        missionButtonCanvasGroup.alpha = 1f;
        missionButton.transform.localScale = buttonOriginalScale;
    }

    public void HideAllUI()
    {
        ShowMissionButton(false);

        if (missionListPanel != null)
        {
            missionListPanel.SetActive(false);
        }

        isListVisible = false;
        isAnimating = false;
    }

    public void CloseMissionListImmediate()
    {
        StopAllCoroutines();

        if (missionListPanel != null)
            missionListPanel.SetActive(false);

        if (missionListRect != null)
        {
            missionListRect.localPosition = originalPosition;
            missionListRect.localScale = originalScale;
            missionListRect.localRotation = originalRotation;
        }

        if (missionButtonCanvasGroup != null)
        {
            missionButtonCanvasGroup.alpha = 1f;
            missionButtonCanvasGroup.interactable = true;
            missionButtonCanvasGroup.blocksRaycasts = true;
        }

        if (missionButton != null)
        {
            missionButton.transform.localScale = buttonOriginalScale;
        }

        isListVisible = false;
        isAnimating = false;
    }

    void UpdateMissionDisplay(MissionData mission)
    {
        if (mission == null) return;

        if (missionImage != null && mission.missionImage != null)
            missionImage.sprite = mission.missionImage;

        if (missionTitle != null)
            missionTitle.text = mission.missionTitle;

        if (missionDesc != null)
            missionDesc.text = mission.description;

        Debug.Log("[OK] UI Update: " + mission.missionTitle);
    }

    void PlaySound(AudioClip clip)
    {
        if (uiAudioSource != null && clip != null)
        {
            uiAudioSource.PlayOneShot(clip);
        }
    }

    void OnDestroy()
    {
        if (MissionSystem.Instance != null)
        {
            MissionSystem.Instance.OnMissionChanged -= UpdateMissionDisplay;
        }
    }
}
