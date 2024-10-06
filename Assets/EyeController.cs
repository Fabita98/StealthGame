using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class EyeController : MonoBehaviour
{
    public DataTracker dt;
    private float timeToenable = 5f, timeSinceClosing;
    private bool restart = false;


    void Start()
    {
       
    }

    void Update()
    {
        if (dt.eyeClosed)
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
