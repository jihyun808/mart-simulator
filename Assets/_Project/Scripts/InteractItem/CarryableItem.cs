using UnityEngine;

public class CarryableItem : MonoBehaviour
{
    public int capacityCost = 1;
    public bool usePickupSize = true;
    public bool IsLoaded { get; private set; }

    private Rigidbody rb;
    private Collider[] colliders;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>(true);
    }

public void MarkLoaded(Transform parent)
{
    if (IsLoaded) return;
    IsLoaded = true;

    // 물리 끔
    if (rb)
    {
        rb.isKinematic = true;
        rb.useGravity  = false;
        rb.detectCollisions = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // ✅ 월드 변환(특히 스케일)을 보존해서 부모 설정
    transform.SetParent(parent, /*worldPositionStays:*/ true);
}

    public void Unload()
    {
        if (!IsLoaded) return;
        IsLoaded = false;

        transform.SetParent(null);
        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        foreach (var c in colliders)
            if (c && !c.isTrigger) c.enabled = true;
    }
}
