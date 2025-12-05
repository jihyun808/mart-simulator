// QuestItemChecker.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestItemChecker : MonoBehaviour
{
    [Header("Required Items")]
    [SerializeField] private List<string> requiredItemNames = new List<string>();
    
    [Header("Value Limit")]
    [SerializeField] private int maxTotalValue = 10;
    
    private Inventory inventory;

    private void Start()
    {
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<Inventory>();
        }
    }

    public bool HasAllRequiredItems()
    {
        if (inventory == null || requiredItemNames.Count == 0)
            return false;

        HashSet<string> inventoryItemNames = new HashSet<string>(
            inventory.GetAllItems().Select(item => item.gameObject.name)
        );

        return requiredItemNames.All(required => inventoryItemNames.Contains(required));
    }

    public bool IsWithinValueLimit()
    {
        if (inventory == null)
            return false;

        return GetCurrentTotalValue() <= maxTotalValue;
    }

    public bool IsQuestComplete()
    {
        return HasAllRequiredItems() && IsWithinValueLimit();
    }

    public int GetCurrentTotalValue()
    {
        if (inventory == null)
            return 0;

        return inventory.GetAllItems().Sum(item => item.GetItemValue());
    }

    public int GetMaxTotalValue()
    {
        return maxTotalValue;
    }

    public List<string> GetRequiredItems()
    {
        return new List<string>(requiredItemNames);
    }

    public List<string> GetMissingItems()
    {
        if (inventory == null)
            return new List<string>(requiredItemNames);

        HashSet<string> inventoryItemNames = new HashSet<string>(
            inventory.GetAllItems().Select(item => item.gameObject.name)
        );

        return requiredItemNames.Where(required => !inventoryItemNames.Contains(required)).ToList();
    }

    public int GetRemainingValue()
    {
        return maxTotalValue - GetCurrentTotalValue();
    }
}