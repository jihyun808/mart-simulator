using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CartInventory : MonoBehaviour
{
    [Header("Capacity (Optional)")]
    public int capacityMax = 30;
    public int CapacityUsed { get; private set; }
    public UnityEvent<int, int> onCapacityChanged;

    [Header("Absorb Rules")]
    public LayerMask itemMask; // 흡수 가능한 아이템 레이어

    [Header("Store Behavior")]
    public bool hideItemInScene = true; // true면 SetActive(false)로 씬에서 숨김

    // 담긴 아이템 저장 (LIFO: 마지막에 담은 것부터 꺼냄)
    private readonly Stack<PickupableItem> _stored = new();

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) => TryStore(other);
    private void OnTriggerStay(Collider other)  => TryStore(other);

    private void TryStore(Collider other)
    {
        // 1) 레이어 필터
        if (((1 << other.gameObject.layer) & itemMask.value) == 0) return;

        // 2) PickupableItem 찾기 (자식 콜라이더 대응)
        var pick = other.GetComponentInParent<PickupableItem>();
        if (!pick) return;

        // 3) 이미 담긴 건 무시 (중복 방지)
        // Stack은 Contains 가능(내부 순회). 아이템 수가 많지 않으면 OK.
        if (_stored.Contains(pick)) return;

        // 4) (Optional) capacity 체크를 쓰고 싶으면 CarryableItem 비용을 이용
        //    없으면 비용을 1로 취급
        int cost = 1;
        var carry = pick.GetComponent<CarryableItem>();
        if (carry) cost = carry.capacityCost;

        if (CapacityUsed + cost > capacityMax) return;

        // 5) 담기 처리
        _stored.Push(pick);
        CapacityUsed += cost;
        onCapacityChanged?.Invoke(CapacityUsed, capacityMax);

        // 씬에서 보일 필요 없으면 숨김
        if (hideItemInScene)
            pick.gameObject.SetActive(false);

        Debug.Log($"[CartInventory] Stored: {pick.name} (used {CapacityUsed}/{capacityMax})");
    }

    /// <summary>
    /// 카트에서 아이템 1개 꺼내서 플레이어 손에 장착
    /// </summary>
    public bool TryTakeOutToHand(Transform hand)
    {
        if (!hand) return false;
        if (_stored.Count == 0) return false;

        var pick = _stored.Pop();

        // capacityUsed 되돌리기 (Optional)
        int cost = 1;
        var carry = pick.GetComponent<CarryableItem>();
        if (carry) cost = carry.capacityCost;

        CapacityUsed = Mathf.Max(0, CapacityUsed - cost);
        onCapacityChanged?.Invoke(CapacityUsed, capacityMax);

        // 다시 보이게
        if (hideItemInScene)
            pick.gameObject.SetActive(true);

        // 손에 장착 (레이어/상태 전환은 PickupableItem 책임)
        pick.PickUp(hand);

        Debug.Log($"[CartInventory] TakeOut -> Hand: {pick.name} (used {CapacityUsed}/{capacityMax})");
        return true;
    }

    public int StoredCount => _stored.Count;
}
