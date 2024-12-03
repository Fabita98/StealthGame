using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unlock_powerUP : MonoBehaviour
{
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
        if (other.tag == "LeftHand" || other.tag == "RightHand")
        {
            //add here variable to unlock gaze power up for fire
        }
    }
}
