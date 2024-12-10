using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickPower : MonoBehaviour
{
    private void Start()
    {
        if (PlayerPrefsManager.GetBool(PlayerPrefsKeys.GotStickPower, false))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand")
        {
            PlayerPrefsManager.SetBool(PlayerPrefsKeys.GotStickPower, true);
            UIController.Instance.StickUI.Set();
            gameObject.SetActive(false);
        }
    }
}
