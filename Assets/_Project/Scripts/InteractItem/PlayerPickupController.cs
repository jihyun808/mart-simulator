using UnityEngine;

public class PlayerPickupController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform hand;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;

    private PickupableItem currentItem = null;
    private Camera cam;

    // 🔥 추가: 인벤토리 UI 연결
    [SerializeField] private InventoryUI inventoryUI;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // E → 인벤토리 넣기
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentItem != null)
            {
                AddToInventory();
            }
        }

        // 우클릭 → 집기 / 놓기
        if (Input.GetMouseButtonDown(1))
        {
            if (currentItem == null)
                TryPickup();
            else
                DropItem();
        }
    }

    void TryPickup()
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

    void DropItem()
    {
        if (currentItem != null)
        {
            currentItem.Drop();
            currentItem = null;
        }
    }

    void AddToInventory()
    {
        if (currentItem == null) return;

        // 🔥 UI 슬롯에 아이콘 추가
        bool added = inventoryUI.AddItem(currentItem.ItemIcon);

        if (added)
        {
            currentItem.gameObject.SetActive(false); // 물건 숨기기
            currentItem = null;
        }
        else
        {
            Debug.Log("인벤토리 슬롯이 가득 참!");
        }
    }
}
