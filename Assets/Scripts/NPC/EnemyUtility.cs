using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public enum EnemyAnimatorParameters
{
    Idle,
    LookAround,
    Walk,
    SpeedWalk,
    Attack,
    Alert
}

public class EnemyUtility : MonoBehaviour
{
    [SerializeField] private GameObject eyeLights;
    public float viewRadius = 25;
    public float overallRadius = 5;
    public float viewAngle = 150;
    public float waitTime = 2;
    public float chaseWaitTime = 2;
    public float alertWaitTime = 1;
    public float maxTimeToLosePlayer = 0;
    public LayerMask playerMask;
    public LayerMask obstacleMask;
    private Animator EnemyAnimator;

    public static EnemyUtility Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        EnemyAnimator = GetComponent<Animator>();
    }

    public void ChooseIdleAnimation()
    {
        int rnd = Random.Range(0, 100);
        if (rnd < 50)
        {
            SetAnimation(idle: true);
        }
        else
        {
            SetAnimation(lookAround: true);
        }
    }
    
    public void ChooseAttackAnimation()
    {
        SetAnimation(attack: true);
    }
    
    public  void SetAnimation(bool idle = false, bool lookAround = false, bool walk = false, bool sprint = false,
        bool attack = false, bool alert = false)
    {
        EnemyAnimator.SetBool(EnemyAnimatorParameters.Idle.ToString(), idle);
        EnemyAnimator.SetBool(EnemyAnimatorParameters.LookAround.ToString(), lookAround);
        EnemyAnimator.SetBool(EnemyAnimatorParameters.Walk.ToString(), walk);
        EnemyAnimator.SetBool(EnemyAnimatorParameters.SpeedWalk.ToString(), sprint);
        EnemyAnimator.SetBool(EnemyAnimatorParameters.Attack.ToString(), attack);
        EnemyAnimator.SetBool(EnemyAnimatorParameters.Alert.ToString(), alert);
    }

    public void SetEyeLights(bool active)
    {
        if (active)
        {
            eyeLights.GetComponent<Light>().color = Color.red;            
        }
        else
        {
            eyeLights.GetComponent<Light>().color = Color.white;
        }
        // eyeLights.SetActive(active);
    }
    
}
