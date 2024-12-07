using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Level5 : Level
{
    private float _timer;
    private LevelConfig _levelConfig;
    private int _processNumber;
    private bool _isReachedToDestination;
    private bool _isSaved = false;

    private void Awake()
    {
        _timer = 0;
        _processNumber = PlayerPrefsManager.GetInt(PlayerPrefsKeys.Level5Process, 1);
        _levelConfig = GameController.Instance.LevelsController.levelsConfigContainer.level5Config;
    }

    public override int LevelNum => 5;

    public override GameObject Self => GameController.Instance.LevelsController.levelsConfigContainer.level5Config.levelGameObject;
    
    public override bool IsDone { get; protected set; }

    public override void Setup()
    {
        SetPlayerPosition();
        PlayerPrefsManager.SaveGame(_levelConfig.playerSpawnPoint, 5);
        if (_processNumber == 1)
        {
            print("Starting level 2(5)");
        }
        // if (PlayerPrefsManager.GetInt(PlayerPrefsKeys.BlueLotus, 0) > 0)
        // {
        //     _levelConfig.otherObjects[0].GetComponent<DoorBehavior>().SetDoorState(DoorBehavior.DoorState.Open);
        // }
    }

    public override void Process()
    {
        if(_processNumber < 2)
            _timer += Time.deltaTime;
        switch (_processNumber)
        {
            case 1:
                firstProcess();
                break;
            default:
                break;
        }
        
    }

    private void firstProcess()
    {
        if (!_isSaved)
        {
            PlayerPrefsManager.SaveGame(_levelConfig.playerSpawnPoint, 5);
            _isSaved = true;
        }
        if (_isReachedToDestination)
        {
            SaveCompletedProcess(1);
            EndOfLevel();
        }
    }

    private void SaveCompletedProcess(int processNumber)
    {
        _processNumber = processNumber;
        PlayerPrefsManager.SetInt(PlayerPrefsKeys.Level5Process, processNumber);
    }

    public override void EndOfLevel()
    {
        Debug.Log("Level 2(5) Finished");
        UIController.Instance.TutorialFinishedUI.gameObject.SetActive(true);
        // PlayerPrefsManager.SetInt(PlayerPrefsKeys.Level, 4);
        IsDone = true;
        PlayerPrefsManager.DeleteKey(PlayerPrefsKeys.Level5Process);
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
            Debug.Log("Player has passed through the Level5 Destination");
            _isReachedToDestination = true;
        }
    }
}
