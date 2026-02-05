using UnityEngine;

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
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameClear(1); 
            }
        };
    }

    public void TryCheckoutByClick()
    {
        // 1. 연결 확인
        if (questChecker == null || playerWallet == null || playerInventory == null)
        {
            Debug.LogError("❌ CashierInteraction: Inspector 연결을 확인해주세요!");
            return;
        }

        // 2. 퀘스트 조건 (아이템 개수)
        if (!questChecker.HasAllRequiredItems())
        {
            if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFXType.CashierReject);
            
            OnItemsMissing?.Invoke();
            return;
        }

        // 3. 퀘스트 조건 (예산)
        if (!questChecker.IsWithinValueLimit())
        {
            if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFXType.CashierReject);

            Debug.Log("❌ 퀘스트 예산 초과!");
            OnValueExceeded?.Invoke();
            return;
        }

        // 4. 결제 시도
        int totalPrice = CalculateTotalPrice(); 
        
        if (playerWallet.TrySpendMoney(totalPrice))
        {
            Debug.Log($"✅ 결제 성공! -${totalPrice}");
            ClearInventoryAfterPurchase();
            OnQuestComplete?.Invoke();
        }
        else
        {
            Debug.Log($"❌ 내 지갑 잔액 부족! (필요: {totalPrice}, 보유: {playerWallet.currentMoney})");
            OnNotEnoughMoney?.Invoke(); 
        }
    }

    // ⭐ 여기가 핵심 수정 부분입니다! ⭐
    private int CalculateTotalPrice()
    {
        int total = 0;
        foreach (var item in playerInventory.GetAllItems())
        {
            // 팀원 코드(PickupableItem)에 있는 함수를 사용하여 값을 가져옵니다.
            // item.price (X) -> item.GetItemValue() (O)
            total += item.GetItemValue(); 
        }
        return total;
    }

    private void ClearInventoryAfterPurchase()
    {
        var items = playerInventory.GetAllItems();
        foreach(var item in items)
        {
            playerInventory.RemoveItem(item); 
            item.gameObject.SetActive(false); 
        }
    }
}