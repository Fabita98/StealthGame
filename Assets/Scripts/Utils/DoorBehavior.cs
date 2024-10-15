using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
    public enum DoorState
    {
        Close,
        Open
    }
    
    public string triggerTagName = "Player";
    public Animator animator;
    public AudioSource[] doorSounds;
    public DoorState initState;
    public DoorState currentState; 
    public bool isSingleBehavior = true;

    private void Awake()
    {
        SetDoorState(initState);
    }

    public void DoorInteraction()
    {
        if (isSingleBehavior)
        {
            if (initState == DoorState.Open && currentState == DoorState.Open)
            {
                animator.SetTrigger("Close");
                Invoke(nameof(PlaySound), 0.5f);
                currentState = DoorState.Close;
            }
            else if (initState == DoorState.Close && currentState == DoorState.Close)
            {
                animator.SetTrigger("Open");
                Invoke(nameof(PlaySound), 0.5f);
                currentState = DoorState.Open;
            }
        }
        else
        {
            if (currentState == DoorState.Open)
            {
                animator.SetTrigger("Close");
                Invoke(nameof(PlaySound), 0.5f);
                currentState = DoorState.Close;
            }
            else 
            {
                animator.SetTrigger("Open");
                Invoke(nameof(PlaySound), 0.5f);
                currentState = DoorState.Open;
            }   
        }

    }

    public void SetDoorState(DoorState state)
    {
        if (state == DoorState.Open)
        {
            currentState = DoorState.Open;
            initState = DoorState.Open;
            animator.Play("DoubleDoorOpened");
        }
        else
        {
            currentState = DoorState.Close;
            initState = DoorState.Close;
            animator.Play("DoubleDoorClosed");
        }
    }
    
    public DoorState GetDoorState()
    {
        return currentState;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag(triggerTagName))
            return;
        DoorInteraction();
    }
    
    void PlaySound()
    {
        foreach (var doorSound in doorSounds)
        {
            doorSound.Play();   
        }
    }
}
