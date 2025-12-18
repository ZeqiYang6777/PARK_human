using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AutoSelectButton : MonoBehaviour
{
    public Button buttonToSelect;

    void OnEnable()
    {
        // 面板显示时自动选中按钮
        if (buttonToSelect != null)
        {
            EventSystem.current.SetSelectedGameObject(buttonToSelect.gameObject);
        }
    }

    void Update()
    {
        // 如果没有选中任何对象，重新选中按钮
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (buttonToSelect != null)
            {
                EventSystem.current.SetSelectedGameObject(buttonToSelect.gameObject);
            }
        }
    }
}
