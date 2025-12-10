using UnityEngine;

public class QuestItemChecker : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;          // 플레이어 인벤토리
    public Stage1Data stageData;         // 현재 스테이지 요구사항
    public TopPanelManager topPanel;     // 돈(예산) UI 및 실제 값 관리하는 스크립트

    private void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<Inventory>();

        if (topPanel == null)
            topPanel = FindObjectOfType<TopPanelManager>();
    }

    // -------------------------------------------------------------
    // 1) 스테이지 요구 아이템 체크
    // -------------------------------------------------------------
    public bool HasAllRequiredItems()
    {
        if (inventory == null || stageData == null)
            return false;

        foreach (var req in stageData.requirements)
        {
            int count = CountItemInInventory(req.itemName);

            if (count < req.requiredCount)
            {
                Debug.Log($"❌ 부족한 아이템: {req.itemName} (필요 {req.requiredCount}개, 현재 {count}개)");
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
            if (item.itemName == itemName)
                count++;
        }

        return count;
    }

    // -------------------------------------------------------------
    // 2) 금액(예산) 체크
    // -------------------------------------------------------------
    public bool IsWithinValueLimit()
    {
        if (inventory == null || topPanel == null)
            return false;

        int currentTotal = CalculateInventoryValue();
        int maxValue = topPanel.GetCurrentBudget();  // 팀원 UI에 있던 예산 가져오기

        Debug.Log($"💰 현재 금액: {currentTotal}, 제한 금액: {maxValue}");

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
