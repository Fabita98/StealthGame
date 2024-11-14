using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerFunctionalities : MonoBehaviour
{
    public static PlayerFunctionalities Instance => _instance;
    private static PlayerFunctionalities _instance;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("Deaths"))
        {
            PlayerPrefsManager.SetInt(PlayerPrefsKeys.Deaths, 0);
        }
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public void CapturedByShadow()
    {
        UIController.Instance.GameOverUI.gameObject.SetActive(true);
        int currentNumOfDeaths = PlayerPrefsManager.GetInt(PlayerPrefsKeys.Deaths, 0);
        PlayerPrefsManager.SetInt(PlayerPrefsKeys.Deaths, ++currentNumOfDeaths);
        Debug.Log("Captured by Shadow");
    }

    public void CapturedByGuard()
    {
        UIController.Instance.GameOverUI.gameObject.SetActive(true);
        int currentNumOfDeaths = PlayerPrefsManager.GetInt(PlayerPrefsKeys.Deaths, 0);
        PlayerPrefsManager.SetInt(PlayerPrefsKeys.Deaths, ++currentNumOfDeaths);
        Debug.Log("Captured by Guard");
    }
}
