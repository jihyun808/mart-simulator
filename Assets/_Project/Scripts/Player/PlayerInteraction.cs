using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public Inventory myInventory;
    public PlayerPickupController pickupController; 
    public Transform dropPoint; // ⭐ 물건이 떨어질 위치 (플레이어 앞)

    void Update()
    {
        // F1 ~ F4 키 입력 감지
        if (Input.GetKeyDown(KeyCode.F1)) HandleInventoryAction(0);
        else if (Input.GetKeyDown(KeyCode.F2)) HandleInventoryAction(1);
        else if (Input.GetKeyDown(KeyCode.F3)) HandleInventoryAction(2);
        else if (Input.GetKeyDown(KeyCode.F4)) HandleInventoryAction(3);
    }

    // 상황에 따라 넣을지, 뺄지 결정하는 함수
    void HandleInventoryAction(int slotIndex)
    {
        // 1. 현재 플레이어가 손에 물건을 들고 있는가?
        PickupableItem itemInHand = null;
        if (pickupController != null) itemInHand = pickupController.GetCurrentItem();

        if (itemInHand != null)
        {
            // [상황 A] 손에 물건이 있음 -> 인벤토리에 "넣기" 시도
            TryStoreItem(slotIndex, itemInHand);
        }
        else
        {
            // [상황 B] 손이 비어있음 -> 인벤토리에서 "꺼내기(버리기)" 시도
            TryDropItem(slotIndex);
        }
    }

    // 넣기 로직
    void TryStoreItem(int slotIndex, PickupableItem item)
    {
        // 1. 들어갈 수 있는지 체크 (Inventory 스크립트 기능 활용)
        if (!myInventory.CheckCanAdd(slotIndex, item))
        {
            Debug.Log("그 칸은 이미 찼거나, 용량이 부족합니다!");
            return;
        }

        // 2. 손에서 놓기 처리 (물리적으로 떨어지기 전에 잡기 위해)
        // PlayerPickupController가 "나 이제 아무것도 안 들고 있어"라고 인식하게 함
        if (pickupController != null)
        {
            // Drop을 호출하되, 물리 힘이 가해지기 전에 가로챌 것임
            // (만약 Drop() 함수 내부에 Force를 주는 코드가 있다면, 
            //  pickupController.ClearCurrentItem() 같은 함수를 만들어 쓰는 게 더 깔끔함.
            //  지금은 기존 Drop 활용)
            item.Drop(); 
        }

        // 3. 인벤토리에 넣기 (오브젝트 비활성화됨)
        myInventory.TryAddItemToSlot(slotIndex, item);
    }

    // 빼기 로직
    void TryDropItem(int slotIndex)
    {
        // 1. 인벤토리에서 아이템 데이터 꺼내기 (아직 화면엔 안 보임)
        // dropToWorld를 false로 해서, 위치를 우리가 직접 잡아줄 것임
        PickupableItem droppedItem = myInventory.DropItem(slotIndex, false);

        if (droppedItem != null)
        {
            // 2. 아이템 위치를 '버리는 위치(DropPoint)'로 이동
            if (dropPoint != null)
            {
                droppedItem.transform.position = dropPoint.position;
                droppedItem.transform.rotation = dropPoint.rotation;
            }
            else
            {
                // DropPoint 안 만들었으면 그냥 플레이어 위치에
                droppedItem.transform.position = transform.position + transform.forward;
            }

            // 3. 오브젝트 활성화
            droppedItem.gameObject.SetActive(true);

            // 4. 물리 효과 켜기 (바닥으로 떨어지게)
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                // 약간 앞으로 던져주는 느낌 (선택사항)
                rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse); 
            }

            Debug.Log($"{slotIndex + 1}번 슬롯에서 물건을 꺼냈습니다.");
        }
        else
        {
            Debug.Log("그 슬롯은 비어있습니다.");
        }
    }
}