using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CartInventory : MonoBehaviour
{
    // ===== 기존 필드/메서드 그대로 유지 =====
    public int capacityMax = 30;
    public int CapacityUsed { get; private set; }
    public UnityEvent<int, int> onCapacityChanged;
    private readonly List<CarryableItem> _items = new();

    public bool TryAdd(CarryableItem item)
    {
        int cost = item.capacityCost;           // ✅ 기존 그대로 사용
        if (CapacityUsed + cost > capacityMax) return false;
        _items.Add(item);
        CapacityUsed += cost;
        onCapacityChanged?.Invoke(CapacityUsed, capacityMax);
        return true;
    }

    public bool Remove(CarryableItem item)
    {
        if (!_items.Remove(item)) return false;
        CapacityUsed -= item.capacityCost;      // ✅ 기존 그대로 사용
        onCapacityChanged?.Invoke(CapacityUsed, capacityMax);
        return true;
    }

    public IReadOnlyList<CarryableItem> Items => _items;

    // ===== ▼▼▼ 여기부터 '추가'만 해주세요 ▼▼▼ =====
    [Header("Load Rules (추가)")]
    public LayerMask itemMask;                  // Item 레이어만 허용
    public float justDroppedWindow = 0.6f;      // 드롭 후 이 시간 안에만 흡수
    public float acceptSpeed = 1.2f;            // 너무 빠르게 스치면 무시(m/s)

    [Header("Anchor (추가)")]
    public Transform anchorParent;              // 담긴 아이템의 부모(없으면 자기 자신)
    public bool snapToAnchor = true;            // 부모 원점 정렬 여부

    private void Reset()
    {
        // 이 컴포넌트가 붙은 Collider는 트리거여야 이벤트가 옴
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
{
    TryAbsorb(other);
    Debug.Log($"[CartInventory] Trigger enter by {other.name} (layer={LayerMask.LayerToName(other.gameObject.layer)})");

    if (((1 << other.gameObject.layer) & itemMask.value) == 0) return;
    Debug.Log("→ ItemMask 통과");

    var pick = other.GetComponent<PickupableItem>();
    if (pick) Debug.Log("→ PickupableItem 확인됨");

    // 나머지 로직 그대로
}
    private void OnTriggerStay(Collider other)  { TryAbsorb(other); }

private void TryAbsorb(Collider other)
{
    // 1) 레이어 필터
    if (((1 << other.gameObject.layer) & itemMask.value) == 0) return;

    // 2) 컴포넌트 안전하게 찾기 (자식 콜라이더 대응)
    var pick = other.GetComponentInParent<PickupableItem>();
    var item = other.GetComponentInParent<CarryableItem>();
    if (!pick || !item) return;

    // 3) 들고 있는 중엔 절대 담지 않기 + 이미 담긴 건 무시
    if (pick.IsCarried()) return;
    if (item.IsLoaded)   return;

    // 4) ‘방금 드롭’만 흡수 (타이밍)
    if (!pick.WasJustDropped(justDroppedWindow)) return;

    // 5) 너무 빠르면 무시 (스침 방지)
    Rigidbody rb = other.attachedRigidbody;
    if (!rb) rb = item.GetComponent<Rigidbody>();
    if (rb && rb.linearVelocity.sqrMagnitude > acceptSpeed * acceptSpeed) return;

    // 6) 용량 체크 후 담기
    if (!TryAdd(item)) return;

    // 7) 부모/물리 처리는 CarryableItem.MarkLoaded에서 처리
    var parent = anchorParent ? anchorParent : transform;
    item.MarkLoaded(parent); // (내부에서 isKinematic/충돌OFF/SetParent 등 처리되도록 구현)

    // 8) 파묻힘 방지: 약간 위로, 살짝 랜덤
    if (snapToAnchor)
    {
        const float yOffset = 0.25f;
        const float jitter  = 0.06f;
        Vector3 rand = new Vector3(Random.Range(-jitter, jitter), 0f,
                                   Random.Range(-jitter, jitter));

        item.transform.position = parent.position + Vector3.up * yOffset + rand;
        item.transform.rotation = parent.rotation;
    }

    // 9) 안전 정지
    if (rb) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

    Debug.Log("[CartInventory] ✅ 담기 완료 & 위로 스냅");
}

}