using UnityEngine;
using Cinemachine;
using UnityEngine.Playables;

public class CameraSequenceManager : MonoBehaviour
{
    [Header("===== Virtual Camera Settings =====")]
    [Tooltip("Virtual cameras in sequence order")]
    public CinemachineVirtualCamera[] virtualCameras;

    [Header("===== Timeline Settings =====")]
    [Tooltip("Timeline for each camera (optional)")]
    public PlayableDirector[] timelines;

    [Tooltip("Auto-play Timeline when switching")]
    public bool autoPlayTimeline = true;

    [Header("===== Transition Settings =====")]
    [Tooltip("Camera blend time")]
    public float blendTime = 1f;

    [Tooltip("Initial active camera index")]
    public int initialCameraIndex = 0;

    [Header("===== Key Bindings =====")]
    [Tooltip("Switch to next camera")]
    public KeyCode nextCameraKey = KeyCode.N;

    [Tooltip("Switch to previous camera")]
    public KeyCode previousCameraKey = KeyCode.B;

    [Tooltip("Enable number keys for quick switch (1-9)")]
    public bool enableNumberKeys = true;

    [Header("===== Debug Info =====")]
    public bool showDebugLogs = true;

    // Private variables
    private int currentCameraIndex = 0;
    private CinemachineBrain cinemachineBrain;

    void Start()
    {
        // Get Cinemachine Brain
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

        if (cinemachineBrain == null)
        {
            Debug.LogError("[CameraSequenceManager] Main Camera does not have Cinemachine Brain component!");
            return;
        }

        // Validate setup
        if (virtualCameras == null || virtualCameras.Length == 0)
        {
            Debug.LogError("[CameraSequenceManager] Please add virtual cameras in Inspector!");
            return;
        }

        // Stop all timelines
        StopAllTimelines();

        // Activate initial camera
        currentCameraIndex = Mathf.Clamp(initialCameraIndex, 0, virtualCameras.Length - 1);
        ActivateCamera(currentCameraIndex);

        if (showDebugLogs)
        {
            Debug.Log("[CameraSequenceManager] Camera system initialized\n" +
                     "Total cameras: " + virtualCameras.Length + "\n" +
                     "Initial camera: " + currentCameraIndex);
        }
    }

    void Update()
    {
        // Key switching
        if (Input.GetKeyDown(nextCameraKey))
        {
            SwitchToNextCamera();
        }

        if (Input.GetKeyDown(previousCameraKey))
        {
            SwitchToPreviousCamera();
        }

        // Number key quick switch
        if (enableNumberKeys)
        {
            for (int i = 0; i < Mathf.Min(9, virtualCameras.Length); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SwitchToCamera(i);
                }
            }
        }
    }

    /// <summary>
    /// Switch to specified camera
    /// </summary>
    public void SwitchToCamera(int index)
    {
        if (index < 0 || index >= virtualCameras.Length)
        {
            Debug.LogWarning("[CameraSequenceManager] Invalid camera index: " + index);
            return;
        }

        if (index == currentCameraIndex)
        {
            if (showDebugLogs)
            {
                Debug.Log("[CameraSequenceManager] Already on camera: " + index);
            }
            return;
        }

        // Stop current timeline
        StopCurrentTimeline();

        // Switch camera
        currentCameraIndex = index;
        ActivateCamera(currentCameraIndex);

        // Play new camera timeline
        if (autoPlayTimeline)
        {
            PlayCurrentTimeline();
        }

        if (showDebugLogs)
        {
            Debug.Log("[CameraSequenceManager] Switched to camera " + currentCameraIndex + ": " + virtualCameras[currentCameraIndex].name);
        }
    }

    /// <summary>
    /// Switch to next camera
    /// </summary>
    public void SwitchToNextCamera()
    {
        int nextIndex = (currentCameraIndex + 1) % virtualCameras.Length;
        SwitchToCamera(nextIndex);
    }

    /// <summary>
    /// Switch to previous camera
    /// </summary>
    public void SwitchToPreviousCamera()
    {
        int prevIndex = (currentCameraIndex - 1 + virtualCameras.Length) % virtualCameras.Length;
        SwitchToCamera(prevIndex);
    }

    /// <summary>
    /// Activate specified camera
    /// </summary>
    private void ActivateCamera(int index)
    {
        // Lower all camera priorities
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            if (virtualCameras[i] != null)
            {
                virtualCameras[i].Priority = 0;
            }
        }

        // Raise target camera priority
        if (virtualCameras[index] != null)
        {
            virtualCameras[index].Priority = 10;
        }
    }

    /// <summary>
    /// Play current timeline
    /// </summary>
    private void PlayCurrentTimeline()
    {
        if (timelines != null && currentCameraIndex < timelines.Length)
        {
            PlayableDirector timeline = timelines[currentCameraIndex];

            if (timeline != null)
            {
                timeline.time = 0;  // Start from beginning
                timeline.Play();

                if (showDebugLogs)
                {
                    Debug.Log("[CameraSequenceManager] Playing Timeline: " + timeline.name);
                }
            }
        }
    }

    /// <summary>
    /// Stop current timeline
    /// </summary>
    private void StopCurrentTimeline()
    {
        if (timelines != null && currentCameraIndex < timelines.Length)
        {
            PlayableDirector timeline = timelines[currentCameraIndex];

            if (timeline != null && timeline.state == PlayState.Playing)
            {
                timeline.Stop();

                if (showDebugLogs)
                {
                    Debug.Log("[CameraSequenceManager] Stopped Timeline: " + timeline.name);
                }
            }
        }
    }

    /// <summary>
    /// Stop all timelines
    /// </summary>
    private void StopAllTimelines()
    {
        if (timelines != null)
        {
            foreach (PlayableDirector timeline in timelines)
            {
                if (timeline != null)
                {
                    timeline.Stop();
                    timeline.time = 0;
                }
            }

            if (showDebugLogs)
            {
                Debug.Log("[CameraSequenceManager] Stopped all timelines");
            }
        }
    }

    /// <summary>
    /// External call: switch camera by name
    /// </summary>
    public void SwitchToCameraByName(string cameraName)
    {
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            if (virtualCameras[i] != null && virtualCameras[i].name == cameraName)
            {
                SwitchToCamera(i);
                return;
            }
        }

        Debug.LogWarning("[CameraSequenceManager] Cannot find camera: " + cameraName);
    }

    void OnGUI()
    {
        if (showDebugLogs && Application.isPlaying)
        {
            GUI.Label(new Rect(10, 10, 300, 20),
                "Current camera: " + currentCameraIndex + " - " + virtualCameras[currentCameraIndex].name);
            GUI.Label(new Rect(10, 30, 300, 20),
                "Press " + nextCameraKey + " next | " + previousCameraKey + " previous");

            if (enableNumberKeys)
            {
                GUI.Label(new Rect(10, 50, 300, 20),
                    "Press 1-9 for quick switch");
            }
        }
    }
}
