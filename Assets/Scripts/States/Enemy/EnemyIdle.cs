using UnityEngine;

public class EnemyIdle : EnemyBaseState
{
    private bool _playerOnSight;
    private bool _walkPointSet;

    private Vector3 _walkPoint;

    public override void EnterState(Enemy stateManager)
    {
        _playerOnSight = false;
        _walkPointSet = false;
    }

    public override void ExecuteState(Enemy stateManager)
    {
        Transform transform = stateManager.transform;

        _playerOnSight = Physics.CheckSphere(transform.position, stateManager.sightRange, stateManager.playerLayer);

        if (_playerOnSight)
        {
            stateManager.SwitchState(stateManager.chaseState);
        }
        else if (!_walkPointSet)
        {
            float randomZ = Random.Range(-stateManager.patrolRange, stateManager.patrolRange);
            float randomX = Random.Range(-stateManager.patrolRange, stateManager.patrolRange);

            _walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(_walkPoint, -transform.up, 2f, stateManager.groundLayer) || Physics.Raycast(_walkPoint, transform.forward, 1f, stateManager.groundLayer))
            {
                _walkPointSet = true;
            }
        } 
        else
        {
            stateManager.agent.SetDestination(_walkPoint);
        }

        Vector3 distanceToPoint = transform.position - _walkPoint;

        if (distanceToPoint.magnitude < 1f) _walkPointSet = false;
    }

    
}
