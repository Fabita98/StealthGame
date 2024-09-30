using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySleepSensor : MonoBehaviour
{
    public bool isSleep;

    public void Awake()
    {
    }

    public bool IsSleeping()
    {
        return isSleep;
    }

}
