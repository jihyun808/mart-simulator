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

    public bool AddItem(PickupableItem item)
    {
        int itemSize = item.GetItemSize();

        if (currentCapacity + itemSize > maxCapacity)
            return false;

        items.Add(item);
        currentCapacity += itemSize;
        item.gameObject.SetActive(false);

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