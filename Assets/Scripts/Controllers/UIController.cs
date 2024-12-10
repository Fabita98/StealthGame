using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameOverUI GameOverUI;
    public AbilitiesUI AbilitiesUI;
    public TutorialFinishedUI TutorialFinishedUI;
    public GameFinishedUI GameFinishedUI;
    public PauseUI PauseUI;
    
    private static UIController _instance;
    public static UIController Instance => _instance;
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Start))
        {
            PauseUI.gameObject.SetActive(true);
        }
    }
}
