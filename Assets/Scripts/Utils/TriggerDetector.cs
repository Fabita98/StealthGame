using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    private MyPlayerController player;
    private void Awake()
    {
        try
        {
            player = GetComponent<MyPlayerController>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            player = MyPlayerController.Instance;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("HidingArea"))
        {
            MyPlayerController.Instance.isHiding = true;
        }
        if (other.CompareTag("StressfulArea"))
        {
            MyPlayerController.Instance.isInStressfulArea = 1;
        }
    }
    
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HidingArea"))
        {
            MyPlayerController.Instance.isHiding = false;
        }
        if (other.CompareTag("StressfulArea"))
        {
            MyPlayerController.Instance.isInStressfulArea = 0;
        }
    }

}
