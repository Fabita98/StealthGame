using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("HidingArea"))
        {
            MyPlayerController.Instance.isHiding = true;
        }
    }
    
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HidingArea"))
        {
            MyPlayerController.Instance.isHiding = false;
        }
    }

}
