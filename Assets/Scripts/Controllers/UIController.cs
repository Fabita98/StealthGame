using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameOverUI GameOverUI;
    public AbilitiesUI AbilitiesUI;
    public TutorialFinishedUI TutorialFinishedUI;
    
    private static UIController _instance;
    public static UIController Instance => _instance;
    
    void Start()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

}
