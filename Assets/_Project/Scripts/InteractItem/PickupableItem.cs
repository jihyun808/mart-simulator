// PickupableItem.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupableItem : MonoBehaviour
{
    [Header("Item Info")]
    [SerializeField] private int itemSize = 1;
    [SerializeField] private int itemValue = 0;
    
    private Rigidbody rb;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isCarried = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void PickUp(Transform hand)
    {
        if (isCarried) return;

        isCarried = true;
        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void Drop()
    {
        if (!isCarried) return;

        isCarried = false;
        transform.SetParent(originalParent);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
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

    public void ResetToOriginalPosition()
    {
        if (isCarried) Drop();
        
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}