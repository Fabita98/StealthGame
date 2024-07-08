using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public class BaseStateMachine : MonoBehaviour
{
    public MyPlayerController PlayerController;
    [SerializeField] private int id;
    [SerializeField] private BaseState _initialState;
    [SerializeField] private float _speed;
    [SerializeField] private float _runSpeed;
    [NonSerialized] public BaseState CurrentState;
    [NonSerialized] public NavMeshAgent NavMeshAgent;
    [NonSerialized] public MovingPoints MovingPoints;
    [NonSerialized] public float WaitTime = 2;
    [NonSerialized] public float ChaseWaitTime = 2;
    [NonSerialized] public float AttackCoolDown = 2.25f;
    [NonSerialized] public float AlertTime = .3f;
    [NonSerialized] public bool isStartOfChase;
    [NonSerialized] public bool isChaseReset;
    [NonSerialized] public bool isStartOfPatrol;
    [NonSerialized] public bool isStartOfAttack;
    [NonSerialized] public bool stopAnimationChoose;
    private Dictionary<Type, Component> _cachedComponents;
    private int _updateCounter;
    private Transform initialTransform;

    private void Awake()
    {
        CurrentState = _initialState;
        _cachedComponents = new Dictionary<Type, Component>();
        _updateCounter = 0;
        NavMeshAgent = GetComponent<NavMeshAgent>();
        MovingPoints = GetComponent<MovingPoints>();
        AttackCoolDown /= 1.5f;
        isStartOfChase = true;
        isStartOfPatrol = true;
        isStartOfAttack = true;
        initialTransform = MovingPoints.GetFirst();
        transform.position = initialTransform.position;
        transform.rotation = initialTransform.rotation;
    }

    private void Start()
    {
        WaitTime = EnemyUtility.Instance.waitTime;
        ChaseWaitTime = EnemyUtility.Instance.chaseWaitTime;
        AlertTime = EnemyUtility.Instance.alertWaitTime;
    }

    private void LateUpdate()
    {
        CurrentState.Execute(this);
        // counter++;
        // if (NavMeshAgent.velocity == Vector3.zero)
        // {
        //     Stop();
        // }
        // _updateCounter++;
        // if (_updateCounter == 300)
        // {
        //     CurrentState.Execute(this);
        //     _updateCounter = 0;
        // }
    }
    
    public new T GetComponent<T>() where T : Component
    {
        if(_cachedComponents.ContainsKey(typeof(T)))
            return _cachedComponents[typeof(T)] as T;

        var component = base.GetComponent<T>();
        if(component != null)
        {
            _cachedComponents.Add(typeof(T), component);
        }
        return component;
    }

    public void Reset()
    {
        transform.position = initialTransform.position;
        transform.rotation = initialTransform.rotation;

        if (NavMeshAgent != null)
        {
            NavMeshAgent.ResetPath();
            NavMeshAgent.velocity = Vector3.zero;
            NavMeshAgent.isStopped = false;
            NavMeshAgent.speed = _speed;
        }

        GetComponent<EnemyAttackSensor>().StartAttack = false;
        GetComponent<EnemyAttackSensor>().IsAttackCompleted = false;
        CurrentState = _initialState;
        isStartOfChase = true;
        isStartOfPatrol = true;
        isStartOfAttack = true;
        stopAnimationChoose = false;
        _updateCounter = 0;

        EnemyUtility.Instance.ResetAnimator();
        

    }

    public void Stop(bool chooseIdleAnimation = true)
    {
        NavMeshAgent.isStopped = true;
        NavMeshAgent.speed = 0;
        if(chooseIdleAnimation)
            EnemyUtility.Instance.ChooseIdleAnimation();
        else
        {
            EnemyUtility.Instance.SetAnimation(lookAround: true);
        }
    }
    
    public void Move(bool isRunning = false)
    {
        NavMeshAgent.isStopped = false;
        NavMeshAgent.speed = isRunning ? _runSpeed : _speed;
        if (isRunning)
        {
            EnemyUtility.Instance.SetAnimation(sprint:true);   
        }
        else
        {
            EnemyUtility.Instance.SetAnimation(walk:true);
        }
    }
    
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("DoorArea"))
    //     {
    //     }
    // }
    
    public void Kill()
    {
    }
}
