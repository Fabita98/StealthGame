using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasLookAtPlayer : MonoBehaviour
{
    private Transform _playerCamera; 

    void Start()
    {
        if (_playerCamera == null)
        {
            _playerCamera = MyPlayerController.Instance.transform;
        }
    }

    void Update()
    {
        if (_playerCamera != null)
        {
            transform.LookAt(_playerCamera);
        }
    }
}
