using Assets.Scripts.GazeTrackingFeature;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "FSM/Actions/Sleep")]
public class SleepAction : FSMAction
{
    
    public override void Execute(BaseStateMachine machine)
    {
        // var sleepSensor = machine.GetComponent<EnemySleepSensor>();
        EnemyUtility enemyUtility = machine.GetComponent<EnemyUtility>();
        if (machine.isStartOfSleep)
        {
            machine.Stop();
            enemyUtility.ChooseSleepAnimation();
            enemyUtility.DisableEyeLights();
            EyeTrackingDebug.SnoringAudioPlaybackTrigger();
            machine.isStartOfPatrol = true;
            machine.isStartOfChase = true;
            machine.isStartOfAttack = true;
            machine.isStartOfSleep = false;
            enemyUtility.sleepTimerUI.SetActive(true);
        }
    }
}