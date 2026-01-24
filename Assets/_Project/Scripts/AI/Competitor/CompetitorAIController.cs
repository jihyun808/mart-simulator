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
    [SerializeField] private State currentState;

    private CompetitorWanderAI wanderAI;
    private CompetitorStealAI stealAI;

    private void Awake()
    {
        wanderAI = GetComponent<CompetitorWanderAI>();
        stealAI = GetComponent<CompetitorStealAI>();

        if (wanderAI == null)
            Debug.LogError("[CompetitorAIController] CompetitorWanderAI 없음");

        if (stealAI == null)
            Debug.LogError("[CompetitorAIController] CompetitorStealAI 없음");
    }

    private void Start()
    {
        ChangeState(State.Wander);
    }

    // ─────────────────────────────
    // 상태 전이 (FSM 핵심)
    // ─────────────────────────────

    public void ChangeState(State newState)
    {
        if (currentState == newState)
        {
            Debug.Log($"[Controller] State Change ignored: {currentState} -> {newState}");
            return;
        }

        Debug.Log($"[Controller] State Change: {currentState} -> {newState}");

        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);
    }

    private void EnterState(State state)
    {
        switch (state)
        {
            case State.Wander:
                wanderAI?.StartWander();
                break;

            case State.Steal:
                // Steal은 StartSteal(cart)에서 시작
                break;

            case State.Stunned:
                wanderAI?.StopWander();
                stealAI?.StopSteal();
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

    // ─────────────────────────────
    // 외부에서 호출하는 명확한 API
    // ─────────────────────────────

    public void RequestSteal(CartInventory targetCart)
    {
        if (targetCart == null) return;
        if (currentState == State.Stunned) return;

        ChangeState(State.Steal);
        stealAI.StartSteal(targetCart);
    }

    public void OnStealFinished()
    {
        ChangeState(State.Wander);
    }

    public void Stun()
    {
        ChangeState(State.Stunned);
    }

    public State GetCurrentState()
    {
        return currentState;
    }
}
