using UnityEngine;

/// <summary>
/// 플레이어 인벤토리 상호작용 (F1~F4 키)
/// 손에 물건 있음: 인벤토리에 저장 / 손이 비어있음: 인벤토리에서 꺼내기
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public Inventory myInventory;
    public PlayerPickupController pickupController;
    public Transform dropPoint;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) HandleInventoryAction(0);
        else if (Input.GetKeyDown(KeyCode.F2)) HandleInventoryAction(1);
        else if (Input.GetKeyDown(KeyCode.F3)) HandleInventoryAction(2);
        else if (Input.GetKeyDown(KeyCode.F4)) HandleInventoryAction(3);
    }

    private void HandleInventoryAction(int slotIndex)
    {
        PickupableItem itemInHand = pickupController?.GetCurrentItem();

        if (itemInHand != null)
        {
            TryStoreItem(slotIndex, itemInHand);
        }
        else
        {
            TryDropItem(slotIndex);
        }
    }

    private void TryStoreItem(int slotIndex, PickupableItem item)
    {
        if (!myInventory.CheckCanAdd(slotIndex, item)) return;

        if (pickupController != null)
        {
            item.Drop();
        }

        myInventory.TryAddItemToSlot(slotIndex, item);
    }

    private void TryDropItem(int slotIndex)
    {
        PickupableItem droppedItem = myInventory.DropItem(slotIndex, false);

        if (droppedItem == null) return;

        if (dropPoint != null)
        {
            droppedItem.transform.position = dropPoint.position;
            droppedItem.transform.rotation = dropPoint.rotation;
        }
        else
        {
            droppedItem.transform.position = transform.position + transform.forward;
        }

        droppedItem.gameObject.SetActive(true);

        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);
        }
    }
}