using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button gameOverButton;
    void Start()
    {
        gameObject.SetActive(false);
        gameOverButton.onClick.AddListener(() =>
        {
            GameController.Instance.CheckpointController.ResetToCheckpoint();
            gameObject.SetActive(false);
        });
    }

    private void OnEnable()
    {
        MyPlayerController.Instance.EnableLinearMovement = false;
    }
    private void OnDisable()
    {
        MyPlayerController.Instance.EnableLinearMovement = true;
    }
}
