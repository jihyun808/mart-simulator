using UnityEngine;
using UnityEngine.AI;

public class CompetitorAnimatorController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int IsStunned = Animator.StringToHash("IsStunned");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (!animator)
            Debug.LogError("[CompetitorAnimatorController] Animator 없음");
    }

    /* ─────────────────────────────
     * 🔥 Blend Tree 이동 제어
     * ───────────────────────────── */

    public void SetMoveSpeed(float value)
    {
        animator.SetFloat(MoveSpeed, value);
    }

    /* ─────────────────────────────
     * 🔥 Stun 제어
     * ───────────────────────────── */

    public void SetStunned(bool value)
    {
        animator.SetBool(IsStunned, value);

        if (agent == null) return;

        if (value)
        {
            agent.isStopped = true;
            agent.updatePosition = true;
            agent.updateRotation = true;
        }
        else
        {
            agent.isStopped = false;
        }
    }
}
