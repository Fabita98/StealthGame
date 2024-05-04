using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "FSM/Actions/Attack")]
public class AttackAction : FSMAction
{
    
    public override void Execute(BaseStateMachine machine)
    {
        var enemyAttackSensor = machine.GetComponent<EnemyAttackSensor>();
        
        if (machine.isStartOfAttack)
        {
            enemyAttackSensor.IsAttackCompleted = true;
            machine.isStartOfAttack = false;
            PlayerFunctionalities.Instance.CapturedByGuard();
        }
    }
}
