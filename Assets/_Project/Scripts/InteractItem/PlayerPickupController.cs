using UnityEngine;

/// <summary>
/// 플레이어 아이템 집기/놓기 제어
/// Raycast 감지 시 OutlineEffect 자동 추가 및 하이라이트 관리
/// </summary>
public class PlayerPickupController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform hand;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;

    private PickupableItem currentItem = null;
    private OutlineEffect currentOutline = null; // ⭐ Outline 직접 관리
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (GameManager.GameIsPaused) return;

        UpdateHighlight();
        HandlePickupInput();
    }

    /// <summary>
    /// 마우스 오버 시 자동으로 OutlineEffect 추가 및 하이라이트
    /// </summary>
    private void UpdateHighlight()
    {
        // 손에 아이템 들고 있으면 하이라이트 정리
        if (currentItem != null)
        {
            ClearOutline();
            return;
        }

        Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(centerRay, out RaycastHit hit, pickupRange, pickupLayer))
        {
            PickupableItem item = hit.collider.GetComponent<PickupableItem>();

            if (item != null && !item.IsCarried())
            {
                OutlineEffect outline = item.GetComponent<OutlineEffect>();

                // ⭐ 없으면 자동 추가
                if (outline == null)
                {
                    outline = item.gameObject.AddComponent<OutlineEffect>();
                }

                // 새로운 아이템이면 이전 외곽선 끄고 새 외곽선 켜기
                if (currentOutline != outline)
                {
                    ClearOutline();
                    currentOutline = outline;
                    currentOutline.SetOutlineActive(true);
                }
                return;
            }
        }

        // 아무것도 안 가리킴
        ClearOutline();
    }

    /// <summary>
    /// 현재 외곽선 정리
    /// </summary>
    private void ClearOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.SetOutlineActive(false);
            currentOutline = null;
        }
    }

    public void ForcePickUp(PickupableItem item)
    {
        if (item == null) return;

        if (currentItem != null)
        {
            currentItem.Drop();
            currentItem = null;
        }

        currentItem = item;
        item.PickUp(hand);
        
        // ⭐ 외곽선 정리
        ClearOutline();
        
        Debug.Log($"[ForcePickUp] {item.name} picked up via Cart");
    }

    private void HandlePickupInput()
    {
        // 마우스 우클릭(1)으로 집기/놓기
        if (Input.GetMouseButtonDown(1))
        {
            if (currentItem == null)
            {
                TryPickup();
            }
            else
            {
                DropItem();
            }
        }
    }

    private void TryPickup()
    {
        Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(centerRay, out RaycastHit hit, pickupRange, pickupLayer))
        {
            PickupableItem item = hit.collider.GetComponent<PickupableItem>();
            if (item != null && !item.IsCarried())
            {
                currentItem = item;
                currentItem.PickUp(hand);
                
                // ⭐ 외곽선 정리
                ClearOutline();
            }
        }
    }

    private void DropItem()
    {
        if (currentItem != null)
        {
            currentItem.Drop();
            currentItem = null;
            
            // ⭐ 외곽선 정리
            ClearOutline();
        }
    }

    public PickupableItem GetCurrentItem()
    {
        return currentItem;
    }

    public void ClearCurrentItem()
    {
        currentItem = null;
    }
}