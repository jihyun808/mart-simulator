using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupableItem : MonoBehaviour
{
    [SerializeField] private int itemSize = 1;
    [SerializeField] private int itemValue = 0;

    [Header("Inventory Icon")]
    public Sprite itemIcon; // ⭐ 인벤토리 아이콘 추가
    
    private Rigidbody rb;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isCarried = false;

    float _lastDroppedTime = -999f;
    public float justDroppedWindow = 0.6f;

    int _pickupLayer;   // PickupableItem 레이어
    int _carriedLayer;  // CarriedItem 레이어

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        _pickupLayer  = LayerMask.NameToLayer("PickupableItem");
        _carriedLayer = LayerMask.NameToLayer("CarriedItem");
    }

    public void PickUp(Transform hand)
    {
        if (isCarried) return;
        isCarried = true;

        // ▶ 들고 있는 동안은 CarriedItem 레이어로 변경
        if (_carriedLayer != -1) gameObject.layer = _carriedLayer;

        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb) { rb.isKinematic = true; rb.useGravity = false; rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
    }

    public void Drop()
    {
        if (!isCarried) return;
        isCarried = false;

        transform.SetParent(originalParent);
        if (rb) { rb.isKinematic = false; rb.useGravity = true; }

        // ▶ 드롭한 즉시 원래 레이어로 복귀(카트가 감지할 수 있게)
        if (_pickupLayer != -1) gameObject.layer = _pickupLayer;

        _lastDroppedTime = Time.time; // 방금 드롭 기록
    }

    public bool IsCarried() => isCarried;
    public int  GetItemSize() => Mathf.Max(1, itemSize);
    public int  GetItemValue() => itemValue;

    public bool WasJustDropped(float window = -1f)
    {
        if (window <= 0f) window = justDroppedWindow;
        return Time.time - _lastDroppedTime <= window;
    }

    public void ResetToOriginalPosition()
    {
        if (isCarried) Drop();
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}
