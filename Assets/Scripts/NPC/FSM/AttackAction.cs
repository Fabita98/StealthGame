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
        EnemyUtility enemyUtility = machine.GetComponent<EnemyUtility>();
        
        if (machine.isStartOfAttack)
        {
            enemyAttackSensor.IsAttackCompleted = true;
            enemyUtility.ChooseAttackAnimation();
            machine.isStartOfAttack = false;
            PlayerFunctionalities.Instance.CapturedByGuard();
        }
    }
}
