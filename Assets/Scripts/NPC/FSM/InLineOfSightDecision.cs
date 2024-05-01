using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/InLineOfSightDecision")]
public class InLineOfSightDecision : Decision
{
    public override bool Decide(BaseStateMachine stateMachine)
    {
        var enemyInLineOfSight = stateMachine.GetComponent<EnemySightSensor>();
        return enemyInLineOfSight.Ping();
    }
}
