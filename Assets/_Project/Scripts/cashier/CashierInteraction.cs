using UnityEngine;
using System.Collections.Generic;

public class CashierInteraction : MonoBehaviour
{
    [Header("Quest Check")]
    [SerializeField] private QuestItemChecker questChecker;

    [Header("Wallet & Inventory")]
    public PlayerWallet playerWallet;      
    public Inventory playerInventory;      

    [Header("Events")]
    public System.Action OnQuestComplete;  
    public System.Action OnItemsMissing;   
    public System.Action OnValueExceeded;  
    public System.Action OnNotEnoughMoney; 

    private void Start()
    {
        OnQuestComplete += () => 
        {
            if (GameManager.Instance != null) GameManager.Instance.GameClear(1); 
        };
    }

    // ==========================================
    // 메인 상호작용 함수 (버튼이나 클릭으로 호출)
    // ==========================================
    public void TryCheckoutByClick()
    {
        if (questChecker == null || playerWallet == null || playerInventory == null)
        {
            Debug.LogError("❌ CashierInteraction: Inspector 연결 확인 필요!");
            return;
        }

        // 1. 퀘스트 조건 (아이템 부족?) - QuestItemChecker가 이미 카트까지 검사함
        if (!questChecker.HasAllRequiredItems())
        {
            Debug.Log("❌ 필요한 아이템 부족!");
            OnItemsMissing?.Invoke();
            return;
        }

        // 2. 예산 초과?
        if (!questChecker.IsWithinValueLimit())
        {
            Debug.Log("❌ 예산 초과!");
            OnValueExceeded?.Invoke();
            return;
        }

        // 3. 실제 가격 계산 (가방 + 카트)
        int totalPrice = CalculateTotalPrice();
        
        // 4. 결제 시도
        if (playerWallet.TrySpendMoney(totalPrice))
        {
            Debug.Log($"✅ 결제 성공! -${totalPrice}");
            
            // 구매 후 처리: 가방도 비우고, 카트도 비운다!
            ClearAllItems();
            
            OnQuestComplete?.Invoke();
        }
        else
        {
            Debug.Log($"❌ 잔액 부족! (필요: {totalPrice}, 보유: {playerWallet.currentMoney})");
            OnNotEnoughMoney?.Invoke(); 
        }
    }

    // ==========================================
    // 내부 계산 로직 (Private)
    // ==========================================

    private int CalculateTotalPrice()
    {
        int total = 0;

        // 1. 가방 가격 합산
        foreach (var item in playerInventory.GetAllItems())
        {
            total += item.GetItemValue(); 
        }

        // 2. 카트 가격 합산
        CartInventory activeCart = GetActiveCart();
        if (activeCart != null)
        {
            foreach (var item in activeCart.GetAllItems())
            {
                total += item.GetItemValue();
            }
        }

        return total;
    }

    // ⭐ [수정됨] 중복되지 않게 하나만 남긴 함수
    // 잡고 있거나(!IsAbandoned) 혹은 물건이 들어있는(Count > 0) 카트를 찾습니다.
    private CartInventory GetActiveCart()
    {
        var allCarts = FindObjectsOfType<CartInventory>();
        foreach (var cart in allCarts)
        {
            if (cart.GetCurrentCount() > 0 || !cart.IsAbandoned) 
            {
                return cart;
            }
        }
        return null;
    }

    private void ClearAllItems()
    {
        // 1. 가방 비우기
        var bagItems = playerInventory.GetAllItems();
        // 리스트를 복사해서 돌리는 것이 안전하므로 ToArray() 혹은 별도 리스트 처리 추천
        // 여기서는 간단히 처리 (Inventory 내부 구현에 따라 다를 수 있음)
        foreach(var item in new List<PickupableItem>(bagItems))
        {
            playerInventory.RemoveItem(item); 
            item.gameObject.SetActive(false); 
        }

        // 2. 카트 비우기
        CartInventory activeCart = GetActiveCart();
        if (activeCart != null)
        {
            activeCart.ClearCart(); 
        }
    }
}