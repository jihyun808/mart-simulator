using UnityEngine;

/// <summary>
/// 스테이지 클리어 조건 체크 (필요 아이템, 예산 초과 여부)
/// 수정됨: TopPanelManager 대신 PlayerWallet의 돈을 확인하도록 변경
/// </summary>
public class QuestItemChecker : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;
    public Stage1Data stageData;
    
    // ⭐ [수정 1] TopPanelManager 삭제 -> PlayerWallet 추가
    public PlayerWallet playerWallet;

    private void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<Inventory>();

        // ⭐ [수정 2] 플레이어의 지갑 스크립트를 찾아서 연결
        if (playerWallet == null)
            playerWallet = FindObjectOfType<PlayerWallet>();
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

    /// <summary>인벤토리 총 가치가 현재 가진 돈(예산) 이하인지 확인</summary>
    public bool IsWithinValueLimit()
    {
        // ⭐ [수정 3] TopPanel 대신 playerWallet 확인
        if (inventory == null || playerWallet == null) return false;

        int currentTotal = CalculateInventoryValue();
        
        // 내 지갑에 있는 돈보다 많이 담았는지 확인
        int myMoney = playerWallet.currentMoney; 

        return currentTotal <= myMoney;
    }

    private int CalculateInventoryValue()
    {
        int sum = 0;

        foreach (var item in inventory.GetAllItems())
        {
            // 팀원 코드의 GetItemValue() 사용
            sum += item.GetItemValue();
        }

        return sum;
    }
}