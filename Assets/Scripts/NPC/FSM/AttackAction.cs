using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "FSM/Actions/Attack")]
public class AttackAction : FSMAction
{
    [NonSerialized] private float timer = 0;
    
    public override void Execute(BaseStateMachine machine)
    {
        var enemyAttackSensor = machine.GetComponent<EnemyAttackSensor>();
        EnemyUtility enemyUtility = EnemyUtility.Instance;
        
        if (machine.isStartOfAttack)
        {
            enemyAttackSensor.IsAttackCompleted = false;
            machine.isStartOfPatrol = true;
            machine.isStartOfChase = true;
            machine.isStartOfAttack = false;
        }
        if (timer <= 0)
        {
            enemyAttackSensor.IsAttackCompleted = true;
            enemyAttackSensor.StartAttack = false;
        }
        timer -= Time.deltaTime;
    }
}
