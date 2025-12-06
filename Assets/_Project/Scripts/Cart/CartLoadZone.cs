using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CartLoadZone : MonoBehaviour
{
    public Transform anchorParent;
    public float acceptSpeed = 0.2f;
    public LayerMask itemMask;

    private CartInventory _inventory;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        _inventory = GetComponentInParent<CartInventory>();
        if (anchorParent == null) anchorParent = transform;
    }

    void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & itemMask.value) == 0) return;

        Rigidbody rb = other.attachedRigidbody;
        if (!rb || rb.linearVelocity.sqrMagnitude > acceptSpeed * acceptSpeed) return;

        var item = other.GetComponent<CarryableItem>();
        if (item == null || item.IsLoaded) return;

        if (_inventory.TryAdd(item))
        {
            item.MarkLoaded(anchorParent);
        }
    }
}
