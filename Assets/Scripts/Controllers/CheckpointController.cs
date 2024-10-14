using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{

    void Start()
    {
        // SetCheckpoint(MyPlayerController.Instance.transform);
    }

    public void SetCheckpoint(Transform checkpointTransform)
    {
        PlayerPrefsManager.SetTransform(PlayerPrefsKeys.PlayerLastCheckpoint, checkpointTransform);
    }

    public void ResetToCheckpoint()
    {
        CharacterController characterController = MyPlayerController.Instance.GetComponent<CharacterController>();
        characterController.enabled = false;
        PlayerPrefsManager.GetAndSetTransform(PlayerPrefsKeys.PlayerLastCheckpoint, MyPlayerController.Instance.transform);
        characterController.enabled = true;
        // playerTransform.position = Vector3.zero;
        List<BaseStateMachine> enemies = GameController.Instance.GetAllEnemies();
        foreach (var enemy in enemies)
        {
            enemy.Reset();
        }

        List<ShadowController> shadows = GameController.Instance.GetAllShadows();
        foreach (var shadow in shadows)
        {
            shadow.Reset();
        }
    }
}
