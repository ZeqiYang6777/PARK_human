using UnityEngine;

public class LoadingCircleRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed (degrees per second)")]
    public float rotationSpeed = 180f;

    [Tooltip("Rotation direction (-1 = counterclockwise, 1 = clockwise)")]
    public int rotationDirection = -1;

    [Header("Optional: Pulse Effect")]
    [Tooltip("Enable scale pulse effect")]
    public bool enablePulse = true;

    [Tooltip("Pulse speed")]
    public float pulseSpeed = 2f;

    [Tooltip("Minimum scale")]
    public float minScale = 0.9f;

    [Tooltip("Maximum scale")]
    public float maxScale = 1.1f;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private float pulseTimer = 0f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Rotate the loading circle
        float rotation = rotationSpeed * rotationDirection * Time.deltaTime;
        rectTransform.Rotate(0, 0, rotation);

        // Apply pulse effect if enabled
        if (enablePulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(pulseTimer) + 1f) / 2f);
            transform.localScale = originalScale * scale;
        }
    }
}
