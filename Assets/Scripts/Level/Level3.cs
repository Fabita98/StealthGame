using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Level3 : Level
{
    private float _timer;
    private LevelConfig _levelConfig;
    private int _processNumber;
    private bool _isReachedToDestination;

    private void Awake()
    {
        _timer = 0;
        _processNumber = PlayerPrefsManager.GetInt(PlayerPrefsKeys.Level2Process, 1);
        _levelConfig = GameController.Instance.LevelsController.levelsConfigContainer.level3Config;
    }

    public override int LevelNum => 3;

    public override GameObject Self => GameController.Instance.LevelsController.levelsConfigContainer.level3Config.levelGameObject;
    
    public override bool IsDone { get; protected set; }

    public override void Setup()
    {
        SetPlayerPosition();
        PlayerPrefsManager.SaveGame(_levelConfig.playerSpawnPoint, 3);
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
        if (PlayerPrefsManager.GetInt(PlayerPrefsKeys.BlueLotus, 0) > 0)
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
        Debug.Log("Tutorial Finished");
        // PlayerPrefsManager.SetInt(PlayerPrefsKeys.Level, 4);
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
            Debug.Log("Player has passed through the Level3 Destination");
            _isReachedToDestination = true;
        }
    }
}
