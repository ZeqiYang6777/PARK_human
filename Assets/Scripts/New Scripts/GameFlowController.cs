using UnityEngine;
using System.Collections;

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance;

    public DialogueSystem dialogueSystem;
    public MissionSystem missionSystem;
    public MissionUIManager missionUI;
    public CinematicController cinematicController;
    public GameObject player;

    public DialogueData dialogue1;
    public DialogueData dialogue2;
    public MissionData mission1;
    public MissionData mission2;

    private bool triggered = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(Begin());
    }

    IEnumerator Begin()
    {
        yield return new WaitForSeconds(0.5f);
        Step1();
    }

    void Step1()
    {
        Debug.Log("【步骤1】旁白1");
        SetControl(false);
        dialogueSystem.StartDialogue(dialogue1, Step2);
    }

    void Step2()
    {
        Debug.Log("【步骤2】任务1");
        SetControl(true);
        missionSystem.SetCurrentMission(mission1);
        missionUI.ShowMissionButton(true);
    }

    public void Step3_TriggerCinematic()
    {
        if (triggered) return;
        triggered = true;

        Debug.Log("【步骤3】过场动画");
        SetControl(false);
        missionUI.HideAllUI();
        cinematicController.PlaySequence(Step4);
    }

    void Step4()
    {
        Debug.Log("【步骤4】旁白2");
        dialogueSystem.StartDialogue(dialogue2, Step5);
    }

    void Step5()
    {
        Debug.Log("【步骤5】任务2");
        SetControl(true);
        missionSystem.SetCurrentMission(mission2);
        missionUI.ShowMissionButton(true);
    }

    void SetControl(bool enabled)
    {
        if (!player) return;
        foreach (var s in player.GetComponents<MonoBehaviour>())
        {
            var name = s.GetType().Name;
            if (name.Contains("Controller") || name.Contains("Input"))
                s.enabled = enabled;
        }
    }
}
