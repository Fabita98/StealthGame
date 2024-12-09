using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Level2 : Level
{
    private float _timer;
    private LevelConfig _levelConfig;
    private int _processNumber;
    private bool _isReachedToDestination;
    private bool _isSaved = false;
    private GameController _gameController;

    private void Awake()
    {
        _timer = 0;
        _processNumber = PlayerPrefsManager.GetInt(PlayerPrefsKeys.Level2Process, 1);
        _gameController = GameController.Instance;
        _levelConfig = _gameController.LevelsController.levelsConfigContainer.level2Config;
    }

    public override int LevelNum => 2;

    public override GameObject Self => GameController.Instance.LevelsController.levelsConfigContainer.level2Config.levelGameObject;
    
    public override bool IsDone { get; protected set; }

    public override void Setup()
    {
        SetPlayerPosition();
        PlayerPrefsManager.SaveGame(_levelConfig.playerSpawnPoint, 2);
        if (_processNumber == 1)
        {
            // Statue talk
            print("Statue Talking");
        }
    }

    public override void Process()
    {
        if(_processNumber < 3)
            _timer += Time.deltaTime;
        switch (_processNumber)
        {
            case 1:
                firstProcess();
                break;
            case 2:
                secondProcess();
                break;
            
            default:
                break;
        }
        
    }

    private void firstProcess()
    {
        if (!_isSaved)
        {
            PlayerPrefsManager.SaveGame(_levelConfig.playerSpawnPoint, 2);
            _isSaved = true;
        }
        _timer += Time.deltaTime;
        if (PlayerPrefsManager.GetBool(PlayerPrefsKeys.GotFirstPinkLotus, false) || _gameController.DebugMode)
        {
            SaveCompletedProcess(2);
        }
    }
    
    private void secondProcess()
    {
        if (_isReachedToDestination)
        {
            SaveCompletedProcess(3);
            EndOfLevel();
        }
    }

    private void SaveCompletedProcess(int processNumber)
    {
        _processNumber = processNumber;
        PlayerPrefsManager.SetInt(PlayerPrefsKeys.Level2Process, processNumber);
    }

    public override void EndOfLevel()
    {
        PlayerPrefsManager.SetInt(PlayerPrefsKeys.Level, 3);
        IsDone = true;
        PlayerPrefsManager.DeleteKey(PlayerPrefsKeys.Level2Process);
    }

    public override void SetPlayerPosition()
    {
        MyPlayerController.Instance.transform.position = _levelConfig.playerSpawnPoint.position;
        MyPlayerController.Instance.transform.rotation = _levelConfig.playerSpawnPoint.rotation;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has passed through the Level2 Destination");
            _isReachedToDestination = true;
        }
    }
}
