// PlayerPickupController.cs
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
    private InputHandler inputHandler;

    private void Start()
    {
        cam = Camera.main;
        inventory = GetComponent<Inventory>();
        inputHandler = GetComponent<InputHandler>();
        
        if (inventory == null)
        {
            inventory = gameObject.AddComponent<Inventory>();
        }
    }

    private void Update()
    {
        if (GameManager.GameIsPaused)
            return;

        HandleInventoryInput();
        HandlePickupInput();
    }

    private void HandleInventoryInput()
    {
        if (inputHandler != null && inputHandler.IsInteractPressed() && currentItem != null)
        {
            AddToInventory();
        }
    }

    private void HandlePickupInput()
    {
        if (inputHandler != null && inputHandler.IsGrabPressed())
        {
            if (currentItem == null)
                TryPickup();
            else
                DropItem();
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