using UnityEngine;

/// <summary>
/// 플레이어 아이템 집기/놓기 제어
/// 수정: E키 기능 삭제 (인벤토리 기능은 PlayerInteraction에서 F1~F4로만 처리)
/// </summary>
public class PlayerPickupController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform hand;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;

    private PickupableItem currentItem = null;
    private Camera cam;
    
    // Inventory 참조가 더 이상 필요 없어서 삭제 (PlayerInteraction이 담당)

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (GameManager.GameIsPaused) return;

        // HandleInventoryInput();  <-- ⭐ 삭제됨! (E키 제거)
        HandlePickupInput();
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
        Debug.Log($"[ForcePickUp] {item.name} picked up via Cart");
    }

    private void HandlePickupInput()
    {
        // 마우스 우클릭(1)으로 집기/놓기 (원하신다면 0으로 변경 가능)
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
            }
        }
    }

    private void DropItem()
    {
        if (currentItem != null)
        {
            currentItem.Drop();
            currentItem = null;
        }
    }

    // AddToInventory 함수 삭제됨 (더 이상 여기서 처리하지 않음)

    public PickupableItem GetCurrentItem()
    {
        return currentItem;
    }

    public void ClearCurrentItem()
    {
        currentItem = null;
    }
}