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

        if (wanderAI == null)
            Debug.LogError("[CompetitorAIController] CompetitorWanderAI 없음");

        if (stealAI == null)
            Debug.LogError("[CompetitorAIController] CompetitorStealAI 없음");
    }

    private void Start()
    {
        // ⚠️ ChangeState 사용하지 않음 (과거 버그 방지)
        currentState = State.Wander;
        EnterState(State.Wander);

        StartCartSearch();
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
                wanderAI?.StartWander();
                StartCartSearch();
                break;

            case State.Steal:
                StopCartSearch();
                // 실제 이동은 RequestSteal에서 시작
                break;

            case State.Stunned:
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
                break;

            case State.Steal:
                stealAI?.StopSteal();
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

    // 🔥 훔친 아이템 드랍
    stealAI?.DropStolenItem();

    if (stunCoroutine != null)
        StopCoroutine(stunCoroutine);

    stunCoroutine = StartCoroutine(StunRoutine());
}

    private IEnumerator StunRoutine()
    {
        Debug.Log("[Competitor] 기절!");

        yield return new WaitForSeconds(stunDuration);

        Debug.Log("[Competitor] 기절 회복");
        stunCoroutine = null;

        ChangeState(State.Wander);
    }

    public State GetCurrentState() => currentState;
}
