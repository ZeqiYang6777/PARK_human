using UnityEngine;

public class Interactable_Animation : MonoBehaviour
{
    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;
    public bool interactOnce = true;

    [Header("UI Settings")]
    public GameObject eKeyPrompt;

    [Header("Animation Settings")]
    //public Animator targetAnimator;
    public Animation[] targetAnimations;
    public string animationTriggerName = "Play";

    private bool playerInRange = false;
    private bool hasInteracted = false;

    void Start()
    {

        if (eKeyPrompt != null)
        {
            eKeyPrompt.SetActive(false);
        }


        //if (targetAnimator == null)
        //{
        //    targetAnimator = GetComponent<Animator>();
        //}
    }

    void Update()
    {

        if (playerInRange && !hasInteracted)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                Interact();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            //  "E" 
            if (eKeyPrompt != null && !hasInteracted)
            {
                eKeyPrompt.SetActive(true);
            }

            Debug.Log("Player can interact - Press " + interactionKey);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Òþ²Ø E ¼üÌáÊ¾
            if (eKeyPrompt != null)
            {
                eKeyPrompt.SetActive(false);
            }
        }
    }

    void Interact()
    {
        Debug.Log("Player interacted with " + gameObject.name);

        // Play Animation
        if (targetAnimations != null)
        {
            foreach (var targetAnimation in targetAnimations)
            {
                targetAnimation.Play();
            }

            // timeline about the machine running process
            Debug.Log("Animation triggered: " + animationTriggerName);
        }
        else
        {
            Debug.LogWarning("Animator is not assigned on " + gameObject.name);
        }


        if (eKeyPrompt != null)
        {
            eKeyPrompt.SetActive(false);
        }


        if (interactOnce)
        {
            hasInteracted = true;
        }
    }

    void OnDrawGizmos()
    {

        Gizmos.color = new Color(0, 0, 1, 0.3f);

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);
        }
    }
}
