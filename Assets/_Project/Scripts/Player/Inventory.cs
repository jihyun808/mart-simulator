using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxCapacity = 4;

    private PickupableItem[] slots = new PickupableItem[4]; 
    private int currentCapacity = 0; 

    [Header("UI References")]
    public InventoryUI inventoryUI;           // 하단 슬롯 UI
    public TopPanelManager topPanelManager;   // 상단 패널 (0/4 표시)
    public ShoppingListManager shoppingListManager; // ⭐ 쇼핑 리스트 매니저

    private void Start()
    {
        if (inventoryUI == null) inventoryUI = FindObjectOfType<InventoryUI>();
        if (topPanelManager == null) topPanelManager = FindObjectOfType<TopPanelManager>();
        
        // ⭐ 쇼핑 리스트 매니저 자동 찾기
        if (shoppingListManager == null) shoppingListManager = FindObjectOfType<ShoppingListManager>();

        UpdateUI();
    }

    public bool CheckCanAdd(int slotIndex, PickupableItem item)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        if (slots[slotIndex] != null) return false; 
        if (currentCapacity + item.GetItemSize() > maxCapacity) return false; 
        return true;
    }

    public bool AddItem(PickupableItem item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) return TryAddItemToSlot(i, item); 
        }
        return false;
    }

    public List<PickupableItem> GetAllItems()
    {
        List<PickupableItem> activeItems = new List<PickupableItem>();
        foreach (var item in slots)
        {
            if (item != null) activeItems.Add(item);
        }
        return activeItems;
    }

    public void RemoveItem(PickupableItem item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == item)
            {
                DropItem(i, false);
                return;
            }
        }
    }

    public int GetItemCount()
    {
        int count = 0;
        foreach (var item in slots) { if (item != null) count++; }
        return count;
    }

    public PickupableItem GetItem(int index)
    {
        if (index >= 0 && index < slots.Length) return slots[index];
        return null;
    }

    public bool TryAddItemToSlot(int slotIndex, PickupableItem item)
    {
        if (!CheckCanAdd(slotIndex, item)) return false;

        slots[slotIndex] = item;
        currentCapacity += item.GetItemSize();
        
        item.gameObject.SetActive(false); 

        UpdateUI(); 
        Debug.Log($"{slotIndex + 1}번 슬롯에 저장 완료!");
        return true;
    }

    public PickupableItem DropItem(int slotIndex, bool dropToWorld = true)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;
        if (slots[slotIndex] == null) return null;

        PickupableItem item = slots[slotIndex];

        currentCapacity -= item.GetItemSize();
        slots[slotIndex] = null; 

        if (dropToWorld) item.gameObject.SetActive(true);
        
        UpdateUI();
        return item;
    }

    private void UpdateUI()
    {
        // 1. 하단 슬롯 갱신
        if (inventoryUI != null) inventoryUI.UpdateUI(slots);

        // 2. 상단 패널 용량 갱신
        if (topPanelManager != null) topPanelManager.UpdateBagDisplay(currentCapacity, maxCapacity);

        // ⭐ 3. 쇼핑 리스트 상태 갱신 (추가된 부분)
        if (shoppingListManager != null)
        {
            shoppingListManager.UpdateCheckList(); // 수량 다시 계산
            
            // 리스트 UI가 켜져 있으면 화면도 즉시 갱신
            ShoppingListUI listUI = FindObjectOfType<ShoppingListUI>();
            if (listUI != null && listUI.gameObject.activeInHierarchy)
            {
                listUI.RefreshUI();
            }
        }
    }

    public int GetCurrentCapacity() => currentCapacity;
    public int GetMaxCapacity() => maxCapacity;
}