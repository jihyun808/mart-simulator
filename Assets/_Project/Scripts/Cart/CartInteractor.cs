using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class CartInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float useDistance = 3f;
    public LayerMask cartMask;          // 카트 레이어
    public Transform playerPivot;       // 탐색 기준 (카메라/몸)
    public Transform playerHand;        // ✅ 우클릭 꺼내기용 손 Transform

    [Header("Fallback when no cart nearby")]
    public UnityEvent onEPressedWhenNoCart;

    [Header("UI (optional)")]
    public UnityEvent<bool> onCartNearbyChanged;

    private CartMount mounted;
    private bool cartNearby;

    void Update()
    {
        // 0) 근처 카트 탐색
        bool nowNearby = FindNearestCart(out CartMount nearest);
        if (nowNearby != cartNearby)
        {
            cartNearby = nowNearby;
            onCartNearbyChanged?.Invoke(cartNearby);
        }

        // 1) E키: 장착/해제
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (mounted != null)
            {
                mounted.Unmount();
                mounted = null;
                return;
            }

            if (nearest != null)
            {
                nearest.Mount(playerPivot);
                mounted = nearest;
                Debug.Log("[CartInteractor] Mounted cart: " + nearest.name);
                return;
            }

            onEPressedWhenNoCart?.Invoke();
        }

        // 2) 우클릭: 카트에서 아이템 꺼내기 (카트를 잡고 있을 때만)
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (mounted == null) return;
            if (!playerHand)
            {
                Debug.LogWarning("[CartInteractor] playerHand is not assigned.");
                return;
            }

            var inv = mounted.GetComponentInChildren<CartInventory>(true);
            if (!inv)
            {
                Debug.LogWarning("[CartInteractor] CartInventory not found under mounted cart.");
                return;
            }

            inv.TryTakeOutToHand(playerHand);
        }
    }

    public bool IsCartMounted()
{
    return mounted != null;
}

    bool FindNearestCart(out CartMount nearest)
    {
        nearest = null;
        if (!playerPivot) return false;

        var hits = Physics.OverlapSphere(playerPivot.position, useDistance, cartMask,
            QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0) return false;

        float best = float.PositiveInfinity;
        foreach (var h in hits)
        {
            var cart = h.GetComponentInParent<CartMount>();
            if (!cart) continue;

            float d = (h.transform.position - playerPivot.position).sqrMagnitude;
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
