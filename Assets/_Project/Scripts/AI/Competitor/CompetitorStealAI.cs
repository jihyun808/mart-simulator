using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CompetitorStealAI : MonoBehaviour
{
    [Header("Steal Settings")]
    [SerializeField] private float stealDistance = 1.5f;
    [SerializeField] private float stealDuration = 1.0f;

    private NavMeshAgent agent;
    private CompetitorAIController controller;

    private CartInventory targetCart;
    private bool isStealing = false;
    private float stealTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<CompetitorAIController>();

        if (controller == null)
        {
            Debug.LogError("[CompetitorStealAI] Controller 없음!");
        }
    }

    private void Update()
    {
        if (!isStealing || targetCart == null) return;

        float distance = Vector3.Distance(transform.position, targetCart.transform.position);

        if (distance > stealDistance)
        {
            agent.SetDestination(targetCart.transform.position);
            return;
        }

        // 카트 도착 → 훔치기 대기
        agent.isStopped = true;
        stealTimer += Time.deltaTime;

        if (stealTimer >= stealDuration)
        {
            StealItem();
        }
    }

    // ─────────────────────────────
    // 외부 진입 API (Controller에서 호출)
    // ─────────────────────────────

    public void StartSteal(CartInventory cart)
    {
        if (cart == null) return;

        targetCart = cart;
        isStealing = true;
        stealTimer = 0f;

        agent.isStopped = false;
        agent.SetDestination(cart.transform.position);
    }

    public void StopSteal()
    {
        isStealing = false;
        stealTimer = 0f;
        targetCart = null;

        agent.ResetPath();
        agent.isStopped = true;
    }

    // ─────────────────────────────
    // 내부 로직
    // ─────────────────────────────

    private void StealItem()
    {
        if (targetCart.StoredCount <= 0)
        {
            FinishSteal();
            return;
        }

        // ✅ 카트에서 아이템 하나 제거
        targetCart.TryTakeOutToHand(transform);

        Debug.Log($"🕵️ 경쟁자가 아이템을 훔침! ({targetCart.name})");

        FinishSteal();
    }

    private void FinishSteal()
    {
        StopSteal();
        controller.ChangeState(CompetitorAIController.State.Wander);
    }
}
