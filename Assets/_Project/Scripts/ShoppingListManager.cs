using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ShoppingItem
{
    public string itemName;
    public int requiredAmount;
    public int currentAmount;
    public bool IsComplete;
}

public class ShoppingListManager : MonoBehaviour
{
    [Header("Quest Settings")]
    public List<ShoppingItem> items; // Inspector에서 미션 설정

    [Header("References")]
    public Inventory playerInventory;

    private void Start()
    {
        if (playerInventory == null) 
            playerInventory = FindObjectOfType<Inventory>();
            
        UpdateCheckList();
    }

    /// <summary>
    /// 가방(Inventory) + 카트(Cart) 아이템 합산 체크
    /// </summary>
    public void UpdateCheckList()
    {
        // 1. 현재 활성화된(잡고 있거나 방치된) 카트 찾기
        // (플레이어 주변의 카트 혹은 잡고 있는 카트)
        CartInventory activeCart = FindActiveCart();

        foreach (var item in items)
        {
            // A. 가방에 있는 개수
            int bagCount = 0;
            if (playerInventory != null)
            {
                bagCount = CountItemInList(playerInventory.GetAllItems(), item.itemName);
            }

            // B. 카트에 있는 개수
            int cartCount = 0;
            if (activeCart != null)
            {
                // activeCart.items 리스트를 직접 가져오거나 GetAllItems() 사용
                cartCount = CountItemInList(activeCart.GetAllItems(), item.itemName);
            }

            // C. 합산
            item.currentAmount = bagCount + cartCount;
            item.IsComplete = item.currentAmount >= item.requiredAmount;
        }
    }

    // 씬에 있는 카트 중 '방치되지 않은(누군가 쓰고 있는)' 카트 찾기
    // 만약 "아무 카트나 다 인정"해주고 싶다면 로직을 바꿔도 됨
    private CartInventory FindActiveCart()
    {
        var carts = FindObjectsOfType<CartInventory>();
        foreach (var cart in carts)
        {
            // 현재 카트 코드는 '잡고 있지 않으면(Unmounted)' IsAbandoned가 true임.
            // 즉, !IsAbandoned는 '플레이어가 잡고 있는 카트'를 뜻함.
            // (만약 잡고 있지 않아도 체크되게 하고 싶다면 이 조건문을 지우고 첫 번째 카트를 리턴하세요)
            
            // 여기서는 "방금 물건을 넣은 그 카트"를 찾기 위해
            // 단순히 아이템이 들어있는 카트를 찾거나, 플레이어 가까운 걸 찾아야 함.
            
            // 가장 쉬운 방법: 그냥 아이템이 1개 이상 들어있는 카트가 있으면 그걸 내 카트로 인정
            if (cart.GetCurrentCount() > 0 || !cart.IsAbandoned)
            {
                return cart;
            }
        }
        return null;
    }

    private int CountItemInList(List<PickupableItem> itemList, string targetName)
    {
        int count = 0;
        if (itemList == null) return 0;

        foreach (var item in itemList)
        {
            if (item.itemName == targetName) count++;
        }
        return count;
    }
}