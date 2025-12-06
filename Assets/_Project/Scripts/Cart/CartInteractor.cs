using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Linq;

public class CartInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float useDistance = 3f;               // 근접 판정 반경
    public LayerMask cartMask;                   // Cart/CartBody 레이어
    public Transform playerPivot;                // 기준 위치(카메라 or 몸 중앙)

    [Header("Fallback when no cart nearby")]
    public UnityEvent onEPressedWhenNoCart;      // 카트 없을 때 E키 동작(기존 행동 연결)

    [Header("UI (optional)")]
    public UnityEvent<bool> onCartNearbyChanged; // 근처에 카트 있으면 true(프롬프트 표시 용도)

    private CartMount mounted;
    private bool cartNearby;

    void Update()
    {
        // 근처 카트 존재 여부를 먼저 계산
        bool nowNearby = FindNearestCart(out CartMount nearest);
        if (nowNearby != cartNearby)
        {
            cartNearby = nowNearby;
            onCartNearbyChanged?.Invoke(cartNearby); // "E 눌러 카트 잡기" 같은 프롬프트 토글
        }

        // E키 입력
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            // 1) 이미 카트를 잡고 있으면 해제 우선
            if (mounted != null)
            {
                mounted.Unmount();
                mounted = null;
                return; // E키 소비
            }

            // 2) 카트가 근처에 있으면 이 스크립트가 E키를 '가로채서' 장착
            if (nearest != null)
            {
                nearest.Mount(playerPivot);
                mounted = nearest;
                Debug.Log("[CartInteractor] Mounted cart: " + nearest.name);
                return; // E키 소비 → 다른 시스템에 안 넘김
            }

            // 3) 근처 카트가 없으면 원래 E키 행동을 실행(이벤트로 연결)
            onEPressedWhenNoCart?.Invoke();
        }
    }

    /// <summary>
    /// 플레이어 주변(useDistance)에서 가장 가까운 CartMount 탐색 (Ray 대신 구 탐색으로 관대한 판정)
    /// </summary>
    bool FindNearestCart(out CartMount nearest)
    {
        nearest = null;
        if (!playerPivot) return false;

        // 플레이어 주변 구(OverlapSphere)로 Cart 레이어 검색 → Ray보다 사용자 친화적
        var hits = Physics.OverlapSphere(playerPivot.position, useDistance, cartMask,
                                         QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

        float best = float.PositiveInfinity;
        foreach (var h in hits)
        {
            var cart = h.GetComponentInParent<CartMount>();
            if (!cart) continue;

            float d = Vector3.SqrMagnitude(h.transform.position - playerPivot.position);
            if (d < best)
            {
                best = d;
                nearest = cart;
            }
        }
        return nearest != null;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!playerPivot) return;
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.2f);
        Gizmos.DrawSphere(playerPivot.position, useDistance);
    }
#endif
}
