using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CartInventory : MonoBehaviour
{
    // ===== 용량 =====
    [Header("Capacity")]
    public int capacityMax = 30;
    public int CapacityUsed { get; private set; }
    public UnityEvent<int, int> onCapacityChanged;

    private readonly List<CarryableItem> _items = new();
    public IReadOnlyList<CarryableItem> Items => _items;

    public bool TryAdd(CarryableItem item)
    {
        if (item == null) return false;
        if (_items.Contains(item)) return false;          // ✅ 중복 방지

        int cost = item.capacityCost;
        if (CapacityUsed + cost > capacityMax) return false;

        _items.Add(item);
        CapacityUsed += cost;
        onCapacityChanged?.Invoke(CapacityUsed, capacityMax);
        return true;
    }

    public bool Remove(CarryableItem item)
    {
        if (item == null) return false;
        if (!_items.Remove(item)) return false;

        CapacityUsed -= item.capacityCost;
        onCapacityChanged?.Invoke(CapacityUsed, capacityMax);
        return true;
    }

    // ===== 흡수 규칙 =====
    [Header("Load Rules")]
    [Tooltip("담을 수 있는 아이템 레이어")]
    public LayerMask itemMask;
    [Tooltip("드롭 후 이 시간(초) 이내만 흡수")]
    public float justDroppedWindow = 0.6f;
    [Tooltip("이 속도(m/s)보다 빠르면 스침으로 간주해 무시")]
    public float acceptSpeed = 1.2f;

    // ===== 배치/앵커 =====
    [Header("Anchor")]
    [Tooltip("담긴 아이템의 부모(비우면 이 오브젝트)")]
    public Transform anchorParent;
    [Tooltip("랜덤 배치 박스의 반경(앵커 로컬 기준, x=좌우, z=앞뒤)")]
    public Vector3 localBoxHalfExtents = new Vector3(0.4f, 0f, 0.6f);
    [Tooltip("앵커 기준 높이 오프셋(카트 바닥보다 약간 위)")]
    public float placeYOffset = 0.25f;
    [Tooltip("기존 항목과 수평 최소 간격(m)")]
    public float minSeparation = 0.12f;
    [Tooltip("랜덤 위치 재시도 횟수")]
    public int placementMaxTries = 12;
    [Tooltip("배치 시 허용할 임의 Yaw(도)")]
    public float randomYawMax = 12f;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true; // LoadZone은 반드시 트리거
    }

    private void OnTriggerEnter(Collider other) => TryAbsorb(other);
    private void OnTriggerStay (Collider other) => TryAbsorb(other);

    private void TryAbsorb(Collider other)
    {
        // 1) 레이어 필터
        if (((1 << other.gameObject.layer) & itemMask.value) == 0) return;

        // 2) 구성요소 안전 획득(자식 콜라이더 대응)
        var pick = other.GetComponentInParent<PickupableItem>();
        var item = other.GetComponentInParent<CarryableItem>();
        if (!pick || !item) return;

        // 3) 들고 있거나 이미 담긴 것은 무시
        if (pick.IsCarried()) return;
        if (item.IsLoaded)   return;

        // 4) 방금 드롭된 경우만
        if (!pick.WasJustDropped(justDroppedWindow)) return;

        // 5) 스치는 속도면 무시
        Rigidbody rb = other.attachedRigidbody;
        if (!rb) rb = item.GetComponent<Rigidbody>();
        if (rb && rb.linearVelocity.sqrMagnitude > acceptSpeed * acceptSpeed) return;

        // 6) 용량 및 중복 체크 후 등록
        if (!TryAdd(item)) return;

        // 7) 부모 설정 전에 '현재 월드 스케일' 저장(스케일 중첩 방지)
        Transform t = item.transform;
        Vector3 worldScaleBefore = t.lossyScale;

        // 8) 부모 지정 + CarryableItem 쪽 물리 전환 처리
        var parent = anchorParent ? anchorParent : transform;
        item.MarkLoaded(parent);

        // 9) ★ 월드 스케일 보존(부모 스케일이 1이 아니어도 찌그러지지 않게)
        Vector3 pScale = parent.lossyScale;
        t.localScale = new Vector3(
            SafeDiv(worldScaleBefore.x, pScale.x),
            SafeDiv(worldScaleBefore.y, pScale.y),
            SafeDiv(worldScaleBefore.z, pScale.z)
        );

        // 10) 카트 내부에 랜덤 배치(겹침 최소화)
        PlaceRandomlyInsideBox(item, parent, rb);

        // 11) 안전 정지
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"[CartInventory] ✅ {item.name} absorbed into cart!");
    }

    // --- 배치 유틸 ---
    private void PlaceRandomlyInsideBox(CarryableItem item, Transform anchor, Rigidbody rb)
    {
        Vector3 pickLocal;
        bool placed = false;

        bool IsFarEnough(Vector3 worldPos)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                if (!it || it == item) continue;
                Vector3 p = it.transform.position;
                Vector2 a = new Vector2(worldPos.x, worldPos.z);
                Vector2 b = new Vector2(p.x, p.z);
                if ((a - b).sqrMagnitude < (minSeparation * minSeparation))
                    return false;
            }
            return true;
        }

        for (int t = 0; t < placementMaxTries; t++)
        {
            float lx = Random.Range(-localBoxHalfExtents.x, localBoxHalfExtents.x);
            float lz = Random.Range(-localBoxHalfExtents.z, localBoxHalfExtents.z);
            pickLocal = new Vector3(lx, placeYOffset, lz);

            Vector3 worldPos = anchor.TransformPoint(pickLocal);
            if (IsFarEnough(worldPos))
            {
                item.transform.position = worldPos;

                float yaw = (randomYawMax > 0f) ? Random.Range(-randomYawMax, randomYawMax) : 0f;
                item.transform.rotation = anchor.rotation * Quaternion.Euler(0f, yaw, 0f);

                placed = true;
                break;
            }
        }

        if (!placed)
        {
            // 중앙 Fallback
            Vector3 fallbackLocal = new Vector3(0f, placeYOffset, 0f);
            item.transform.position = anchor.TransformPoint(fallbackLocal);
            item.transform.rotation = anchor.rotation;
        }
    }

    private static float SafeDiv(float a, float b) => Mathf.Approximately(b, 0f) ? 0f : a / b;
}
