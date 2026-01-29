using UnityEngine;

/// <summary>
/// 플레이어 인벤토리 상호작용 (F1~F4 키 전용)
/// 손에 물건 있음: 해당 슬롯에 넣기
/// 손이 비어있음: 해당 슬롯에서 꺼내기
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public Inventory myInventory;
    public PlayerPickupController pickupController;
    public Transform dropPoint;

    private void Update()
    {
        // ⭐ 오직 F키로만 인벤토리 조작
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
            // 손에 들고 있으면 -> 넣기
            TryStoreItem(slotIndex, itemInHand);
        }
        else
        {
            // 손이 비었으면 -> 꺼내기
            TryDropItem(slotIndex);
        }
    }

    private void TryStoreItem(int slotIndex, PickupableItem item)
    {
        // 1. 인벤토리가 꽉 찼거나 슬롯이 이미 차있으면 중단
        if (!myInventory.CheckCanAdd(slotIndex, item)) return;

        // 2. 손에서 아이템 분리 및 상태 초기화
        if (pickupController != null)
        {
            item.Drop(); // 물리적 분리
            pickupController.ClearCurrentItem(); // ⭐ 핵심: 컨트롤러에게 "손 비었다"고 알림
        }

        // 3. 인벤토리에 저장
        myInventory.TryAddItemToSlot(slotIndex, item);
    }

    private void TryDropItem(int slotIndex)
    {
        // 인벤토리에서 아이템 방출 (DropItem 함수가 물리 설정을 다 처리함)
        PickupableItem droppedItem = myInventory.DropItem(slotIndex, false);

        if (droppedItem == null) return;

        // 위치 지정
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

        // 물리력 부여 (Inventory.cs에서 이미 초기화했지만, 앞으로 툭 던지는 느낌을 위해 추가)
        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            // 살짝 앞으로 던짐
            rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);
        }
    }
}