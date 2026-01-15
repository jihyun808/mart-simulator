using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShoppingItem
{
    public string itemName;      // 아이템 이름 (예: "와인잔") - 뒤에 x2 붙이지 마세요!
    public int requiredAmount;   // 목표 개수 (예: 2)
    
    [HideInInspector] 
    public int currentAmount;    // 현재 먹은 개수 (게임 중 자동 계산됨)

    public bool IsComplete => currentAmount >= requiredAmount; // 다 모았는지 확인
}

public class ShoppingListManager : MonoBehaviour
{
    [Header("Shopping List")]
    public List<ShoppingItem> items = new List<ShoppingItem>();

    [Header("References")]
    public Inventory playerInventory; // 플레이어 인벤토리 연결

    // 인벤토리가 변할 때마다 호출되어 개수를 다시 셉니다.
    public void UpdateCheckList()
    {
        // 1. 개수 초기화
        foreach (var item in items)
        {
            item.currentAmount = 0;
        }

        // 2. 플레이어 인벤토리 뒤져서 개수 세기
        if (playerInventory != null)
        {
            foreach (var invItem in playerInventory.GetAllItems())
            {
                CountItem(invItem.itemName);
            }
        }
    }

    private void CountItem(string targetName)
    {
        foreach (var shopItem in items)
        {
            // 공백 제거하고 이름 비교 (실수 방지)
            if (shopItem.itemName.Trim() == targetName.Trim())
            {
                shopItem.currentAmount++;
            }
        }
    }
}