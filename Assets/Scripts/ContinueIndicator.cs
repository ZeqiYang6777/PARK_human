using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContinueIndicator : MonoBehaviour
{
    public float blinkSpeed = 1.5f;
    private Image image;
    private TextMeshProUGUI tmpText;
    private Text legacyText;

    void Start()
    {
        image = GetComponent<Image>();
        tmpText = GetComponent<TextMeshProUGUI>();
        legacyText = GetComponent<Text>();
    }

    void Update()
    {
        float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);

        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        if (tmpText != null)
        {
            Color color = tmpText.color;
            color.a = alpha;
            tmpText.color = color;
        }

        if (legacyText != null)
        {
            Color color = legacyText.color;
            color.a = alpha;
            legacyText.color = color;
        }
    }
}
