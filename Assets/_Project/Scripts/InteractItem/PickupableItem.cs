using UnityEngine;

public class PickupableItem : MonoBehaviour
{
    [Header("아이템 정보")]
    [SerializeField] private int itemSize = 1; // 아이템이 차지하는 용량
    [SerializeField] private int itemValue = 0; // 아이템의 값
    
    [Header("인벤토리 아이콘")]
    [SerializeField] private Sprite itemIcon;   // ★ 인벤토리 아이콘 추가!

    public Sprite ItemIcon => itemIcon;
    public Sprite GetItemIcon() => itemIcon;
    private Rigidbody rb;
    private Transform originalParent;
    private bool isCarried = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PickUp(Transform hand)
    {
        isCarried = true;
        originalParent = transform.parent;
        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public void Drop()
    {
        isCarried = false;
        transform.SetParent(originalParent);

        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    public bool IsCarried()
    {
        return isCarried;
    }

    public int GetItemSize()
    {
        return itemSize;
    }

    public int GetItemValue()
    {
        return itemValue;
    }
}