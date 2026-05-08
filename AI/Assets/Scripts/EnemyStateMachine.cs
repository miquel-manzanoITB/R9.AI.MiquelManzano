using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Patrol, Chase, Search, Attack, Death }

public class EnemyStateMachine : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Transform[] waypoints;       // Arrastra los waypoints aquí

    [Header("Detección")]
    public float visionRange = 12f;     // Radio del Sphere Collider trigger
    public float visionAngle = 90f;     // Ángulo del cono de visión (total, no la mitad)
    public LayerMask obstacleMask;      // Selecciona la capa de obstáculos

    [Header("Combate")]
    public float attackRange = 2f;      // Distancia para pasar a Attack
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    public int maxHP = 100;

    [Header("Búsqueda")]
    public float searchDuration = 8f;   // Segundos buscando antes de volver a Patrol

    private EnemyState _currentState;
    private NavMeshAgent _agent;
    private int _currentWaypoint = 0;
    private bool _playerInTrigger = false;
    private float _searchTimer = 0f;
    private float _attackTimer = 0f;
    private int _currentHP;
    private Vector3 _lastSeenPosition;
    private EnemyAnimatorController _animController;

    void Start()
    {
        _animController = GetComponent<EnemyAnimatorController>();
        _agent = GetComponent<NavMeshAgent>();
        _currentHP = maxHP;
        EnterState(EnemyState.Patrol);
    }

    void Update()
    {
        if (_currentState == EnemyState.Death) return;

        switch (_currentState)
        {
            case EnemyState.Patrol:  UpdatePatrol();  break;
            case EnemyState.Chase:   UpdateChase();   break;
            case EnemyState.Search:  UpdateSearch();  break;
            case EnemyState.Attack:  UpdateAttack();  break;
        }
    }

    // ─── ENTER STATE ────────────────────────────────────────────────────────

    void EnterState(EnemyState newState)
    {
        _currentState = newState;

        switch (newState)
        {
            case EnemyState.Patrol:
                _agent.isStopped = false;
                _agent.speed = 2f;
                GoToNextWaypoint();
                break;

            case EnemyState.Chase:
                _agent.isStopped = false;
                _agent.speed = 5f;
                break;

            case EnemyState.Search:
                _agent.isStopped = false;
                _agent.speed = 2.5f;
                _searchTimer = searchDuration;
                // Va al último punto donde vio al player
                _agent.SetDestination(_lastSeenPosition);
                break;

            case EnemyState.Attack:
                _agent.isStopped = true;
                _attackTimer = 0f;
                _animController?.SetAttacking(true);
                break;

            case EnemyState.Death:
                _agent.isStopped = true;
                _animController?.SetDead();
                Destroy(gameObject, 2f);
                break;
        }
    }

    // ─── UPDATE PER STATE ───────────────────────────────────────────────────

    void UpdatePatrol()
    {
        // Cuando llega al waypoint, va al siguiente
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            GoToNextWaypoint();

        // Comprueba si ve al player
        if (CanSeePlayer())
            EnterState(EnemyState.Chase);
    }

    void UpdateChase()
    {
        if (CanSeePlayer())
        {
            _lastSeenPosition = player.position;
            _agent.SetDestination(player.position);

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackRange)
                EnterState(EnemyState.Attack);
        }
        else
        {
            // Perdió la visión → Search
            EnterState(EnemyState.Search);
        }
    }

    void UpdateSearch()
    {
        _searchTimer -= Time.deltaTime;

        // Si vuelve a ver al player → Chase
        if (CanSeePlayer())
        {
            EnterState(EnemyState.Chase);
            return;
        }

        // Si llegó al último punto visto, da vueltas por la zona
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            // Busca un punto aleatorio cerca de donde perdió al player
            Vector3 randomPoint = _lastSeenPosition + Random.insideUnitSphere * 5f;
            randomPoint.y = transform.position.y;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);
        }

        // Tiempo agotado → vuelve a Patrol
        if (_searchTimer <= 0f)
            EnterState(EnemyState.Patrol);
    }

    void UpdateAttack()
    {
        if (Vector3.Distance(transform.position, player.position) > attackRange * 1.5f)
        {
            _animController?.SetAttacking(false);  // ← añade esto
            EnterState(EnemyState.Chase);
            return;
        }
        // Mira al player
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

        float dist = Vector3.Distance(transform.position, player.position);

        // Si el player se aleja → vuelve a Chase
        if (dist > attackRange * 1.5f)
        {
            EnterState(EnemyState.Chase);
            return;
        }

        // Ataca con cooldown
        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
        {
            _attackTimer = attackCooldown;
            player.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
            Debug.Log("¡Enemy ataca!");
        }
    }

    // ─── DETECCIÓN: CONO DE VISIÓN + RAYCAST ────────────────────────────────

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = player.position - transform.position;
        float dist = dirToPlayer.magnitude;

        // 1. ¿Está dentro del rango?
        if (dist > visionRange) return false;

        // 2. ¿Está dentro del ángulo del cono?
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > visionAngle / 2f) return false;

        // 3. ¿Hay obstáculos entre el enemy y el player? (Raycast)
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f,
                            dirToPlayer.normalized, dist, obstacleMask))
            return false;

        return true;
    }

    // ─── WAYPOINTS ──────────────────────────────────────────────────────────

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        _agent.SetDestination(waypoints[_currentWaypoint].position);
        _currentWaypoint = (_currentWaypoint + 1) % waypoints.Length;
    }

    // ─── DAÑO AL ENEMY ──────────────────────────────────────────────────────

    public void TakeDamage(int amount)
    {
        _currentHP -= amount;
        if (_currentHP <= 0)
            EnterState(EnemyState.Death);
    }

    // ─── GIZMOS (para ver el cono en el editor) ─────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Dibuja el cono de visión
        Vector3 leftDir  = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0,  visionAngle / 2f, 0) * transform.forward;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, leftDir  * visionRange);
        Gizmos.DrawRay(transform.position, rightDir * visionRange);
    }
}