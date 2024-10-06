using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySightSensor : MonoBehaviour
{
    private bool _isFinallyEscaped;
    private Transform _lastSeenPlayerTransform;
    private float _lastSeenPlayerTimer;
    private MyPlayerController playerController;

    public void Awake()
    {
        _isFinallyEscaped = false;
        _lastSeenPlayerTimer = 0;
        GameObject lastSeenPlayerTransformObject = new GameObject("LastSeenPlayerTransform");
        _lastSeenPlayerTransform = lastSeenPlayerTransformObject.transform;
    }

    public void Start()
    {
        playerController = MyPlayerController.Instance;
    }

    public bool Ping()
    {
        EnemyUtility enemyUtility = GetComponent<EnemyUtility>();
        Collider[] playerInRange = Physics.OverlapSphere(transform.position, enemyUtility.viewRadius, enemyUtility.playerMask);
        _lastSeenPlayerTimer += Time.deltaTime;
        
        for (int i = 0; i < playerInRange.Length; i++)
        {
            Transform playerTransform = playerInRange[i].transform;
            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
            float dstToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (playerController.isHiding)
            {
                return false;
            }
            if (dstToPlayer < enemyUtility.overallRadius)
            {
                _lastSeenPlayerTimer = 0;
                _lastSeenPlayerTransform.position = playerTransform.position;
                // _lastSeenPlayerTransform = new Transform(playerTransform);
                return true;
            }
            else if (Vector3.Angle(transform.forward, dirToPlayer) < enemyUtility.viewAngle / 2)
            {
                if (!Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, enemyUtility.obstacleMask))
                {
                    _lastSeenPlayerTimer = 0;
                    _lastSeenPlayerTransform.position = playerTransform.position;
                    // _lastSeenPlayerTransform = playerTransform;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (Vector3.Distance(transform.position, playerTransform.position) > enemyUtility.viewRadius)
            {
                return false;
            }
        }
        return false;
    }
    
    private void OnDrawGizmos()
    {
        EnemyUtility enemyUtility = GetComponent<EnemyUtility>();;
        if (enemyUtility == null)
        {
            enemyUtility = GetComponent<EnemyUtility>();
        }

        // Draw view radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyUtility.viewRadius);

        // Draw view angle
        Gizmos.color = Color.yellow;
        Vector3 fovLine1 = Quaternion.AngleAxis(enemyUtility.viewAngle / 2, transform.up) * transform.forward * enemyUtility.viewRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-enemyUtility.viewAngle / 2, transform.up) * transform.forward * enemyUtility.viewRadius;
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);

        // Draw overall radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, enemyUtility.overallRadius);
    }
    
    public bool EscapedPlayerPos(Transform playerTransform, Transform enemyTransform, float escapeDistance)
    {
        if (Vector3.Distance(playerTransform.position, enemyTransform.position) > escapeDistance)
        {
            return true;
        }
        return false;
    }
    
    
    public bool Escaped(BaseStateMachine machine, Transform enemyTransform, float escapeDistance)
    {
        if (Vector3.Distance(_lastSeenPlayerTransform.position, enemyTransform.position) > escapeDistance)
        {
            machine.isChaseReset = true;
            return true;
        }
        else if (_lastSeenPlayerTimer > GetComponent<EnemyUtility>().maxTimeToLosePlayer)
        {
            machine.isChaseReset = true;
            return true;
        }
        else if (_lastSeenPlayerTimer > 0.1f && machine.NavMeshAgent.velocity == Vector3.zero)
        {
            machine.isChaseReset = true;
            return true;
        }
        return false;
    }

    public Transform GetLastSeenPlayerTransform()
    {
        return _lastSeenPlayerTransform;
    }

    public void ChangeEscapedState(bool state)
    {
        _isFinallyEscaped = state;
    }

    public bool IsFinallyEscaped()
    {
        return _isFinallyEscaped;
    }
    
}
