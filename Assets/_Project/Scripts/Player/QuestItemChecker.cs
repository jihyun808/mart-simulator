using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class QuestItemChecker : MonoBehaviour
{
    [Header("필요한 아이템 리스트")]
    [SerializeField] private List<string> requiredItemNames = new List<string>();
    
    [Header("값 제한")]
    [SerializeField] private int maxTotalValue = 10;
    
    private Inventory inventory;

    void Start()
    {
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<Inventory>();
        }

        // 디버그
        if (inventory != null)
        {
            // 인벤 변경되면 매번 상태 로그
            inventory.OnInventoryChanged += LogQuestStatus;
        }

        // 디버그
        LogQuestStatus();
        
    }

    // 인벤토리 변경 시마다 퀘스트 상태 로그 갱신
    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= LogQuestStatus;
        }
    }


    // 디버그: 예산 + 용량 + 필요한 아이템 요약
    private void LogQuestStatus()
    {
        if (inventory == null)
        {
            Debug.Log("[Quest] Inventory not found.");
            return;
        }

        int currentTotalValue = GetCurrentTotalValue();
        int remainingMoney = maxTotalValue - currentTotalValue;

        int currentCapacity = inventory.GetCurrentCapacity();
        int maxCapacity = inventory.GetMaxCapacity();
        int remainingCapacity = inventory.GetRemainingCapacity();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[Quest] ===== 지갑 상황 =====");
        sb.AppendLine($"Money: {remainingMoney}/{maxTotalValue}");
        sb.AppendLine($"Capacity: {currentCapacity}/{maxCapacity} (remaining={remainingCapacity})");

        if (requiredItemNames != null && requiredItemNames.Count > 0)
        {
            // 인벤에 이미 있는 아이템 이름들
            List<string> inventoryItemNames = inventory.GetAllItems()
                .Select(i => i.gameObject.name)
                .ToList();

            sb.AppendLine("구매할 아이템 목록:");

            foreach (string itemName in requiredItemNames)
            {
                bool hasItem = inventoryItemNames.Contains(itemName);
                sb.AppendLine(hasItem ? $"[X] {itemName}" : $"[ ] {itemName}");
            }
        }

        Debug.Log(sb.ToString());
    }

    // 필요한 아이템을 모두 가지고 있는지 확인
    public bool HasAllRequiredItems()
    {
        if (inventory == null || requiredItemNames.Count == 0)
            return false;

        List<string> inventoryItemNames = inventory.GetAllItems()
            .Select(item => item.gameObject.name)
            .ToList();

        return requiredItemNames.All(required => 
            inventoryItemNames.Contains(required)
        );
    }

    // 값 제한을 지키고 있는지 확인
    public bool IsWithinValueLimit()
    {
         if (inventory == null)
            return false;

        int totalValue = 0;
        foreach (PickupableItem item in inventory.GetAllItems())
        {
            totalValue += item.GetItemValue();
        }

        return totalValue <= maxTotalValue;
    }

    // 모든 조건을 만족하는지 확인
    public bool IsQuestComplete()
    {
        return HasAllRequiredItems() && IsWithinValueLimit();
    }

    public int GetCurrentTotalValue()
    {
        if (inventory == null)
            return 0;

        int totalValue = 0;
        foreach (PickupableItem item in inventory.GetAllItems())
        {
            totalValue += item.GetItemValue();
        }
        return totalValue;
    }

    public int GetMaxTotalValue()
    {
        return maxTotalValue;
    }

    public List<string> GetRequiredItems()
    {
        return requiredItemNames;
    }

    public List<string> GetMissingItems()
    {
        if (inventory == null)
            return requiredItemNames;

        List<string> inventoryItemNames = inventory.GetAllItems()
            .Select(item => item.gameObject.name)
            .ToList();

        return requiredItemNames
            .Where(required => !inventoryItemNames.Contains(required))
            .ToList();
    }
}