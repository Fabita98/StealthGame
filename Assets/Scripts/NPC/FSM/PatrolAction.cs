using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "FSM/Actions/Patrol")]
public class PatrolAction : FSMAction
{
    [NonSerialized] private float timer = 0;
    [NonSerialized] private bool stopAnimationChoose = false;


    public override void Execute(BaseStateMachine machine)
    {
        NavMeshAgent navMeshAgent = machine.GetComponent<NavMeshAgent>();
        MovingPoints movingPoints = machine.GetComponent<MovingPoints>();
        EnemySightSensor enemySightSensor = machine.GetComponent<EnemySightSensor>();
        EnemyUtility enemyUtility = machine.GetComponent<EnemyUtility>();

        
        if (machine.isStartOfPatrol)
        {
            enemyUtility.SetEyeLights(false);
            navMeshAgent.SetDestination(movingPoints.GetNextCircular(navMeshAgent).position);
            machine.Move();
            timer = 0;
            stopAnimationChoose = false;
            machine.isStartOfPatrol = false;
            machine.isStartOfChase = true;
            machine.isStartOfAttack = true;
            machine.isStartOfSleep = true;
            machine.GetComponent<EnemySightSensor>().ChangeEscapedState(false);
            machine.GetComponent<EnemyAttackSensor>().StartAttack = false;
            machine.GetComponent<EnemyAttackSensor>().IsAttackCompleted = false;
        }

        if (movingPoints.Count() <= 1)
        {
            if (!stopAnimationChoose)
            {
                machine.Stop(false);
                stopAnimationChoose = true;   
            }
        }

        else
        {
            if (movingPoints.HasReached(navMeshAgent))
            {
                timer += Time.deltaTime;
                if (timer < machine.WaitTime)
                {
                    if(!stopAnimationChoose)
                    {
                        machine.Stop();
                        stopAnimationChoose = true;
                    }   
                }
                else
                {
                    navMeshAgent.SetDestination(movingPoints.GetNextCircular(navMeshAgent).position);
                    machine.Move();
                    timer = 0;
                    stopAnimationChoose = false;
                }
            }
        
            if (timer > machine.WaitTime)
            {
                navMeshAgent.SetDestination(movingPoints.GetNextCircular(navMeshAgent).position);
                machine.Move();
                timer = 0;
                stopAnimationChoose = false;
            }            
   
        }

        
        // if (enemySightSensor.Ping())
        // {
        //     machine.isStartOfPatrol = true;
        // }
    }

}
