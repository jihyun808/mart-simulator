using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CompetitorAIController : MonoBehaviour
{
    public enum State
    {
        Wander,
        Steal,
        Stunned
    }

    [Header("Current State (Debug)")]
    [SerializeField] private State currentState = State.Wander;

    private CompetitorWanderAI wanderAI;
    private CompetitorStealAI stealAI;
    private CompetitorAnimatorController anim;
    private CompetitorStunIndicator stunIndicator;
    private NavMeshAgent agent;

    [Header("Cart Search")]
    [SerializeField] private float cartSearchInterval = 2.0f;
    [SerializeField] private float stealSearchRadius = 15f;

    [Header("Stun Settings")]
    [SerializeField] private float stunDuration = 3f;
    private Coroutine stunCoroutine;

    private CartInventory targetCart;

    // 🔥 Blend 안정 변수
    private float currentMoveSpeed = 0f;

    /* ───────────────────────────── */

    private void Awake()
    {
        wanderAI = GetComponent<CompetitorWanderAI>();
        stealAI = GetComponent<CompetitorStealAI>();
        anim = GetComponent<CompetitorAnimatorController>();
        stunIndicator = GetComponent<CompetitorStunIndicator>();
        agent = GetComponent<NavMeshAgent>();

        if (!wanderAI) Debug.LogError("[CompetitorAI] WanderAI 없음");
        if (!stealAI) Debug.LogError("[CompetitorAI] StealAI 없음");
        if (!anim) Debug.LogError("[CompetitorAI] AnimatorController 없음");
        if (!agent) Debug.LogError("[CompetitorAI] NavMeshAgent 없음");
    }

    private void Start()
    {
        EnterState(State.Wander);
    }

    private void Update()
    {
        UpdateMovementAnimation();
    }

    /* ─────────────────────────────
     * FSM
     * ───────────────────────────── */

    public void ChangeState(State newState)
    {
        if (currentState == newState)
            return;

        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);
    }

    private void EnterState(State state)
    {
        switch (state)
        {
            case State.Wander:
                anim.SetStunned(false);
                wanderAI.StartWander();
                StartCartSearch();
                break;

            case State.Steal:
                anim.SetStunned(false);
                StopCartSearch();
                break;

            case State.Stunned:
                anim.SetStunned(true);
                wanderAI.StopWander();
                stealAI.StopSteal();
                StopCartSearch();
                agent.isStopped = true;
                break;
        }
    }

    private void ExitState(State state)
    {
        switch (state)
        {
            case State.Wander:
                wanderAI.StopWander();
                break;

            case State.Steal:
                stealAI.StopSteal();
                break;

            case State.Stunned:
                anim.SetStunned(false);
                agent.isStopped = false;
                break;
        }
    }

    /* ─────────────────────────────
     * 🔥 이동 애니메이션 (Blend Tree 안정화)
     * ───────────────────────────── */

    private void UpdateMovementAnimation()
    {
        if (currentState == State.Stunned)
        {
            currentMoveSpeed = 0f;
            anim.SetMoveSpeed(0f);
            return;
        }

        float rawSpeed = agent.velocity.magnitude;

        // 🔥 1. Dead Zone (미세 흔들림 제거)
        if (rawSpeed < 0.1f)
            rawSpeed = 0f;

        // 🔥 2. 0~1 정규화
        float targetSpeed = 0f;
        if (agent.speed > 0f)
            targetSpeed = Mathf.Clamp01(rawSpeed / agent.speed);

        // 🔥 3. 부드럽게 보간 (튐 방지)
        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetSpeed, Time.deltaTime * 8f);

        anim.SetMoveSpeed(currentMoveSpeed);
    }

    /* ─────────────────────────────
     * Cart 탐색
     * ───────────────────────────── */

    private void StartCartSearch()
    {
        CancelInvoke(nameof(SearchForAbandonedCart));
        InvokeRepeating(nameof(SearchForAbandonedCart), 1f, cartSearchInterval);
    }

    private void StopCartSearch()
    {
        CancelInvoke(nameof(SearchForAbandonedCart));
    }

    private void SearchForAbandonedCart()
    {
        if (currentState != State.Wander)
            return;

        CartInventory[] carts = FindObjectsOfType<CartInventory>();

        CartInventory closest = null;
        float closestDist = float.MaxValue;

        foreach (var cart in carts)
        {
            if (!cart.IsAbandoned) continue;
            if (cart.StoredCount <= 0) continue;

            float dist = Vector3.Distance(transform.position, cart.transform.position);
            if (dist > stealSearchRadius) continue;

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = cart;
            }
        }

        if (closest != null)
        {
            RequestSteal(closest);
        }
    }

    /* ─────────────────────────────
     * 외부 API
     * ───────────────────────────── */

    public void RequestSteal(CartInventory cart)
    {
        if (!cart) return;
        if (currentState != State.Wander) return;

        targetCart = cart;
        ChangeState(State.Steal);
        stealAI.StartSteal(targetCart);
    }

    public void OnStealFinished()
    {
        targetCart = null;
        ChangeState(State.Wander);
    }

    public void Stun()
    {
        if (currentState == State.Stunned)
            return;

        ChangeState(State.Stunned);
        stunIndicator?.Show();

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        yield return new WaitForSeconds(stunDuration);

        stunIndicator?.Hide();
        stunCoroutine = null;

        ChangeState(State.Wander);
    }

    public State GetCurrentState() => currentState;
}
