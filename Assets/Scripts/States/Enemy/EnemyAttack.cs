using System.Collections;
using UnityEngine;

public class EnemyAttack : EnemyBaseState
{
    private bool _outsideAttackRange;

    private Coroutine _attackRoutine;

    public override void EnterState(Enemy stateManager)
    {
        _attackRoutine = stateManager.StartCoroutine(AttackRoutine(stateManager));

        return;
    }

    public override void ExecuteState(Enemy stateManager)
    {
        stateManager.agent.SetDestination(stateManager.transform.position);

        stateManager.transform.LookAt(stateManager.player);

        _outsideAttackRange = !Physics.CheckSphere(stateManager.transform.position, stateManager.attackRange, stateManager.playerLayer);

        if (_outsideAttackRange)
        {
            stateManager.SwitchState(stateManager.chaseState);

            stateManager.StopCoroutine(_attackRoutine);
        }
    }

    private IEnumerator AttackRoutine(Enemy stateManager)
    {
        while (true)
        {
            yield return new WaitForSeconds(stateManager.attackTiming);
            Debug.Log("ATTACK!");
        }
    }
}
