using UnityEngine;
using System.Collections.Generic;

public class QuestItemChecker : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;
    
    // ⭐ [수정 1] Stage1Data 삭제 -> ShoppingListManager 추가
    // public Stage1Data stageData; (삭제됨)
    public ShoppingListManager listManager; 

    public PlayerWallet playerWallet;

    private void Awake()
    {
        if (inventory == null) inventory = GetComponent<Inventory>();
        if (playerWallet == null) playerWallet = FindObjectOfType<PlayerWallet>();
        
        // 매니저가 연결 안 되어 있으면 자동으로 찾기
        if (listManager == null) listManager = FindObjectOfType<ShoppingListManager>();
    }

    // 물건이 들어있는 카트 찾기
    private CartInventory GetActiveCart()
    {
        var allCarts = FindObjectsOfType<CartInventory>();
        foreach (var cart in allCarts)
        {
            if (cart.GetCurrentCount() > 0) return cart;
        }
        return null; 
    }

    // ⭐ [핵심 수정] StageData가 아니라, 리스트 매니저의 내용을 검사함
    public bool HasAllRequiredItems()
    {
        if (listManager == null)
        {
            Debug.LogError("❌ ShoppingListManager가 연결되지 않았습니다!");
            return false;
        }

        // 1. 검사 전, 최신 상태로 업데이트 (가방+카트 확인)
        listManager.UpdateCheckList();

        // 2. 리스트 매니저의 모든 아이템이 '완료(IsComplete)' 상태인지 확인
        foreach (var item in listManager.items)
        {
            if (!item.IsComplete)
            {
                Debug.Log($"❌ 결제 거부: {item.itemName} 부족함 ({item.currentAmount}/{item.requiredAmount})");
                return false;
            }
        }

        Debug.Log("✅ 모든 리스트 조건 만족!");
        return true;
    }

    public bool IsWithinValueLimit()
    {
        if (inventory == null || playerWallet == null) return false;

        int totalValue = CalculateListValue(inventory.GetAllItems());

        CartInventory activeCart = GetActiveCart();
        if (activeCart != null)
        {
            totalValue += CalculateListValue(activeCart.GetAllItems());
        }

        int myMoney = playerWallet.currentMoney; 
        return totalValue <= myMoney;
    }

    private int CalculateListValue(List<PickupableItem> items)
    {
        int sum = 0;
        foreach (var item in items)
        {
            sum += item.GetItemValue(); 
        }
        return sum;
    }
}