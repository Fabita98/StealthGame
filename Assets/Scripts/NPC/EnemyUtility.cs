using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public enum EnemyAnimatorParameters
{
    Idle,
    LookAround,
    Walk,
    SpeedWalk,
    Attack,
    Alert,
    Sleep
}

public class EnemyUtility : MonoBehaviour
{
    [SerializeField] private GameObject eyeLights;
    public GameObject sleepTimer;
    public float viewRadius = 25;
    public float overallRadius = 5;
    public float viewAngle = 150;
    public float waitTime = 2;
    public float chaseWaitTime = 2;
    public float alertWaitTime = 1;
    public float maxTimeToLosePlayer = 0;
    public LayerMask playerMask;
    public LayerMask obstacleMask;
    [SerializeField] private Animator enemyAnimator;

    // public static EnemyUtility Instance;
    //
    // private void Awake()
    // {
    //     if (Instance == null)
    //     {
    //         Instance = this;
    //     }
    //
    // }

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
    
    public void ChooseSleepAnimation()
    {
        SetAnimation(sleep: true);
    }
    
    public  void SetAnimation(bool idle = false, bool lookAround = false, bool walk = false, bool sprint = false,
        bool attack = false, bool alert = false, bool sleep = false)
    {
        enemyAnimator.SetBool(EnemyAnimatorParameters.Idle.ToString(), idle);
        enemyAnimator.SetBool(EnemyAnimatorParameters.LookAround.ToString(), lookAround);
        enemyAnimator.SetBool(EnemyAnimatorParameters.Walk.ToString(), walk);
        enemyAnimator.SetBool(EnemyAnimatorParameters.SpeedWalk.ToString(), sprint);
        enemyAnimator.SetBool(EnemyAnimatorParameters.Attack.ToString(), attack);
        enemyAnimator.SetBool(EnemyAnimatorParameters.Alert.ToString(), alert);
        enemyAnimator.SetBool(EnemyAnimatorParameters.Sleep.ToString(), sleep);
    }

    public void ResetAnimator()
    {
        enemyAnimator.Play("Idle");
    }

    public void SetEyeLights(bool active)
    {
        eyeLights.SetActive(true);
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

    public void DisableEyeLights()
    {
        eyeLights.SetActive(false);
    }
    
}
