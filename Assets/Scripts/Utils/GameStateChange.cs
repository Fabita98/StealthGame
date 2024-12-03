using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateChange : MonoBehaviour
{
    [SerializeField] private GameObject tutorialObjects;
    [SerializeField] private GameObject finalObjects;
    
    private void OnEnable()
    {
        EnableGameObjects(false);
    }

    private void EnableGameObjects(bool isTutorial)
    {
        tutorialObjects.SetActive(isTutorial);
        finalObjects.SetActive(!isTutorial);
    }
}
