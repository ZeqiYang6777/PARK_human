using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiggerController : MonoBehaviour
{
    public ImageFader theImgae;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "Player")
        {
            StartCoroutine(theImgae.FadeIn());
        }
    }
}
