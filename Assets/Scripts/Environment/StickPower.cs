using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickPower : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand")
        {
            PlayerPrefsManager.SetBool(PlayerPrefsKeys.GotStickPower, true);
            gameObject.SetActive(false);
        }
    }
}
