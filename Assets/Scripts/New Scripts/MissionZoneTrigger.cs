using UnityEngine;

public class MissionZoneTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public bool triggerOnce = true;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        // Prevent multiple triggers if set to trigger once
        if (triggered && triggerOnce)
        {
            return;
        }

        // Check if player entered the zone
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered mission zone");
            triggered = true;

            // Trigger the cinematic sequence
            if (GameFlowController.Instance != null)
            {
                GameFlowController.Instance.Step3_TriggerCinematic();
            }
            else
            {
                Debug.LogWarning("GameFlowController.Instance is null");
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw a semi-transparent green box in the editor
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
