using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "FSM/Actions/Sleep")]
public class SleepAction : FSMAction
{
    
    public override void Execute(BaseStateMachine machine)
    {
        var sleepSensor = machine.GetComponent<EnemySleepSensor>();
        EnemyUtility enemyUtility = EnemyUtility.Instance;
        if (machine.isStartOfSleep)
        {
            machine.Stop();
            enemyUtility.ChooseSleepAnimation();
            machine.isStartOfPatrol = true;
            machine.isStartOfChase = true;
            machine.isStartOfAttack = true;
            machine.isStartOfSleep = false;
        }
    }
}