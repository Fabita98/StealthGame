using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Level1 : Level
{
    private float _timer;
    private LevelConfig _levelConfig;
    private int _processNumber;
    private bool _isReachedToDestination;

    private void Awake()
    {
        _timer = 0;
        _processNumber = PlayerPrefsManager.GetInt(PlayerPrefsKeys.Level1Process, 1);
        _levelConfig = GameController.Instance.LevelsController.levelsConfigContainer.level1Config;
    }

    public override int LevelNum => 1;

    public override GameObject Self => GameController.Instance.LevelsController.levelsConfigContainer.level1Config.levelGameObject;
    
    public override bool IsDone { get; protected set; }

    public override void Setup()
    {
        SetPlayerPosition();
        PlayerPrefsManager.SaveGame(_levelConfig.playerSpawnPoint, 1);
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
        _timer += Time.deltaTime;
        _levelConfig.pathBlockingGameObject.SetActive(true);
        if (PlayerPrefsManager.GetInt(PlayerPrefsKeys.WhiteLotus, 0) > 0)
        {
            _levelConfig.pathBlockingGameObject.SetActive(false);
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
        PlayerPrefsManager.SetInt(PlayerPrefsKeys.Level1Process, processNumber);
    }

    public override void EndOfLevel()
    {
        PlayerPrefsManager.SetInt(PlayerPrefsKeys.Level, 2);
        IsDone = true;
        PlayerPrefsManager.DeleteKey(PlayerPrefsKeys.Level1Process);
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
            Debug.Log("Player has passed through the Level1 Destination");
            _isReachedToDestination = true;
        }
    }
}
