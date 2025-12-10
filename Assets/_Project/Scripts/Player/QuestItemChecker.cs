// QuestItemChecker.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class RequiredItemData
{
    public string itemName;    // 요구 아이템 이름
    public int requiredCount;  // 요구 개수
}

public class QuestItemChecker : MonoBehaviour
{
    [Header("Required Items (Name + Count)")]
    public List<RequiredItemData> requiredItems = new List<RequiredItemData>();

    [Header("Value Limit")]
    [SerializeField] private int maxTotalValue = 999; // 1스테이지는 금액제한 거의 없게

    private Inventory inventory;

    private void Start()
    {
        inventory = GetComponent<Inventory>();
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();
    }

    // 🔎 인벤토리에 특정 아이템이 몇 개 있는지 세기
    private int CountItem(string itemName)
    {
        return inventory.GetAllItems()
            .Count(item => item.itemName == itemName);
    }

    // 🔎 모든 아이템이 요구 개수를 만족하는지 확인
    public bool HasAllRequiredItems()
    {
        foreach (var req in requiredItems)
        {
            int count = CountItem(req.itemName);
            if (count < req.requiredCount)
            {
                Debug.Log($"❌ 부족한 아이템: {req.itemName} (필요:{req.requiredCount}, 보유:{count})");
                return false;
            }
        }
        return true;
    }

    // 기존 기능 유지 (가격 제한)
    public bool IsWithinValueLimit()
    {
        return GetCurrentTotalValue() <= maxTotalValue;
    }

    public bool IsQuestComplete()
    {
        return HasAllRequiredItems() && IsWithinValueLimit();
    }

    public int GetCurrentTotalValue()
    {
        return inventory.GetAllItems().Sum(item => item.GetItemValue());
    }

    public List<string> GetMissingItems()
    {
        List<string> missing = new List<string>();

        foreach (var req in requiredItems)
        {
            int count = CountItem(req.itemName);
            if (count < req.requiredCount)
                missing.Add($"{req.itemName} 부족 ({count}/{req.requiredCount})");
        }

        return missing;
    }
}
