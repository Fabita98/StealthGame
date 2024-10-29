using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "FSM/Actions/Patrol")]
public class PatrolAction : FSMAction
{
    // private float timer = 0;
    // private bool stopAnimationChoose = false;


    public override void Execute(BaseStateMachine machine)
    {
        NavMeshAgent navMeshAgent = machine.GetComponent<NavMeshAgent>();
        MovingPoints movingPoints = machine.GetComponent<MovingPoints>();
        EnemySightSensor enemySightSensor = machine.GetComponent<EnemySightSensor>();
        EnemyAttackSensor enemyAttackSensor = machine.GetComponent<EnemyAttackSensor>();
        EnemyUtility enemyUtility = machine.GetComponent<EnemyUtility>();

        
        if (machine.isStartOfPatrol)
        {
            enemyUtility.SetEyeLights(false);
            navMeshAgent.SetDestination(movingPoints.GetNextCircular(navMeshAgent).position);
            machine.Move();
            // timer = 0;
            machine.patrolWaitTimer = 0;
            machine.stopAnimationChoose = false;
            machine.isStartOfPatrol = false;
            machine.isStartOfChase = true;
            machine.isStartOfAttack = true;
            machine.isStartOfSleep = true;
            enemySightSensor.ChangeEscapedState(false);
            enemyAttackSensor.StartAttack = false;
            enemyAttackSensor.IsAttackCompleted = false;
            enemyUtility.transitionMusic.startChase();
        }

        if (movingPoints.Count() <= 1)
        {
            if (!machine.stopAnimationChoose)
            {
                machine.Stop(false);
                machine.stopAnimationChoose = true;   
            }
        }

        else
        {
            if (movingPoints.HasReached(navMeshAgent))
            {
                machine.patrolWaitTimer += Time.deltaTime;
                if (machine.patrolWaitTimer < machine.WaitTime)
                {
                    machine.Stop();
                    // if(!stopAnimationChoose)
                    // {
                    //     machine.Stop();
                    //     stopAnimationChoose = true;
                    // }   
                }
                else
                {
                    navMeshAgent.SetDestination(movingPoints.GetNextCircular(navMeshAgent).position);
                    machine.Move();
                    machine.patrolWaitTimer = 0;
                    machine.stopAnimationChoose = false;
                }
            }
        
            if (machine.patrolWaitTimer > machine.WaitTime)
            {
                navMeshAgent.SetDestination(movingPoints.GetNextCircular(navMeshAgent).position);
                machine.Move();
                machine.patrolWaitTimer = 0;
                machine.stopAnimationChoose = false;
            }            
   
        }

        
        // if (enemySightSensor.Ping())
        // {
        //     machine.isStartOfPatrol = true;
        // }
    }

}
