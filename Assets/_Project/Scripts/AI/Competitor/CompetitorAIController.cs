using System.Collections;
using UnityEngine;

public class CompetitorAIController : MonoBehaviour
{
    public enum State
    {
        Wander,
        Steal,
        Stunned
    }

    [Header("Current State (Debug)")]
    [SerializeField] private State currentState = (State)(-1);

    private CompetitorWanderAI wanderAI;
    private CompetitorStealAI stealAI;
    private CompetitorAnimatorController anim;

    private CompetitorStunIndicator stunIndicator;
    [Header("Cart Search")]
    [SerializeField] private float cartSearchInterval = 2.0f;
    [SerializeField] private float stealSearchRadius = 15f;

    [Header("Stun Settings")]
    [SerializeField] private float stunDuration = 3f;
    private Coroutine stunCoroutine;

    private CartInventory targetCart;

    /* ───────────────────────────── */

    private void Awake()
    {
        wanderAI = GetComponent<CompetitorWanderAI>();
        stealAI  = GetComponent<CompetitorStealAI>();
        anim     = GetComponent<CompetitorAnimatorController>();
        stunIndicator = GetComponent<CompetitorStunIndicator>();

        if (anim == null)
            Debug.LogError("[CompetitorAIController] AnimatorController 없음");
        if (wanderAI == null)
            Debug.LogError("[CompetitorAIController] CompetitorWanderAI 없음");
        if (stealAI == null)
            Debug.LogError("[CompetitorAIController] CompetitorStealAI 없음");
    }

    private void Start()
    {
        // ⚠️ 과거 버그 방지: ChangeState 사용 안 함
        currentState = State.Wander;
        EnterState(State.Wander);
    }

    /* ─────────────────────────────
     * FSM
     * ───────────────────────────── */

    public void ChangeState(State newState)
    {
        if (currentState == newState)
            return;

        Debug.Log($"[CompetitorAI] State: {currentState} -> {newState}");

        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);
    }

        private void EnterState(State state)
{
    Debug.Log($"[FSM] EnterState: {state}");

    switch (state)
    {
        case State.Wander:
            anim?.SetStunned(false);
            anim?.SetWalking(true);
            wanderAI?.StartWander();
            StartCartSearch();
            break;

        case State.Steal:
            anim?.SetStunned(false);
            anim?.SetWalking(true);
            StopCartSearch();
            break;

        case State.Stunned:
            anim?.SetWalking(false);
            anim?.SetStunned(true);

            wanderAI?.StopWander();
            stealAI?.StopSteal();
            StopCartSearch();
            break;
    }
}



    private void ExitState(State state)
    {
        switch (state)
        {
            case State.Wander:
                wanderAI?.StopWander();
                anim?.SetWalking(false);
                break;

            case State.Steal:
                stealAI?.StopSteal();
                break;

            case State.Stunned:
                anim?.SetStunned(false);
                break;
        }
    }

    /* ─────────────────────────────
     * 방치 카트 탐색 (Invoke 관리)
     * ───────────────────────────── */

    private void StartCartSearch()
    {
        CancelInvoke(nameof(SearchForAbandonedCart));
        InvokeRepeating(
            nameof(SearchForAbandonedCart),
            1f,
            cartSearchInterval
        );
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
            if (!cart.IsAbandoned)
                continue;

            if (cart.StoredCount <= 0)
                continue;

            float dist = Vector3.Distance(
                transform.position,
                cart.transform.position
            );

            if (dist > stealSearchRadius)
                continue;

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = cart;
            }
        }

        if (closest != null)
        {
            Debug.Log($"🛒 [Competitor] 방치 카트 발견: {closest.name}");
            RequestSteal(closest);
        }
    }

    /* ─────────────────────────────
     * 외부 API
     * ───────────────────────────── */

    public void RequestSteal(CartInventory cart)
    {
        if (cart == null) return;
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

    stunIndicator?.Show();   // 🔥 여기 추가

    if (stunCoroutine != null)
        StopCoroutine(stunCoroutine);

    stunCoroutine = StartCoroutine(StunRoutine());
}


    private IEnumerator StunRoutine()
{
    yield return new WaitForSeconds(stunDuration);

    stunIndicator?.Hide();   // 🔥 여기 추가

    stunCoroutine = null;
    ChangeState(State.Wander);
}


    public State GetCurrentState() => currentState;
}
