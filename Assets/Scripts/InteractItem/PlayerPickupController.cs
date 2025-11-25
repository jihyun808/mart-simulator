using UnityEngine;

public class PlayerPickupController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform hand;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;
    
    private PickupableItem currentItem = null;
    private Camera cam;
    private Inventory inventory;

    void Start()
    {
        cam = Camera.main;
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            inventory = gameObject.AddComponent<Inventory>();
        }
    }

    void Update()
    {
        // E를 눌러 인벤에 넣기
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentItem != null)
            {
                AddToInventory();
            }
        }

        // 마우스 우클릭으로 집기
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

        RaycastHit hit;
        
        if (Physics.Raycast(centerRay, out hit, pickupRange, pickupLayer))
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
        if (currentItem != null && inventory != null)
        {
            inventory.AddItem(currentItem);
            currentItem = null;
        }
    }
}