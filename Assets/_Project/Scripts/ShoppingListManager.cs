using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShoppingItem
{
    public string itemName;   // 품목 이름 (예: 우유, 빵)
    public bool purchased;    // 샀는지 여부
}

public class ShoppingListManager : MonoBehaviour
{
    public List<ShoppingItem> items = new List<ShoppingItem>();

    // 아이템을 샀다고 표시하는 함수 (나중에 "아이템 집기"랑 연결)
    public void MarkPurchased(string itemName)
    {
        ShoppingItem item = items.Find(i => i.itemName == itemName);
        if (item != null)
        {
            item.purchased = true;
        }
        else
        {
            Debug.LogWarning($"ShoppingListManager: '{itemName}' 항목을 찾을 수 없습니다.");
        }
    }
}
