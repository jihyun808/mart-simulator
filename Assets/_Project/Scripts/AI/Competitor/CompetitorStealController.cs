using UnityEngine;

public class CompetitorStealController : MonoBehaviour
{
    [Header("Steal Settings")]
    [SerializeField] private float cartAbandonTime = 10f;   // 카트 방치 시간
    [SerializeField] private float dropDelay = 6f;          // 훔친 후 드롭까지 시간
    [SerializeField] private float stealRange = 2.0f;       // 카트 접근 거리

    [Header("References")]
    [SerializeField] private CartInventory targetCart;

    private float abandonTimer = 0f;
    private bool isStealing = false;

    private PickupableItem stolenItem;
    private float stealTime;

    private void Update()
    {
        if (targetCart == null) return;

        HandleCartAbandonCheck();
        HandleDropStolenItem();
    }

    // ─────────────────────────────────────
    // 1️⃣ 카트 방치 감지
    // ─────────────────────────────────────
    private void HandleCartAbandonCheck()
    {
        if (isStealing) return;

        // 카트가 플레이어에게 사용 중인지 여부
        if (IsCartInUse())
        {
            abandonTimer = 0f;
            return;
        }

        abandonTimer += Time.deltaTime;

        if (abandonTimer >= cartAbandonTime)
        {
            TryStealFromCart();
            abandonTimer = 0f;
        }
    }

    // 카트가 사용 중인지 판단 (간단 버전)
    private bool IsCartInUse()
    {
        // 🔧 지금은 "플레이어 근처에 있으면 사용 중"으로 판단
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return false;

        float dist = Vector3.Distance(player.transform.position, targetCart.transform.position);
        return dist < 2.5f;
    }

    // ─────────────────────────────────────
    // 2️⃣ 훔치기 시도
    // ─────────────────────────────────────
    private void TryStealFromCart()
    {
        if (stolenItem != null) return;

        float dist = Vector3.Distance(transform.position, targetCart.transform.position);
        if (dist > stealRange) return;

        if (targetCart.TryStealOne(out PickupableItem item))
        {
            stolenItem = item;
            stealTime = Time.time;
            isStealing = true;

            // 아이템 임시 숨김
            stolenItem.gameObject.SetActive(false);

            Debug.Log($"[Competitor] 아이템 훔침: {stolenItem.name}");
        }
    }

    // ─────────────────────────────────────
    // 3️⃣ 훔친 아이템 드롭
    // ─────────────────────────────────────
    private void HandleDropStolenItem()
    {
        if (!isStealing || stolenItem == null) return;

        if (Time.time - stealTime < dropDelay) return;

        DropItem();
    }

    private void DropItem()
    {
        stolenItem.gameObject.SetActive(true);

        Vector3 dropPos = transform.position + Random.insideUnitSphere * 0.8f;
        dropPos.y = transform.position.y + 0.5f;

        stolenItem.transform.position = dropPos;
        stolenItem.transform.rotation = Quaternion.identity;

        Rigidbody rb = stolenItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(Vector3.up * 1.5f, ForceMode.Impulse);
        }

        Debug.Log($"[Competitor] 아이템 드롭: {stolenItem.name}");

        stolenItem = null;
        isStealing = false;
    }

    // 외부에서 카트 지정 (Inspector or 런타임)
    public void SetTargetCart(CartInventory cart)
    {
        targetCart = cart;
    }
}
