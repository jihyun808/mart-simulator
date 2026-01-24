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
    public LayerMask itemMask;

    [Header("Store Behavior")]
    public bool hideItemInScene = true;
    public float justDroppedWindow = 0.6f;

    [Header("Storage Layer (Optional)")]
    public bool changeLayerWhileStored = true;
    public string storedLayerName = "Ignore Raycast"; // ✅ 저장 중 레이어
    // 원하면 "LoadedItem" 같은 네 레이어로 바꿔도 됨

    
    private readonly Stack<PickupableItem> _stored = new();

    private class SavedState
    {
        public bool[] colliderEnabled;
        public bool[] rendererEnabled;
        public bool rbKinematic;
        public bool rbUseGravity;
        public bool rbDetectCollisions;
        public int originalLayer; // ✅ 추가: 레이어 저장
    }
    private readonly Dictionary<PickupableItem, SavedState> _saved = new();

    private int _storedLayer = -1;

    private void Awake()
    {
        if (changeLayerWhileStored)
        {
            _storedLayer = LayerMask.NameToLayer(storedLayerName);
            if (_storedLayer == -1)
            {
                Debug.LogWarning($"[CartInventory] storedLayerName='{storedLayerName}' 레이어를 찾을 수 없습니다. 레이어 변경 기능이 비활성화됩니다.");
                changeLayerWhileStored = false;
            }
        }
    }

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) => TryStore(other);
    private void OnTriggerStay(Collider other)  => TryStore(other);

    private void TryStore(Collider other)
    {
        if (((1 << other.gameObject.layer) & itemMask.value) == 0) return;

        var pick = other.GetComponentInParent<PickupableItem>();
        if (!pick) return;

        if (pick.IsCarried()) return;
        if (pick.WasJustPickedUp()) return;
        if (!pick.WasJustDropped(justDroppedWindow)) return;

        if (_stored.Contains(pick)) return;

        int cost = 1;
        var carry = pick.GetComponent<CarryableItem>();
        if (carry) cost = carry.capacityCost;
        if (CapacityUsed + cost > capacityMax) return;

        // ✅ 담기 직전 상태 확정 (핵심)
        pick.Drop();   // ← 이 줄 추가

        _stored.Push(pick);
        CapacityUsed += cost;
        onCapacityChanged?.Invoke(CapacityUsed, capacityMax);

        if (hideItemInScene)
            DisableForStorage(pick);

        Debug.Log($"[CartInventory] Stored: {pick.name} (used {CapacityUsed}/{capacityMax})");
    }

    public bool TryTakeOutToHand(Transform hand)
{
    if (!hand) return false;
    if (_stored.Count == 0) return false;

    var pick = _stored.Pop();

    // 복구 로직들 …
    if (hideItemInScene)
        RestoreAfterStorage(pick);

    // 🔥 여기!
    var controller = hand.GetComponentInParent<PlayerPickupController>();
    if (controller != null)
    {
        controller.ForcePickUp(pick);
    }
    else
    {
        pick.PickUp(hand); // fallback
    }

    Debug.Log($"[CartInventory] TakeOut -> Hand: {pick.name}");
    return true;
}

    public int StoredCount => _stored.Count;

    private void DisableForStorage(PickupableItem pick)
    {
        if (!_saved.ContainsKey(pick))
        {
            var cols = pick.GetComponentsInChildren<Collider>(true);
            var rends = pick.GetComponentsInChildren<Renderer>(true);
            var rb = pick.GetComponent<Rigidbody>();

            var s = new SavedState
            {
                colliderEnabled = new bool[cols.Length],
                rendererEnabled = new bool[rends.Length],
                rbKinematic = rb ? rb.isKinematic : false,
                rbUseGravity = rb ? rb.useGravity : false,
                rbDetectCollisions = rb ? rb.detectCollisions : false,
                originalLayer = pick.gameObject.layer // ✅ 레이어 저장
            };

            for (int i = 0; i < cols.Length; i++) s.colliderEnabled[i] = cols[i].enabled;
            for (int i = 0; i < rends.Length; i++) s.rendererEnabled[i] = rends[i].enabled;

            _saved[pick] = s;
        }

        // 저장 중 레이어 변경 (Raycast/상호작용 차단)
        if (changeLayerWhileStored && _storedLayer != -1)
            pick.gameObject.layer = _storedLayer;

        // 렌더/콜라이더 끄기
        foreach (var r in pick.GetComponentsInChildren<Renderer>(true))
            r.enabled = false;

        foreach (var c in pick.GetComponentsInChildren<Collider>(true))
            c.enabled = false;

        // 물리 끄기
        var rb2 = pick.GetComponent<Rigidbody>();
        if (rb2)
        {
            rb2.isKinematic = true;
            rb2.useGravity = false;
            rb2.detectCollisions = false;
            rb2.linearVelocity = Vector3.zero;
            rb2.angularVelocity = Vector3.zero;
        }
    }

    private void RestoreAfterStorage(PickupableItem pick)
    {
        if (!_saved.TryGetValue(pick, out var s))
        {
            // 기록이 없으면 최소 복구
            foreach (var r in pick.GetComponentsInChildren<Renderer>(true)) r.enabled = true;
            foreach (var c in pick.GetComponentsInChildren<Collider>(true)) c.enabled = true;

            var rb = pick.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.detectCollisions = true;
                rb.useGravity = true;
                rb.isKinematic = false;
            }
            return;
        }

        // ✅ 레이어 원복 (상호작용 부활의 핵심 포인트)
        pick.gameObject.layer = s.originalLayer;

        var cols = pick.GetComponentsInChildren<Collider>(true);
        var rends = pick.GetComponentsInChildren<Renderer>(true);
        var rb2 = pick.GetComponent<Rigidbody>();

        for (int i = 0; i < cols.Length && i < s.colliderEnabled.Length; i++)
            cols[i].enabled = s.colliderEnabled[i];

        for (int i = 0; i < rends.Length && i < s.rendererEnabled.Length; i++)
            rends[i].enabled = s.rendererEnabled[i];

        if (rb2)
        {
            rb2.isKinematic = s.rbKinematic;
            rb2.useGravity = s.rbUseGravity;
            rb2.detectCollisions = s.rbDetectCollisions;
        }

        _saved.Remove(pick);

        Debug.Log($"[CartInventory] Restored: {pick.name}, layer={LayerMask.LayerToName(pick.gameObject.layer)}");

    }

        public bool TryStealOne(out PickupableItem stolenItem)
    {
        stolenItem = null;

        if (_stored.Count == 0)
            return false;

        // 1️⃣ 아이템 꺼내기
        stolenItem = _stored.Pop();

        // 2️⃣ capacity 되돌리기 (있다면)
        int cost = 1;
        var carry = stolenItem.GetComponent<CarryableItem>();
        if (carry) cost = carry.capacityCost;

        CapacityUsed = Mathf.Max(0, CapacityUsed - cost);
        onCapacityChanged?.Invoke(CapacityUsed, capacityMax);

        // 3️⃣ 저장 상태 복구 (렌더러 / 콜라이더 / 레이어 / Rigidbody)
        if (hideItemInScene)
            RestoreAfterStorage(stolenItem);

        Debug.Log($"[CartInventory] ❌ Stolen: {stolenItem.name} (used {CapacityUsed}/{capacityMax})");

        return true;
    }
}
