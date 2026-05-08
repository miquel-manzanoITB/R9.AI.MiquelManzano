using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyStateMachine))]
[RequireComponent(typeof(Animator))]
public class EnemyAnimatorController : MonoBehaviour
{
    private Animator _anim;
    private NavMeshAgent _agent;

    void Start()
    {
        _anim  = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Pasa la velocidad actual del NavMeshAgent al Animator
        _anim.SetFloat("Speed", _agent.velocity.magnitude);
    }

    // Llama a estos métodos desde EnemyAI cuando cambies de estado
    public void SetAttacking(bool value) => _anim.SetBool("IsAttacking", value);
    public void SetDead()               => _anim.SetBool("IsDead", true);
}