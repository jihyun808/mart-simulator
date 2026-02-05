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
    public string storedLayerName = "Ignore Raycast";

    /* ─────────────────────────────
     * 🔧 Abandon (방치) Settings
     * ───────────────────────────── */
    [Header("Abandon")]
    [SerializeField] private float abandonTime = 10f;
    [SerializeField] private float stealCooldown = 3f;
    private float lastStolenTime;
    private float lastInteractionTime;

    public bool IsAbandoned =>
        Time.time - lastInteractionTime >= abandonTime;
    public bool CanBeStolen =>
        IsAbandoned && Time.time - lastStolenTime >= stealCooldown;

    public float AbandonRatio =>
    Mathf.Clamp01((Time.time - lastInteractionTime) / abandonTime);
    /* ───────────────────────────── */

    private readonly Stack<PickupableItem> _stored = new();

    private class SavedState
    {
        public bool[] colliderEnabled;
        public bool[] rendererEnabled;
        public bool rbKinematic;
        public bool rbUseGravity;
        public bool rbDetectCollisions;
        public int originalLayer;
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
                Debug.LogWarning($"[CartInventory] storedLayerName='{storedLayerName}' 레이어를 찾을 수 없습니다.");
                changeLayerWhileStored = false;
            }
        }
    }

    private void Start()
    {
        NotifyInteraction();
    }
    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) => TryStore(other);
    private void OnTriggerStay(Collider other)  => TryStore(other);

    /* ─────────────────────────────
     * 🔧 Public API
     * ───────────────────────────── */

    public int StoredCount => _stored.Count;

    public void NotifyInteraction()
    {
        lastInteractionTime = Time.time;
    }

    public bool TryTakeOutToHand(Transform hand)
    {
        if (!hand || _stored.Count == 0) return false;

        NotifyInteraction(); // 🔧 방치 타이머 리셋

        var pick = _stored.Pop();
        ReduceCapacity(pick); // 🔧 capacity 감소 누락 보완

        if (hideItemInScene)
            RestoreAfterStorage(pick);

        var controller = hand.GetComponentInParent<PlayerPickupController>();
        if (controller != null)
            controller.ForcePickUp(pick);
        else
            pick.PickUp(hand);

        Debug.Log($"[CartInventory] TakeOut -> Hand: {pick.name}");
        return true;
    }

    public bool TryStealOne(out PickupableItem stolenItem)
    {
        stolenItem = null;

        if (_stored.Count == 0)
            return false;

        // 🔧 경쟁자 훔치기 조건은 외부에서 IsAbandoned로 체크
        stolenItem = _stored.Pop();
        ReduceCapacity(stolenItem);

        if (hideItemInScene)
            RestoreAfterStorage(stolenItem);

        Debug.Log($"[CartInventory] ❌ Stolen: {stolenItem.name}");
        return true;
    }

    /* ─────────────────────────────
     * Internal
     * ───────────────────────────── */

    private void TryStore(Collider other)
    {
        if (((1 << other.gameObject.layer) & itemMask.value) == 0) return;

        var pick = other.GetComponentInParent<PickupableItem>();
        if (!pick) return;

        if (pick.IsCarried()) return;
        if (pick.WasJustPickedUp()) return;
        if (!pick.WasJustDropped(justDroppedWindow)) return;
        if (_stored.Contains(pick)) return;

        int cost = GetItemCost(pick);
        if (CapacityUsed + cost > capacityMax) return;

        NotifyInteraction(); // 🔧 상호작용 갱신

        pick.Drop();

        _stored.Push(pick);
        CapacityUsed += cost;
        onCapacityChanged?.Invoke(CapacityUsed, capacityMax);

        if (hideItemInScene)
            DisableForStorage(pick);

        Debug.Log($"[CartInventory] Stored: {pick.name} (used {CapacityUsed}/{capacityMax})");
    }

    private int GetItemCost(PickupableItem item)
    {
        var carry = item.GetComponent<CarryableItem>();
        return carry ? carry.capacityCost : 1;
    }

    private void ReduceCapacity(PickupableItem item)
    {
        CapacityUsed = Mathf.Max(0, CapacityUsed - GetItemCost(item));
        onCapacityChanged?.Invoke(CapacityUsed, capacityMax);
    }

    /* ─────────────────────────────
     * Storage Visual / Physics
     * ───────────────────────────── */

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
                originalLayer = pick.gameObject.layer
            };

            for (int i = 0; i < cols.Length; i++) s.colliderEnabled[i] = cols[i].enabled;
            for (int i = 0; i < rends.Length; i++) s.rendererEnabled[i] = rends[i].enabled;

            _saved[pick] = s;
        }

        if (changeLayerWhileStored && _storedLayer != -1)
            pick.gameObject.layer = _storedLayer;

        foreach (var r in pick.GetComponentsInChildren<Renderer>(true))
            r.enabled = false;

        foreach (var c in pick.GetComponentsInChildren<Collider>(true))
            c.enabled = false;

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
        if (!_saved.TryGetValue(pick, out var s)) return;

        pick.gameObject.layer = s.originalLayer;

        var cols = pick.GetComponentsInChildren<Collider>(true);
        var rends = pick.GetComponentsInChildren<Renderer>(true);
        var rb2 = pick.GetComponent<Rigidbody>();

        for (int i = 0; i < cols.Length; i++) cols[i].enabled = s.colliderEnabled[i];
        for (int i = 0; i < rends.Length; i++) rends[i].enabled = s.rendererEnabled[i];

        if (rb2)
        {
            rb2.isKinematic = s.rbKinematic;
            rb2.useGravity = s.rbUseGravity;
            rb2.detectCollisions = s.rbDetectCollisions;
        }

        _saved.Remove(pick);
    }
}
