using UnityEngine;
using TMPro;

/// <summary>
/// 상단 UI 관리 (타이머, 돈, 가방, 카트)
/// 수정사항: 돈 계산 로직 삭제(PlayerWallet으로 이관), UI 갱신 기능만 남김
/// </summary>
public class TopPanelManager : MonoBehaviour
{
    [Header("UI Text - Inspector에서 연결")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI bagText;
    public TextMeshProUGUI cartText;

    [Header("Game Settings")]
    public int startTime = 150;
    public int maxBag = 4;
    public int maxCart = 30;

    private float currentTime;
    // private int currentMoney = 50; // 삭제됨 (PlayerWallet이 관리함)
    private int currentBag = 0;
    private int currentCart = 0;
    private bool isTimeOver = false;

    private void Start()
    {
        currentTime = startTime;
        
        // 초기화 (돈은 PlayerWallet이 시작할 때 UpdateMoneyDisplay를 호출해줄 것임)
        timerText.text = startTime.ToString();
        bagText.text = $"0/{maxBag}";
        cartText.text = $"0/{maxCart}";
    }

    private void Update()
    {
        HandleTimer();
    }

    private void HandleTimer()
    {
        if (isTimeOver) return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
        else
        {
            currentTime = 0;
            timerText.text = "0";
            isTimeOver = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    // ========== Money 관련 (수정됨) ==========
    
    // 돈 계산 로직(Spend, AddMoney 등)은 모두 PlayerWallet.cs로 이동했습니다.
    // 여기서는 오직 텍스트만 바꿔줍니다.

    /// <summary>돈 UI 갱신 (PlayerWallet에서 호출)</summary>
    public void UpdateMoneyDisplay(int amount)
    {
        if (moneyText != null)
        {
            moneyText.text = "$" + amount;
        }
    }

    // ========== Bag & Cart 관련 ==========
    
    /// <summary>
    /// 가방 UI 업데이트 (인벤토리에서 호출)
    /// </summary>
    public void UpdateBagDisplay(int currentWeight, int maxWeight)
    {
        currentBag = currentWeight;
        maxBag = maxWeight;
        bagText.text = $"{currentBag}/{maxBag}";
    }

    /// <summary>카트 아이템 수 증가</summary>
    public void AddToCart()
    {
        currentCart++;
        // [버그 수정] 원래 코드에 bagText로 되어 있어서 cartText로 고쳤습니다.
        cartText.text = $"{currentCart}/{maxCart}"; 
    }
}