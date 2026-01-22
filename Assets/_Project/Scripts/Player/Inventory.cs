using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어 인벤토리 관리 (최대 4개 슬롯)
/// 아이템 추가/제거 및 UI 자동 갱신
/// </summary>
public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxCapacity = 4;

    [Header("UI References")]
    public InventoryUI inventoryUI;
    public TopPanelManager topPanelManager;
    public ShoppingListManager shoppingListManager;

    private PickupableItem[] slots = new PickupableItem[4];
    private int currentCapacity = 0;

    private void Start()
    {
        if (inventoryUI == null) inventoryUI = FindObjectOfType<InventoryUI>();
        if (topPanelManager == null) topPanelManager = FindObjectOfType<TopPanelManager>();
        if (shoppingListManager == null) shoppingListManager = FindObjectOfType<ShoppingListManager>();

        UpdateUI();
    }

    /// <summary>슬롯에 아이템 추가 가능 여부 확인</summary>
    public bool CheckCanAdd(int slotIndex, PickupableItem item)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        if (slots[slotIndex] != null) return false;
        if (currentCapacity + item.GetItemSize() > maxCapacity) return false;
        return true;
    }

    /// <summary>비어있는 첫 번째 슬롯에 아이템 추가</summary>
    public bool AddItem(PickupableItem item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) return TryAddItemToSlot(i, item);
        }
        return false;
    }

    /// <summary>인벤토리에 있는 모든 아이템 반환</summary>
    public List<PickupableItem> GetAllItems()
    {
        List<PickupableItem> activeItems = new List<PickupableItem>();
        foreach (var item in slots)
        {
            if (item != null) activeItems.Add(item);
        }
        return activeItems;
    }

    /// <summary>특정 아이템 제거</summary>
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

    /// <summary>인벤토리에 있는 아이템 개수 반환</summary>
    public int GetItemCount()
    {
        int count = 0;
        foreach (var item in slots)
        {
            if (item != null) count++;
        }
        return count;
    }

    /// <summary>특정 슬롯의 아이템 반환</summary>
    public PickupableItem GetItem(int index)
    {
        if (index >= 0 && index < slots.Length) return slots[index];
        return null;
    }

    /// <summary>특정 슬롯에 아이템 추가</summary>
    public bool TryAddItemToSlot(int slotIndex, PickupableItem item)
    {
        if (!CheckCanAdd(slotIndex, item)) return false;

        slots[slotIndex] = item;
        currentCapacity += item.GetItemSize();
        item.gameObject.SetActive(false);

        UpdateUI();
        return true;
    }

    /// <summary>특정 슬롯에서 아이템 제거</summary>
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
        if (inventoryUI != null)
        {
            inventoryUI.UpdateUI(slots);
        }

        if (topPanelManager != null)
        {
            topPanelManager.UpdateBagDisplay(currentCapacity, maxCapacity);
        }

        if (shoppingListManager != null)
        {
            shoppingListManager.UpdateCheckList();
        }
    }

    public int GetCurrentCapacity() => currentCapacity;
    public int GetMaxCapacity() => maxCapacity;
}