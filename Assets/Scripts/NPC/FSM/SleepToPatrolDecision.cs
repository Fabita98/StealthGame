using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/SleepToPatrol")]
public class SleepToPatrolDecision : Decision
{
    public override bool Decide(BaseStateMachine stateMachine)
    {
        var enemyInLineOfSight = stateMachine.GetComponent<EnemySleepSensor>();
        return !enemyInLineOfSight.IsSleeping();
    }
}
