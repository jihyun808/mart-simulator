using UnityEngine;
using System.Collections;

public class CartIdleDetector : MonoBehaviour
{
    [Header("Idle Settings")]
    [SerializeField] private float idleTimeToSteal = 10f;

    private float idleTimer = 0f;
    private bool isPlayerNearby = false;
    private bool stealRequested = false;

    private CartInventory cart;

    private void Awake()
    {
        cart = GetComponent<CartInventory>();
    }

    private void Update()
    {
        if (isPlayerNearby)
        {
            idleTimer = 0f;
            stealRequested = false;
            return;
        }

        idleTimer += Time.deltaTime;

        if (!stealRequested && idleTimer >= idleTimeToSteal)
        {
            TryRequestSteal();
        }
    }

    private void TryRequestSteal()
    {
        stealRequested = true;

        CompetitorAIController competitor =
            FindClosestCompetitor();

        if (competitor != null)
        {
            Debug.Log("[Cart] 10초 방치 → 경쟁자에게 훔치기 요청");
            competitor.RequestSteal(cart);
        }
    }

    private CompetitorAIController FindClosestCompetitor()
    {
        CompetitorAIController[] all =
            FindObjectsOfType<CompetitorAIController>();

        float minDist = float.MaxValue;
        CompetitorAIController closest = null;

        foreach (var c in all)
        {
            if (c.GetCurrentState() != CompetitorAIController.State.Wander)
                continue;

            float dist = Vector3.Distance(
                transform.position,
                c.transform.position
            );

            if (dist < minDist)
            {
                minDist = dist;
                closest = c;
            }
        }

        return closest;
    }

    // ─────────────────────────────
    // 플레이어 감지 (카트 근처)
    // ─────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            idleTimer = 0f;
        }
    }
}
