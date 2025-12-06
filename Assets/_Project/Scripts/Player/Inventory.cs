// Inventory.cs
using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxCapacity = 20;

    private List<PickupableItem> items = new List<PickupableItem>();
    private int currentCapacity = 0;

    public System.Action OnInventoryChanged;

    // ⭐ 추가: UI 참조 캐싱 (FindObjectOfType 반복 방지)
    private InventoryUI inventoryUI;

    private void Awake()
    {
        inventoryUI = FindObjectOfType<InventoryUI>();
    }

    public bool AddItem(PickupableItem item)
    {
        int itemSize = item.GetItemSize();

        // 용량 초과 방지
        if (currentCapacity + itemSize > maxCapacity)
            return false;

        // 실제 인벤토리에 추가
        items.Add(item);
        currentCapacity += itemSize;

        // 오브젝트 비활성화 (팀원 로직 유지!)
        item.gameObject.SetActive(false);

        // ⭐ UI 아이콘 추가 (아이콘이 있고 UI가 있을 때만)
        if (inventoryUI != null && item.itemIcon != null)
        {
            inventoryUI.AddItem(item.itemIcon);
        }

        // ⭐⭐⭐ Bag 카운트 증가 추가!
        FindObjectOfType<TopPanelManager>()?.AddToBag();

        // 이벤트 호출 (팀원 기능 유지!)
        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool RemoveItem(PickupableItem item)
    {
        if (!items.Contains(item))
            return false;

        items.Remove(item);
        currentCapacity -= item.GetItemSize();

        OnInventoryChanged?.Invoke();
        return true;
    }

    public PickupableItem GetItem(int index)
    {
        if (index >= 0 && index < items.Count)
            return items[index];

        return null;
    }

    public int GetItemCount()
    {
        return items.Count;
    }

    public int GetCurrentCapacity()
    {
        return currentCapacity;
    }

    public int GetMaxCapacity()
    {
        return maxCapacity;
    }

    public bool IsFull()
    {
        return currentCapacity >= maxCapacity;
    }

    public int GetRemainingCapacity()
    {
        return maxCapacity - currentCapacity;
    }

    public List<PickupableItem> GetAllItems()
    {
        return new List<PickupableItem>(items);
    }

    public void ClearInventory()
    {
        items.Clear();
        currentCapacity = 0;

        OnInventoryChanged?.Invoke();
    }
}
