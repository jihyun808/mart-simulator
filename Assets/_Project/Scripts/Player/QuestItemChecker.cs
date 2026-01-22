using UnityEngine;

/// <summary>
/// 스테이지 클리어 조건 체크 (필요 아이템, 예산 초과 여부)
/// HasAllRequiredItems()와 IsWithinValueLimit() 호출하여 검증
/// </summary>
public class QuestItemChecker : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;
    public Stage1Data stageData;
    public TopPanelManager topPanel;

    private void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<Inventory>();

        if (topPanel == null)
            topPanel = FindObjectOfType<TopPanelManager>();
    }

    /// <summary>스테이지 요구 아이템을 모두 가지고 있는지 확인</summary>
    public bool HasAllRequiredItems()
    {
        if (inventory == null || stageData == null) return false;

        foreach (var req in stageData.requirements)
        {
            int count = CountItemInInventory(req.itemName);

            if (count < req.requiredCount)
            {
                return false;
            }
        }

        return true;
    }

    private int CountItemInInventory(string itemName)
    {
        int count = 0;

        foreach (var item in inventory.GetAllItems())
        {
            if (item.itemName == itemName) count++;
        }

        return count;
    }

    /// <summary>인벤토리 총 가치가 예산 이하인지 확인</summary>
    public bool IsWithinValueLimit()
    {
        if (inventory == null || topPanel == null) return false;

        int currentTotal = CalculateInventoryValue();
        int maxValue = topPanel.GetCurrentBudget();

        return currentTotal <= maxValue;
    }

    private int CalculateInventoryValue()
    {
        int sum = 0;

        foreach (var item in inventory.GetAllItems())
        {
            sum += item.GetItemValue();
        }

        return sum;
    }
}