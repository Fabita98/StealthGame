using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialFinishedUI : MonoBehaviour
{
    
    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    
    private void Update()
    {
        // if (OVRInput.GetDown(OVRInput.Button.Any))
        // {
        //     PlayerPrefs.DeleteAll();
        //     Time.timeScale = 1;
        //     gameObject.SetActive(false);
        //     GameController.Instance.CheckpointController.ResetToCheckpoint();
        // }
    }


}
