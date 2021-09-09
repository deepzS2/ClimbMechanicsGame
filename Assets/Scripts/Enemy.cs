using UnityEngine.AI;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region States
    private EnemyBaseState _currentState;

    public EnemyIdle idleState = new EnemyIdle();
    public EnemyChase chaseState = new EnemyChase();
    public EnemyAttack attackState = new EnemyAttack();
    #endregion

    #region AI
    public NavMeshAgent agent { get; private set; }

    public Transform player { get; private set; }

    [Header("AI")]

    [SerializeField]
    private LayerMask _groundLayer;

    [SerializeField]
    private LayerMask _playerLayer;

    [SerializeField]
    private float _sightRange, _attackRange, _attackTiming, _patrolRange;

    #region Public getters
    public LayerMask groundLayer { get => _groundLayer; }
    public LayerMask playerLayer { get => _playerLayer; }

    public float sightRange { get => _sightRange; }
    public float attackRange { get => _attackRange; }
    public float attackTiming { get => _attackTiming; }

    public float patrolRange { get => _patrolRange; }
    #endregion
    #endregion

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        _currentState = idleState;
    }

    void Start()
    {
        _currentState.EnterState(this);
    }

    void Update()
    {
        _currentState.ExecuteState(this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _sightRange);
    }

    #region Switch states
    public void SwitchState(EnemyBaseState nextState)
    {
        _currentState = nextState;

        _currentState.EnterState(this);
    }
    #endregion
}

#region State abstract
public abstract class EnemyBaseState
{
    public abstract void EnterState(Enemy stateManager);
    public abstract void ExecuteState(Enemy stateManager);
}
#endregion