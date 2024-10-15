using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Any))
        {
            Time.timeScale = 1;
            gameObject.SetActive(false);
            GameController.Instance.CheckpointController.ResetToCheckpoint();
        }
    }


    // [SerializeField] private Button gameOverButton;
    // void Start()
    // {
    //     //gameObject.SetActive(false);
    //     gameOverButton.onClick.AddListener(() =>
    //     {
    //         GameController.Instance.CheckpointController.ResetToCheckpoint();
    //         gameObject.SetActive(false);
    //     });
    // }

    // private void OnEnable()
    // {
    //     MyPlayerController.Instance.EnableLinearMovement = false;
    // }
    // private void OnDisable()
    // {
    //     MyPlayerController.Instance.EnableLinearMovement = true;
    // }
}
