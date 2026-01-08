using UnityEngine;
using System.Collections.Generic; // 리스트 사용을 위해 필요

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxCapacity = 4; // 플레이어 주머니 용량 (기본 4)

    // 4칸짜리 슬롯 (F1~F4에 대응)
    private PickupableItem[] slots = new PickupableItem[4]; 
    
    // 현재 무게 합계
    private int currentCapacity = 0; 

    public InventoryUI inventoryUI;

    private void Start()
    {
        if (inventoryUI == null) inventoryUI = FindObjectOfType<InventoryUI>();
        UpdateUI();
    }

    // ---------------------------------------------------------
    // ⭐ [1] 현재 발생하는 오류(CheckCanAdd) 해결용 함수
    // ---------------------------------------------------------
    public bool CheckCanAdd(int slotIndex, PickupableItem item)
    {
        // 1. 인덱스 범위 확인
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        
        // 2. 이미 칸이 차있는지 확인
        if (slots[slotIndex] != null) return false; 

        // 3. 무게(용량) 확인
        if (currentCapacity + item.GetItemSize() > maxCapacity) return false; 

        return true; // 넣을 수 있음!
    }

    // ---------------------------------------------------------
    // ⭐ [2] 호환성 패치 (다른 스크립트 오류 해결용)
    // ---------------------------------------------------------

    // 자동 넣기 (PlayerPickupController 등에서 사용)
    public bool AddItem(PickupableItem item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            // 빈 칸을 찾아서 넣기 시도
            if (slots[i] == null)
            {
                return TryAddItemToSlot(i, item); 
            }
        }
        Debug.Log("인벤토리에 빈 슬롯이 없습니다.");
        return false;
    }

    // 모든 아이템 가져오기 (Cashier, QuestItemChecker 등에서 사용)
    public List<PickupableItem> GetAllItems()
    {
        List<PickupableItem> activeItems = new List<PickupableItem>();
        foreach (var item in slots)
        {
            if (item != null) activeItems.Add(item);
        }
        return activeItems;
    }

    // 특정 아이템 제거 (StunHandler 등에서 사용)
    public void RemoveItem(PickupableItem item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == item)
            {
                DropItem(i, false); // 데이터만 삭제
                return;
            }
        }
    }

    // 아이템 개수 세기
    public int GetItemCount()
    {
        int count = 0;
        foreach (var item in slots)
        {
            if (item != null) count++;
        }
        return count;
    }

    // 인덱스로 아이템 가져오기
    public PickupableItem GetItem(int index)
    {
        if (index >= 0 && index < slots.Length)
            return slots[index];
        return null;
    }

    // ---------------------------------------------------------
    // ⭐ [3] 핵심 기능 (특정 슬롯 넣기/빼기)
    // ---------------------------------------------------------

    // 특정 슬롯에 넣기
    public bool TryAddItemToSlot(int slotIndex, PickupableItem item)
    {
        // CheckCanAdd로 미리 검사했지만, 안전을 위해 한 번 더 체크
        if (!CheckCanAdd(slotIndex, item)) return false;

        slots[slotIndex] = item;
        currentCapacity += item.GetItemSize();
        
        // 인벤토리에 들어갔으므로 월드에서 숨김
        item.gameObject.SetActive(false); 

        UpdateUI();
        Debug.Log($"{slotIndex + 1}번 슬롯에 저장 완료!");
        return true;
    }

    // 아이템 빼기 (dropToWorld가 true면 바닥에 생성)
    public PickupableItem DropItem(int slotIndex, bool dropToWorld = true)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;
        if (slots[slotIndex] == null) return null;

        PickupableItem item = slots[slotIndex];

        currentCapacity -= item.GetItemSize();
        slots[slotIndex] = null; // 슬롯 비우기

        if (dropToWorld)
        {
            item.gameObject.SetActive(true);
            // 위치 초기화 등은 호출한 쪽에서 처리
        }
        
        UpdateUI();
        return item;
    }

    // UI 갱신
    private void UpdateUI()
    {
        if (inventoryUI != null) inventoryUI.UpdateUI(slots);
    }

    public int GetCurrentCapacity() => currentCapacity;
    public int GetMaxCapacity() => maxCapacity;
}