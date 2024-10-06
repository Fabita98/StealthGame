using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlinePOI : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject POI;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "RightHand" || other.tag == "LeftHand")
        {
            POI.GetComponent<Outline>().enabled=true;
        } 
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "RightHand" || other.tag == "LeftHand")
        {
            POI.GetComponent<Outline>().enabled = false;
        }
    }
}
