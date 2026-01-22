using UnityEngine;

/// <summary>
/// 플레이어 아이템 집기/놓기 제어
/// 키 설정: E(인벤토리 추가), Mouse 1(집기/놓기)
/// </summary>
public class PlayerPickupController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform hand;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;

    private PickupableItem currentItem = null;
    private Camera cam;
    private Inventory inventory;

    private void Start()
    {
        cam = Camera.main;
        inventory = GetComponent<Inventory>();
    }

    private void Update()
    {
        if (GameManager.GameIsPaused) return;

        HandleInventoryInput();
        HandlePickupInput();
    }

    private void HandleInventoryInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentItem != null)
        {
            AddToInventory();
        }
    }

    private void HandlePickupInput()
    {
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

    private void AddToInventory()
    {
        if (currentItem != null && inventory != null)
        {
            if (inventory.AddItem(currentItem))
            {
                currentItem = null;
            }
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