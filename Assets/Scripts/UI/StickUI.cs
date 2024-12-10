using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StickUI : MonoBehaviour
{
    [SerializeField] private GameObject stickImageGO;

    private void Start()
    {
        Set();
    }

    public void Set()
    {
        if (PlayerPrefsManager.GetBool(PlayerPrefsKeys.GotStickPower, false))
        {
            stickImageGO.SetActive(true);
        }
    }
}
