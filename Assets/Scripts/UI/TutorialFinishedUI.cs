using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialFinishedUI : MonoBehaviour
{
    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Any))
        {
            gameObject.SetActive(false);
            GameController.Instance.CheckpointController.ResetToCheckpoint();
        }
    }


}
