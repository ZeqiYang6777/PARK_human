using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class ImageNarratorController : MonoBehaviour
{
    [Header("UI设置")]
    [SerializeField] private Image narratorImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [Header("动画设置")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeOutDuration = 1f;
    [Header("触发设置")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float startDelay = 1f;
    [Header("事件")]
    public UnityEvent OnNarrationComplete;

    void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
        }
        if (playOnStart)
        {
            StartCoroutine(StartNarrationWithDelay());
        }
    }
    IEnumerator StartNarrationWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        PlayNarration();
    }
    public void PlayNarration()
    {
        StartCoroutine(NarrationSequence());
    }
    IEnumerator NarrationSequence()
    {
        if (canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeInDuration));
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeOutDuration));
            canvasGroup.gameObject.SetActive(false);
        }
        OnNarrationComplete?.Invoke();
    }
    IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }
    public void SkipNarration()
    {
        StopAllCoroutines();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
        }
        OnNarrationComplete?.Invoke();
    }
}