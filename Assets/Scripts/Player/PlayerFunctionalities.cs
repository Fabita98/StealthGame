using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFunctionalities : MonoBehaviour
{
    public static PlayerFunctionalities Instance => _instance;
    private static PlayerFunctionalities _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public void CapturedByShadow()
    {
        Debug.Log("Captured by Shadow");
    }

    public void CapturedByGuard()
    {
        Debug.Log("Captured by Guard");
    }
}
