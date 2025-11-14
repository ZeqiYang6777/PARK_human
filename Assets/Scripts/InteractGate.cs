using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractGate : MonoBehaviour, IInteract
{
    public string showText;
    
    public string Description()
    {
        return showText;
    }

    public void OnInteract()
    {
        // interact feedback
        GetComponent<MeshRenderer>().materials[0].color = new Color(Random.value, Random.value, Random.value);
    }

}
