using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowController : MonoBehaviour
{
    public float triggerRadius = 1;
    public float appearanceRadius = 3;
    public float dissolveTime = .5f;

    // private GameManager _gameManager;
    // private AudioManager _audioManager;
    // private AudioSource _audioSource;
    private PlayerFunctionalities _playerFunctionalities;
    private CapsuleCollider _triggerCapsuleCollider;
    
    void Start()
    {
        // _gameManager = GameManager.Instance;
        // _audioManager = _gameManager.AudioManager;
        // _audioSource = GetComponent<AudioSource>();
        _playerFunctionalities = PlayerFunctionalities.Instance;
        _triggerCapsuleCollider = GetComponent<CapsuleCollider>();
        SetTriggerRadius(triggerRadius);
    }

    private void Update()
    {
        SetTriggerRadius(triggerRadius);
        LookAtPlayer();
    }
    
    private void LookAtPlayer()
    {
        Transform playerTransform = MyPlayerController.Instance.transform;
        if (playerTransform != null)
        {
            Vector3 playerPosition = playerTransform.position;
            Vector3 directionToPlayer = playerPosition - transform.position;
            directionToPlayer.y = 0;
            transform.rotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
        }
    }

    private void SetTriggerRadius(float radius)
    {
        _triggerCapsuleCollider.radius = radius;
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerFunctionalities.CapturedByShadow();
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
        
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, appearanceRadius);

    }
}
