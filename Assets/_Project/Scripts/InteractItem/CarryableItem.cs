using UnityEngine;

public class CarryableItem : MonoBehaviour
{
    public int capacityCost = 1;
    public bool usePickupSize = true;
    public bool IsLoaded { get; private set; }

    private Rigidbody rb;
    private Collider[] cols;
    private int originalLayer = -1;

    // ✅ 담긴 뒤에도 따라붙게 유지할 기준
    private Transform _anchor;               // CartInventory.anchorParent or ItemsAnchor
    private Vector3   _localPosOnAnchor;     // 담을 때의 로컬 위치
    private Quaternion _localRotOnAnchor;    // 담을 때의 로컬 회전

    void Awake()
    {
        rb   = GetComponent<Rigidbody>();
        cols = GetComponentsInChildren<Collider>(true);
        originalLayer = gameObject.layer;
    }

    public void MarkLoaded(Transform parent)
    {
        if (IsLoaded) return;
        IsLoaded = true;

        // 1) 부모 설정(월드 유지) + Anchor 저장
        transform.SetParent(parent, true);
        _anchor = parent;
        _localPosOnAnchor = parent.InverseTransformPoint(transform.position);
        _localRotOnAnchor = Quaternion.Inverse(parent.rotation) * transform.rotation;

        // 2) 물리 끄기
        if (rb)
        {
            rb.isKinematic      = true;
            rb.useGravity       = false;
            rb.detectCollisions = false;
            rb.linearVelocity         = Vector3.zero;
            rb.angularVelocity  = Vector3.zero;
        }

        if (cols != null)
            foreach (var c in cols) if (c) c.enabled = false;

        // 3) (선택) LoadedItem 레이어로
        int loadedLayer = LayerMask.NameToLayer("LoadedItem");
        if (loadedLayer >= 0) gameObject.layer = loadedLayer;
    }

    public void Unload()
    {
        if (!IsLoaded) return;
        IsLoaded = false;

        transform.SetParent(null, true);

        if (rb)
        {
            rb.isKinematic      = false;
            rb.useGravity       = true;
            rb.detectCollisions = true;
            rb.linearVelocity         = Vector3.zero;
            rb.angularVelocity  = Vector3.zero;
        }

        if (cols != null)
            foreach (var c in cols) if (c && !c.isTrigger) c.enabled = true;

        if (originalLayer >= 0) gameObject.layer = originalLayer;

        // ✅ 추적 해제
        _anchor = null;
    }

    void LateUpdate()
    {
        // ✅ 부모가 끊기거나, 다른 스크립트가 건드려도 Anchor 기준으로 계속 따라옴
        if (IsLoaded && _anchor)
        {
            transform.position = _anchor.TransformPoint(_localPosOnAnchor);
            transform.rotation = _anchor.rotation * _localRotOnAnchor;
        }
    }
}
