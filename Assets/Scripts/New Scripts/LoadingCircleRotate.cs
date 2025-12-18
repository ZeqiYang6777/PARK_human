using UnityEngine;

public class LoadingCircleRotate : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("旋转速度（度/秒）")]
    public float rotationSpeed = 180f;

    [Tooltip("旋转方向（-1 = 逆时针，1 = 顺时针）")]
    public int rotationDirection = -1;

    [Header("可选：脉冲效果")]
    [Tooltip("启用缩放脉冲")]
    public bool enablePulse = true;

    [Tooltip("脉冲速度")]
    public float pulseSpeed = 2f;

    [Tooltip("最小缩放")]
    public float minScale = 0.9f;

    [Tooltip("最大缩放")]
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
        // 旋转动画
        float rotation = rotationSpeed * rotationDirection * Time.deltaTime;
        rectTransform.Rotate(0, 0, rotation);

        // 可选：脉冲缩放
        if (enablePulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(pulseTimer) + 1f) / 2f);
            transform.localScale = originalScale * scale;
        }
    }
}
