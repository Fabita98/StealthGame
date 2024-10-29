using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/SleepToPatrol")]
public class SleepToPatrolDecision : Decision
{
    public override bool Decide(BaseStateMachine stateMachine)
    {
        var enemySleepSensor = stateMachine.GetComponent<EnemySleepSensor>();
        var enemyUtility = stateMachine.GetComponent<EnemyUtility>();
        if (!enemySleepSensor.IsSleeping())
            enemyUtility.sleepTimerUI.SetActive(false);
        return !enemySleepSensor.IsSleeping();
    }
}
