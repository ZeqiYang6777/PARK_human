using UnityEngine;
using UnityEngine.UI;

public class ImageFader : FaderBase
{
    public Image element;

    protected override Color GetColor()
    {
        return element.color;
    }

    protected override void SetColor(Color color)
    {
        element.color = color;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(FadeIn());
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(FadeOut());
        }
    }
}