using UnityEngine;

/// <summary>
/// 플레이어가 집을 수 있는 아이템
/// 인벤토리 추가, 집기/놓기, 위치 리셋 기능 제공
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PickupableItem : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName;
    [SerializeField] private int itemSize = 1;
    [SerializeField] private int itemValue = 0;

    [Header("Inventory Icon")]
    public Sprite itemIcon;

    [Header("Drop Settings")]
    public float justDroppedWindow = 0.6f;

    private Rigidbody rb;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isCarried = false;
    private float lastDroppedTime = -999f;
    private int pickupLayer;
    private int carriedLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        pickupLayer = LayerMask.NameToLayer("PickupableItem");
        carriedLayer = LayerMask.NameToLayer("CarriedItem");
    }

    /// <summary>아이템 집기 (손에 부착)</summary>
    public void PickUp(Transform hand)
    {
        if (isCarried) return;
        isCarried = true;

        if (carriedLayer != -1)
        {
            gameObject.layer = carriedLayer;
        }

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
    }

    /// <summary>아이템 놓기 (물리 활성화)</summary>
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

        if (pickupLayer != -1)
        {
            gameObject.layer = pickupLayer;
        }

        lastDroppedTime = Time.time;
    }

    /// <summary>원래 위치로 리셋</summary>
    public void ResetToOriginalPosition()
    {
        if (isCarried) Drop();

        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    /// <summary>방금 드롭되었는지 확인 (재집기 방지용)</summary>
    public bool WasJustDropped(float window = -1f)
    {
        if (window <= 0f) window = justDroppedWindow;
        return Time.time - lastDroppedTime <= window;
    }

    public bool IsCarried() => isCarried;
    public int GetItemSize() => Mathf.Max(1, itemSize);
    public int GetItemValue() => itemValue;
}