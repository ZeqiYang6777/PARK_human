using UnityEngine;
using System.Collections;

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance;

    [Header("===== System Components =====")]
    public DialogueSystem dialogueSystem;
    public MissionSystem missionSystem;
    public MissionUIManager missionUI;
    public CameraSequenceManager cameraSequenceManager;
    public GameObject player;

    [Header("===== Dialogue Data =====")]
    public DialogueData dialogue1;
    public DialogueData dialogue2;

    [Header("===== Mission Data =====")]
    public MissionData mission1;
    public MissionData mission2;

    [Header("===== Cinematic Settings =====")]
    [Tooltip("Camera index for cinematic sequence")]
    public int cinematicCameraIndex = 1;

    [Tooltip("Duration of cinematic in seconds")]
    public float cinematicDuration = 5f;

    private bool triggered = false;
    private bool isCinematicPlaying = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
        Debug.Log("[GameFlow] Step1: Narration 1");
        SetControl(false);
        dialogueSystem.StartDialogue(dialogue1, Step2);
    }

    void Step2()
    {
        Debug.Log("[GameFlow] Step2: Mission 1");
        SetControl(true);
        missionSystem.SetCurrentMission(mission1);
        missionUI.ShowMissionButton(true);
    }

    public void Step3_TriggerCinematic()
    {
        if (triggered) return;
        triggered = true;

        Debug.Log("[GameFlow] Step3: Playing Cinematic");
        SetControl(false);
        missionUI.HideAllUI();

        StartCoroutine(PlayCinematicSequence());
    }

    private IEnumerator PlayCinematicSequence()
    {
        if (cameraSequenceManager == null)
        {
            Debug.LogError("[GameFlow] CameraSequenceManager is not assigned!");
            Step4();
            yield break;
        }

        isCinematicPlaying = true;

        cameraSequenceManager.SwitchToCamera(cinematicCameraIndex);

        Debug.Log("[GameFlow] Cinematic is playing...");

        yield return new WaitForSeconds(cinematicDuration);

        isCinematicPlaying = false;

        Debug.Log("[GameFlow] Cinematic finished");

        Step4();
    }

    void Step4()
    {
        Debug.Log("[GameFlow] Step4: Narration 2");

        if (cameraSequenceManager != null)
        {
            cameraSequenceManager.SwitchToCamera(0);
        }

        dialogueSystem.StartDialogue(dialogue2, Step5);
    }

    void Step5()
    {
        Debug.Log("[GameFlow] Step5: Mission 2");
        SetControl(true);
        missionSystem.SetCurrentMission(mission2);
        missionUI.ShowMissionButton(true);
    }

    void SetControl(bool enabled)
    {
        if (player == null)
        {
            Debug.LogWarning("[GameFlow] Player object is not assigned!");
            return;
        }

        MonoBehaviour[] components = player.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component == null) continue;

            string typeName = component.GetType().Name;

            if (typeName.Contains("Controller") ||
                typeName.Contains("Input") ||
                typeName.Contains("Movement"))
            {
                component.enabled = enabled;

                // Log control change
                string status = enabled ? "Enabled" : "Disabled";
                Debug.Log("[GameFlow] Player control " + status + ": " + typeName);
            }
        }
    }

    public bool IsCinematicPlaying()
    {
        return isCinematicPlaying;
    }

    public void StopCinematic()
    {
        if (isCinematicPlaying)
        {
            StopCoroutine(PlayCinematicSequence());
            isCinematicPlaying = false;
            Step4();

            Debug.Log("[GameFlow] Cinematic stopped manually");
        }
    }
}
