using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameFinishedUI : MonoBehaviour
{
    private void OnEnable()
    {
        Time.timeScale = 0;
    }
    
    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Any))
        {
            PlayerPrefs.DeleteAll();
#if UNITY_STANDALONE
            Application.Quit();
#endif
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }


}
