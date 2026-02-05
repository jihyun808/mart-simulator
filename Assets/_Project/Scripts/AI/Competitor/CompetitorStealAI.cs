using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CompetitorStealAI : MonoBehaviour
{
    [Header("Steal Settings")]
    [SerializeField] private float stealDistance = 1.5f;
    [SerializeField] private float stealDuration = 1.0f;

    private PickupableItem stolenItem;
    [SerializeField] private Transform dropPoint;

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
            Debug.LogError("[CompetitorStealAI] CompetitorAIController 없음!");
    }

    private void Update()
    {
        if (!isStealing || targetCart == null) return;

        // 🔒 훔치는 도중 카트가 다시 사용되면 중단
        if (!targetCart.IsAbandoned)
        {
            Debug.Log("[Steal] 카트가 다시 사용됨 → 훔치기 중단");
            FinishSteal();
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            targetCart.transform.position
        );

        // 아직 멀면 접근
        if (distance > stealDistance)
        {
            agent.isStopped = false;
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
    // 외부 API
    // ─────────────────────────────

    public void StartSteal(CartInventory cart)
    {
        if (cart == null) return;

        if (!cart.IsAbandoned)
        {
            Debug.Log("[Steal] 방치되지 않은 카트 → 훔치기 취소");
            return;
        }

        targetCart = cart;
        isStealing = true;
        stealTimer = 0f;

        agent.isStopped = false;
        agent.SetDestination(cart.transform.position);

        Debug.Log($"[Steal] 훔치기 시작: {cart.name}");
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
    if (targetCart == null)
    {
        FinishSteal();
        return;
    }

    if (!targetCart.TryStealOne(out PickupableItem item))
    {
        Debug.Log("[Steal] 훔칠 아이템 없음");
        FinishSteal();
        return;
    }

    stolenItem = item;

    // 훔친 즉시 월드에서 숨김 (들고 다니는 연출 X)
    stolenItem.gameObject.SetActive(false);

    Debug.Log($"🕵️ 경쟁자가 아이템 훔침: {stolenItem.name}");

    FinishSteal();
}


    public void DropStolenItem()
{
    if (stolenItem == null)
        return;

    stolenItem.gameObject.SetActive(true);

    Vector3 dropPos = dropPoint != null
        ? dropPoint.position
        : transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;

    stolenItem.transform.position = dropPos;

    Rigidbody rb = stolenItem.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
    }

    Debug.Log($"💥 경쟁자 기절 → 아이템 드랍: {stolenItem.name}");

    stolenItem = null;
}



    private void FinishSteal()
    {
        StopSteal();

        if (controller != null)
        {
            controller.ChangeState(CompetitorAIController.State.Wander);
        }
    }
}
