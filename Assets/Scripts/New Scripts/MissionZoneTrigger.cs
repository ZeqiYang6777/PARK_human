using UnityEngine;

public class MissionZoneTrigger : MonoBehaviour
{
    public bool triggerOnce = true;
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered && triggerOnce) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log(" Ω¯»Î«¯”Ú");
            triggered = true;

            if (GameFlowController.Instance)
                GameFlowController.Instance.Step3_TriggerCinematic();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
