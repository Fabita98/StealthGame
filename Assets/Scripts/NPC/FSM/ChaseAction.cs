using Assets.Scripts.GazeTrackingFeature;
using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "FSM/Actions/Chase")]
public class ChaseAction : FSMAction
{
    // [NonSerialized] private float timer = 0;
    // [NonSerialized] private float alertTimer = 0;
    // [NonSerialized] private float waitTime = 0;
    // [NonSerialized] private bool stopAnimationChoose = false;

    public override void Execute(BaseStateMachine machine)
    {
        NavMeshAgent navMeshAgent = machine.GetComponent<NavMeshAgent>();
        MovingPoints movingPoints = machine.GetComponent<MovingPoints>();
        EnemySightSensor enemySightSensor = machine.GetComponent<EnemySightSensor>();
        var enemyAttackSensor = machine.GetComponent<EnemyAttackSensor>();
        EyeInteractable eyeInteractable = machine.GetComponent<EyeInteractable>();
        EnemyUtility enemyUtility = machine.GetComponent<EnemyUtility>();
        Transform playerTransform = machine.PlayerController.transform;
        
        if (machine.isStartOfChase)
        {
            eyeInteractable.StartPlayerSpottedAudioCoroutine();
            enemyUtility.SetEyeLights(true);
            navMeshAgent.SetDestination(playerTransform.position);
            machine.stopAnimationChoose = false;
            machine.isStartOfChase = false;
            machine.isStartOfPatrol = true;
            machine.isStartOfAttack = true;
            machine.isStartOfSleep = true;
            machine.Stop();
            enemyUtility.SetAnimation(alert: true);
            machine.chaseAlertTimer = 0;
            enemyUtility.transitionMusic.startChase();
        }

        if (machine.isChaseReset)
        {
            machine.stopAnimationChoose = false;
        }
        machine.chaseAlertTimer += Time.deltaTime;
        if (machine.chaseAlertTimer > machine.AlertTime)
        {
            if (movingPoints.HasReached(navMeshAgent, playerTransform))
            {
                enemyAttackSensor.StartAttack = true;
            }
            else
            {
                // if (!enemySightSensor.Escaped(playerTransform, machine.transform, enemyUtility.viewRadius))
                if (!enemySightSensor.Escaped(machine, machine.transform, enemyUtility.viewRadius))
                {
                    machine.chaseWaitTimer = machine.ChaseWaitTime;
                    machine.Move(true);
                    Transform lastSeenPlayerTransform = enemySightSensor.GetLastSeenPlayerTransform();
                    navMeshAgent.SetDestination(lastSeenPlayerTransform.position);   
                }
                else 
                {
                    machine.chaseWaitTimer -= Time.deltaTime;
                    if (!machine.stopAnimationChoose)
                    {
                        machine.Stop(false);
                        machine.stopAnimationChoose = true;   
                    }
                }

                if (machine.chaseWaitTimer <= .1f)
                {
                    enemySightSensor.ChangeEscapedState(true);
                    machine.isStartOfChase = true;
                    machine.stopAnimationChoose = false;
                }
            }
        }
    }
}
