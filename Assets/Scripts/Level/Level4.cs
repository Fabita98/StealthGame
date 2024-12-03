using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Level4 : Level
{
    private float _timer;
    private LevelConfig _levelConfig;
    private int _processNumber;
    private bool _isReachedToDestination;
    private bool _isSaved;

    private void Awake()
    {
        _timer = 0;
        _processNumber = PlayerPrefsManager.GetInt(PlayerPrefsKeys.Level4Process, 1);
        _levelConfig = GameController.Instance.LevelsController.levelsConfigContainer.level4Config;
    }

    public override int LevelNum => 3;

    public override GameObject Self => GameController.Instance.LevelsController.levelsConfigContainer.level4Config.levelGameObject;
    
    public override bool IsDone { get; protected set; }

    public override void Setup()
    {
        SetPlayerPosition();
        PlayerPrefsManager.SaveGame(_levelConfig.playerSpawnPoint, 4);
        if (_processNumber == 1)
        {
            // Statue talk
            print("Statue Talking");
        }
        else if (_processNumber >= 2)
        {
            _levelConfig.otherObjects[0].GetComponent<DoorBehavior>().DoorInteraction();
        }
        // if (PlayerPrefsManager.GetInt(PlayerPrefsKeys.BlueLotus, 0) > 0)
        // {
        //     _levelConfig.otherObjects[0].GetComponent<DoorBehavior>().SetDoorState(DoorBehavior.DoorState.Open);
        // }
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
            PlayerPrefsManager.SaveGame(_levelConfig.playerSpawnPoint, 3);
            _isSaved = true;
        }
        _timer += Time.deltaTime;
        //if (PlayerPrefsManager.GetBool(PlayerPrefsKeys.GotFirstBlueLotus, false))
        if (_levelConfig.otherObjects[1].GetComponent<TalkingStatue>().finished)
        {
            _levelConfig.otherObjects[0].GetComponent<DoorBehavior>().DoorInteraction();
            SaveCompletedProcess(2);
        }
    }
    
    private void secondProcess()
    {   

        // if (_levelConfig.otherObjects[0].GetComponent<DoorBehavior>().GetDoorState() == DoorBehavior.DoorState.Close)
        // {
        // }
        if (_isReachedToDestination)
        {
            SaveCompletedProcess(4);
            EndOfLevel();
        }
    }

    private void SaveCompletedProcess(int processNumber)
    {
        _processNumber = processNumber;
        PlayerPrefsManager.SetInt(PlayerPrefsKeys.Level4Process, processNumber);
    }

    public override void EndOfLevel()
    {
        Debug.Log("Tutorial Finished");
        UIController.Instance.TutorialFinishedUI.gameObject.SetActive(true);
        // PlayerPrefsManager.SetInt(PlayerPrefsKeys.Level, 4);
        IsDone = true;
        PlayerPrefsManager.DeleteKey(PlayerPrefsKeys.Level4Process);
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
            Debug.Log("Player has passed through the Level4 Destination");
            _isReachedToDestination = true;
        }
    }
}
