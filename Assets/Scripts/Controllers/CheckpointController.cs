using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{

    void Start()
    {
        SetCheckpoint(transform);
        // ResetToCheckpoint();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToCheckpoint();
        }
    }

    public void SetCheckpoint(Transform checkpointTransform)
    {
        PlayerPrefsManager.SetTransform(PlayerPrefsKeys.PlayerLastCheckpoint, checkpointTransform);
    }

    public void ResetToCheckpoint()
    {   
        PlayerPrefsManager.GetAndSetTransform(PlayerPrefsKeys.PlayerLastCheckpoint, MyPlayerController.Instance.gameObject.transform);
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
