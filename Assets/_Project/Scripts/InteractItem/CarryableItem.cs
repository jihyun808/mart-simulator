using UnityEngine;

/// <summary>
/// 카트에 실을 수 있는 아이템 (물리 비활성화 및 위치 고정)
/// MarkLoaded/Unload로 카트 적재 상태 관리
/// </summary>
public class CarryableItem : MonoBehaviour
{
    [Header("Item Settings")]
    public int capacityCost = 1;
    public bool usePickupSize = true;
    
    public bool IsLoaded { get; private set; }

    private Rigidbody rb;
    private Collider[] cols;
    private int originalLayer = -1;
    private Transform anchor;
    private Vector3 localPosOnAnchor;
    private Quaternion localRotOnAnchor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cols = GetComponentsInChildren<Collider>(true);
        originalLayer = gameObject.layer;
    }

    /// <summary>카트에 적재 (물리 비활성화 및 위치 고정)</summary>
    public void MarkLoaded(Transform parent)
    {
        if (IsLoaded) return;
        IsLoaded = true;

        transform.SetParent(parent, true);
        anchor = parent;
        localPosOnAnchor = parent.InverseTransformPoint(transform.position);
        localRotOnAnchor = Quaternion.Inverse(parent.rotation) * transform.rotation;

        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.detectCollisions = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (cols != null)
        {
            foreach (var c in cols)
            {
                if (c) c.enabled = false;
            }
        }

        int loadedLayer = LayerMask.NameToLayer("LoadedItem");
        if (loadedLayer >= 0) gameObject.layer = loadedLayer;
    }

    /// <summary>카트에서 제거 (물리 활성화)</summary>
    public void Unload()
    {
        if (!IsLoaded) return;
        IsLoaded = false;

        transform.SetParent(null, true);

        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (cols != null)
        {
            foreach (var c in cols)
            {
                if (c && !c.isTrigger) c.enabled = true;
            }
        }

        if (originalLayer >= 0) gameObject.layer = originalLayer;

        anchor = null;
    }

    private void LateUpdate()
    {
        if (IsLoaded && anchor)
        {
            transform.position = anchor.TransformPoint(localPosOnAnchor);
            transform.rotation = anchor.rotation * localRotOnAnchor;
        }
    }
}