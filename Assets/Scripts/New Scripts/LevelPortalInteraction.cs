using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LevelPortalInteraction : MonoBehaviour
{
    [Header("===== Scene Settings =====")]
    [Tooltip("Name of the next scene")]
    public string nextSceneName = "Zone2";

    [Header("===== Interaction Settings =====")]
    [Tooltip("Interaction key")]
    public KeyCode interactKey = KeyCode.E;

    [Header("===== UI Prompt =====")]
    [Tooltip("Drag in the prompt UI object")]
    public GameObject promptUI;

    [Tooltip("Prompt text component (optional, for dynamic text)")]
    public TextMeshProUGUI promptText;
    public GameObject loadingCircle;
    [Tooltip("Prompt message")]
    public string promptMessage = "Press E to interact.";

    [Header("===== Optional Settings =====")]
    [Tooltip("Delay before teleport")]
    public float teleportDelay = 0.3f;

    [Tooltip("Teleport sound effect")]
    public AudioClip teleportSound;

    [Header("===== Debug Info =====")]
    [Tooltip("Show debug logs")]
    public bool showDebugLogs = true;

    // Private variables
    private bool playerInRange = false;
    private bool isActivated = false;
    private AudioSource audioSource;

    void Start()
    {
        // Initialize audio
        if (teleportSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Hide prompt UI initially
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

        // Set prompt text
        if (promptText != null)
        {
            promptText.text = promptMessage;
        }

        if (showDebugLogs)
        {
            Debug.Log("[Portal] LevelPortalInteraction initialized\n" +
                     "Target scene: " + nextSceneName + "\n" +
                     "Interact key: " + interactKey);
        }
    }

    void Update()
    {
        // Check for interaction input
        if (playerInRange && !isActivated)
        {
            if (Input.GetKeyDown(interactKey))
            {
                if (showDebugLogs)
                {
                    Debug.Log("[Portal] Player pressed " + interactKey + " key, preparing to teleport!");
                }

                StartCoroutine(TeleportToNextLevel());
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            playerInRange = true;

            // Show prompt
            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }

            // Show loading circle
            if (loadingCircle != null)
            {
                loadingCircle.SetActive(true);
            }

            if (showDebugLogs)
            {
                Debug.Log("[Portal] Player entered portal range, showing prompt UI");
            }
        }
    }

    // Player exits trigger area
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Hide prompt
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }

            // Hide loading circle
            if (loadingCircle != null)
            {
                loadingCircle.SetActive(false);
            }

            if (showDebugLogs)
            {
                Debug.Log("[Portal] Player left portal range, hiding prompt UI");
            }
        }
    }

    // Teleport coroutine
    IEnumerator TeleportToNextLevel()
    {
        isActivated = true;

        // Hide prompt
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
        // Hide loading circle
        if (loadingCircle != null)
        {
            loadingCircle.SetActive(false);
        }

        // Play sound effect
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }

        // Wait before teleporting
        if (teleportDelay > 0)
        {
            if (showDebugLogs)
            {
                Debug.Log("[Portal] Waiting " + teleportDelay + " seconds before teleporting...");
            }
            yield return new WaitForSeconds(teleportDelay);
        }

        // Load scene
        if (showDebugLogs)
        {
            Debug.Log("[Portal] Loading scene: " + nextSceneName);
        }

        SceneManager.LoadScene(nextSceneName);
    }

    // Gizmo visualization
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);
        }
    }
}
