using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowController : MonoBehaviour
{
    public float triggerRedius = 1;
    // private GameManager _gameManager;
    // private AudioManager _audioManager;
    // private AudioSource _audioSource;
    private PlayerFunctionalities _playerFunctionalities;
    private CapsuleCollider _capsuleCollider;
    
    void Start()
    {
        // _gameManager = GameManager.Instance;
        // _audioManager = _gameManager.AudioManager;
        // _audioSource = GetComponent<AudioSource>();
        _playerFunctionalities = PlayerFunctionalities.Instance;
        _capsuleCollider = GetComponent<CapsuleCollider>();
        SetTriggerRadius(triggerRedius);
    }

    private void Update()
    {
        SetTriggerRadius(triggerRedius);
    }

    private void SetTriggerRadius(float radius)
    {
        _capsuleCollider.radius = radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerFunctionalities.CapturedByShadow();
        }
    }
}
