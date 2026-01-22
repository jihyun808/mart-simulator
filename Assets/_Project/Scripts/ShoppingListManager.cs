using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 쇼핑 목록 아이템 (아이템 이름 + 필요 개수)
/// currentAmount는 게임 중 자동 계산됨
/// </summary>
[Serializable]
public class ShoppingItem
{
    public string itemName;
    public int requiredAmount;
    
    [HideInInspector] 
    public int currentAmount;

    public bool IsComplete => currentAmount >= requiredAmount;
}

/// <summary>
/// 쇼핑 리스트 관리 (목표 아이템 추적)
/// 인벤토리 변경 시 UpdateCheckList() 호출 필요
/// </summary>
public class ShoppingListManager : MonoBehaviour
{
    [Header("Shopping List Items")]
    public List<ShoppingItem> items = new List<ShoppingItem>();

    [Header("References")]
    public Inventory playerInventory;

    /// <summary>
    /// 인벤토리 변경 시 호출 - 현재 수집한 아이템 개수 재계산
    /// </summary>
    public void UpdateCheckList()
    {
        ResetCounts();
        
        if (playerInventory != null)
        {
            foreach (var invItem in playerInventory.GetAllItems())
            {
                CountItem(invItem.itemName);
            }
        }
    }

    private void ResetCounts()
    {
        foreach (var item in items)
        {
            item.currentAmount = 0;
        }
    }

    private void CountItem(string targetName)
    {
        foreach (var shopItem in items)
        {
            if (shopItem.itemName.Trim() == targetName.Trim())
            {
                shopItem.currentAmount++;
            }
        }
    }
}