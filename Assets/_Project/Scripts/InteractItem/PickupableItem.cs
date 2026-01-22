using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupableItem : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName;
    [SerializeField] private int itemSize = 1;
    [SerializeField] private int itemValue = 0;

    [Header("Inventory Icon")]
    public Sprite itemIcon;

    private Rigidbody rb;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isCarried = false;

    // ─────────────────────────────────────
    // Drop / Pickup 타이밍 관리
    // ─────────────────────────────────────
    float _lastDroppedTime = -999f;
    float _lastPickedUpTime = -999f;

    [Header("Timing Windows")]
    public float justDroppedWindow = 0.6f;
    public float justPickedUpWindow = 0.3f; // ✅ 추가 (핵심)

    // ─────────────────────────────────────
    // Layer
    // ─────────────────────────────────────
    int _pickupLayer;
    int _carriedLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        _pickupLayer  = LayerMask.NameToLayer("PickupableItem");
        _carriedLayer = LayerMask.NameToLayer("CarriedItem");
    }

    // ─────────────────────────────────────
    // PickUp / Drop
    // ─────────────────────────────────────
    public void PickUp(Transform hand)
    {
        if (isCarried) return;
        isCarried = true;

        _lastPickedUpTime = Time.time; // ✅ 핵심 포인트

        if (_carriedLayer != -1)
            gameObject.layer = _carriedLayer;

        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"[PickupableItem] PickUp: {name}, layer={LayerMask.LayerToName(gameObject.layer)}, carried={isCarried}");
    }

    public void Drop()
    {
        if (!isCarried) return;
        isCarried = false;

        transform.SetParent(originalParent);

        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (_pickupLayer != -1)
            gameObject.layer = _pickupLayer;

        _lastDroppedTime = Time.time;
    }

    // ─────────────────────────────────────
    // State Queries (외부에서 쓰는 API)
    // ─────────────────────────────────────
    public bool IsCarried() => isCarried;

    public bool WasJustDropped(float window = -1f)
    {
        if (window <= 0f) window = justDroppedWindow;
        return Time.time - _lastDroppedTime <= window;
    }

    // ✅ 이번 리팩토링의 핵심 API
    public bool WasJustPickedUp(float window = -1f)
    {
        if (window <= 0f) window = justPickedUpWindow;
        return Time.time - _lastPickedUpTime <= window;
    }

    public int GetItemSize() => Mathf.Max(1, itemSize);
    public int GetItemValue() => itemValue;

    public void ResetToOriginalPosition()
    {
        if (isCarried) Drop();

        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}
