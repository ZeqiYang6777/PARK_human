using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionUIManager : MonoBehaviour
{
    [Header("UI“˝”√")]
    public Button missionButton;
    public GameObject missionListPanel;
    public Image missionImage;
    public TextMeshProUGUI missionTitle;
    public TextMeshProUGUI missionDesc;

    [Header("…Ë÷√")]
    public KeyCode toggleKey = KeyCode.M;

    private bool isListVisible = false;
    private MissionSystem missionSystem;

    void Start()
    {
        missionSystem = MissionSystem.Instance;

        if (missionSystem != null)
        {
            missionSystem.OnMissionChanged += UpdateMissionDisplay;
        }

        if (missionButton != null)
        {
            missionButton.onClick.AddListener(ToggleMissionList);
        }

        HideAllUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (missionButton != null && missionButton.gameObject.activeSelf)
            {
                ToggleMissionList();
            }
        }
    }

    public void ShowMissionButton(bool show)
    {
        if (missionButton != null)
            missionButton.gameObject.SetActive(show);
    }

    public void ToggleMissionList()
    {
        isListVisible = !isListVisible;
        if (missionListPanel != null)
            missionListPanel.SetActive(isListVisible);
    }

    public void HideAllUI()
    {
        ShowMissionButton(false);
        if (missionListPanel != null)
        {
            missionListPanel.SetActive(false);
            isListVisible = false;
        }
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
    }

    void OnDestroy()
    {
        if (missionSystem != null)
        {
            missionSystem.OnMissionChanged -= UpdateMissionDisplay;
        }
    }
}
