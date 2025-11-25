using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxCapacity = 20;
    
    private List<PickupableItem> items = new List<PickupableItem>();
    private int currentCapacity = 0;

    public System.Action OnInventoryChanged;

    private QuestItemChecker questChecker;

    void Start()
    {
        questChecker = GetComponent<QuestItemChecker>();
        if (questChecker == null)
        {
            questChecker = FindFirstObjectByType<QuestItemChecker>();
        }
    }

    public bool AddItem(PickupableItem item)
    {
        int itemSize = item.GetItemSize();
        int itemValue = item.GetItemValue();

        // 용량 초과 체크
        if (currentCapacity + itemSize > maxCapacity)
        {
            Debug.Log($"[Inventory] 인벤토리 꽉 참 (item={item.name}, size={itemSize}), " +
                      $"current={currentCapacity}/{maxCapacity}");
            return false;
        }

        items.Add(item);
        currentCapacity += itemSize;
        item.gameObject.SetActive(false);

        int remainingCapacity = GetRemainingCapacity();

        // 남은 금액 계산
        int remainingMoney = -1;
        int maxMoney = 0;
        if (questChecker != null)
        {
            int currentTotalValue = questChecker.GetCurrentTotalValue();
            maxMoney = questChecker.GetMaxTotalValue();
            remainingMoney = maxMoney - currentTotalValue;
        }

        Debug.Log(
            $"[Inventory] Picked: {item.name} | size={itemSize}, value={itemValue} " +
            $"→ capacity: {currentCapacity}/{maxCapacity} (remaining={remainingCapacity})" +
            (questChecker != null ? $" | money: {remainingMoney}/{maxMoney}" : "")
        );

        OnInventoryChanged?.Invoke();

        return true;
    }

    public void RemoveItem(PickupableItem item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            currentCapacity -= item.GetItemSize();
            OnInventoryChanged?.Invoke();
        }
    }

    public PickupableItem GetItem(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            return items[index];
        }
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
        return items;
    }
}