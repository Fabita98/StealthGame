using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class EyeController : MonoBehaviour
{
    private float timeToenable = 5f, timeSinceClosing;
    private bool restart = false;


    void Start()
    {
       
    }

    void Update()
    {
        if (DataTracker.eyeClosed)
        {
            if (!restart) 
            { 
                timeSinceClosing += Time.deltaTime; 
            }
            restart = false;
        }
        else
        {
            restart = true;
            timeSinceClosing = 0;
        }
    }
}
