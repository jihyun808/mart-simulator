using UnityEngine;
using UnityEngine.AI;

public class CompetitorAnimatorController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsStunned = Animator.StringToHash("IsStunned");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    // 🚶 이동 중
    public void SetWalking(bool value)
    {
        animator.SetBool(IsWalking, value);
    }

    // 🥴 기절 상태
    public void SetStunned(bool value)
    {
        animator.SetBool(IsStunned, value);

        if (agent == null) return;

        if (value)
        {
            // 🔥 핵심: 애니메이션이 위치를 못 건드리게 함
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
        else
        {
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.isStopped = false;
        }
    }
}
