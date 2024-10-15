using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            Time.timeScale = 1;
            gameObject.SetActive(false);
            GameController.Instance.CheckpointController.ResetToCheckpoint();
        }
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            Time.timeScale = 1;
            gameObject.SetActive(false);
        }
    }


}
