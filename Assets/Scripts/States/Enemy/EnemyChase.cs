using UnityEngine;

public class EnemyChase : EnemyBaseState
{
    private bool _insideAttackRange;
    private bool _outsideSightRange;

    public override void EnterState(Enemy stateManager)
    {
        _insideAttackRange = false;
        _outsideSightRange = false;
    }

    public override void ExecuteState(Enemy stateManager)
    {
        Transform transform = stateManager.transform;
        stateManager.agent.SetDestination(stateManager.player.position);

        _insideAttackRange = Physics.CheckSphere(transform.position, stateManager.attackRange, stateManager.playerLayer);
        _outsideSightRange = !Physics.CheckSphere(transform.position, stateManager.sightRange, stateManager.playerLayer);

        if (_insideAttackRange)
        {
            stateManager.SwitchState(stateManager.attackState);
        } 
        else if (_outsideSightRange)
        {
            stateManager.SwitchState(stateManager.idleState);
        }
    }
}
