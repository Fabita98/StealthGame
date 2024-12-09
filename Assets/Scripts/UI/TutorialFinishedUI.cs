using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialFinishedUI : MonoBehaviour
{
    private GameController _gameController;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text message;
    
    
    private void OnEnable()
    {
        _gameController = GameController.Instance;
        Time.timeScale = 0;
    }

    
    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Any))
        {
            if (_gameController.IsOnlyTutorial)
            {
                title.text = "Congrats";
                message.text = "Tutorial finished";
                PlayerPrefs.DeleteAll();
#if UNITY_STANDALONE
                Application.Quit();
#endif
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            else
            {
                title.text = "Tutorial finished";
                message.text = "Press any button to continue";
                Time.timeScale = 1;
                gameObject.SetActive(false);   
            }
        }
    }


}
