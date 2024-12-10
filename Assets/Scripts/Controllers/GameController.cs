using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This Class can be used as a singleton and has the control info and functionalities that will be needed in
/// the Campus scene 
/// </summary>
public class GameController : MonoBehaviour
{
    private GameManager _gameManager;
    public bool IsOnlyTutorial;
    public bool DebugMode;
    [NonSerialized] public CheckpointController CheckpointController;
    [NonSerialized] public LevelsController LevelsController;
    
    private static GameController _instance;
    public static GameController Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        Time.timeScale = 1;
        _gameManager = GameManager.Instance;
        CheckpointController = GetComponent<CheckpointController>();
        LevelsController = GetComponent<LevelsController>();
    }

    private void Start()
    {
        bool isGameStarted = PlayerPrefsManager.GetBool(PlayerPrefsKeys.GameStarted, false);
        if (!isGameStarted)
        {
            PlayerPrefsManager.SetBool(PlayerPrefsKeys.GameStarted, true);
        }

        List<BaseStateMachine> enemiesSM = GetAllEnemies();
        foreach (var SM in enemiesSM)
        {
            SM.enabled = true;
        }
        // _gameManager.AudioManager.play(SoundName.MainTheme);
    }

    public List<BaseStateMachine> GetAllEnemies()
    {
        return FindObjectsOfType<BaseStateMachine>().ToList();
    }
    
    public List<ShadowController> GetAllShadows()
    {
        return FindObjectsOfType<ShadowController>().ToList();
    }

}
