using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTriggerBehavior : MonoBehaviour
{
    public string triggerTag = "Player";
    public DoorBehavior door;

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag(triggerTag))
            return;
        door.DoorInteraction();
    }
}
