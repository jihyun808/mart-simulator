using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class CartInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float useDistance = 3f;
    public LayerMask cartMask;       
    public Transform playerPivot;    
    
    // ⭐ [필수 연결] 물건이 나올 위치 (플레이어 손)
    public Transform playerHand;     

    [Header("Fallback when no cart nearby")]
    public UnityEvent onEPressedWhenNoCart;

    [Header("UI (optional)")]
    public UnityEvent<bool> onCartNearbyChanged;

    private CartMount mounted;
    private bool cartNearby;

    void Update()
    {
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

                // 잡은 카트 정보로 UI 갱신
                CartInventory cartInv = nearest.GetComponentInChildren<CartInventory>();
                if (cartInv != null)
                {
                    var topPanel = FindObjectOfType<TopPanelManager>();
                    if (topPanel != null)
                    {
                        topPanel.UpdateCartDisplay(cartInv.GetCurrentCount(), cartInv.maxCapacity);
                    }
                }
                Debug.Log("[CartInteractor] Mounted cart: " + nearest.name);
                return;
            }
            onEPressedWhenNoCart?.Invoke();
        }

        // 2) 우클릭: 카트에서 아이템 꺼내기
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (mounted == null) return; // 잡고 있는 카트가 없으면 무시

            if (playerHand == null)
            {
                Debug.LogError("🚨 [오류] PlayerHand가 연결되지 않았습니다! Inspector에서 할당해주세요.");
                return;
            }

            var inv = mounted.GetComponentInChildren<CartInventory>(true);
            if (!inv) return;

            // ⭐ 카트에게 '내 손 위치'를 주면서 꺼내달라고 요청
            inv.TryTakeOutToHand(playerHand);
            
            // UI 갱신 (꺼냈으니까 용량 변경)
            var topPanel = FindObjectOfType<TopPanelManager>();
            if (topPanel != null)
            {
                topPanel.UpdateCartDisplay(inv.GetCurrentCount(), inv.maxCapacity);
            }
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