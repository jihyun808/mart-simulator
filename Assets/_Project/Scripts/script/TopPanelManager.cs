using UnityEngine;
using TMPro;

/// <summary>
/// 상단 UI 관리 (타이머, 돈, 가방, 카트)
/// 타이머 종료 시 자동으로 GameOver 호출
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
    private int currentMoney = 50;
    private int currentBag = 0;
    private int currentCart = 0;
    private bool isTimeOver = false;

    private void Start()
    {
        currentTime = startTime;
        UpdateUI();
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

    // ========== Money 관련 ==========
    
    /// <summary>현재 소지금 반환</summary>
    public int GetCurrentBudget()
    {
        return currentMoney;
    }

    /// <summary>구매 가능 여부 확인</summary>
    public bool CanAfford(int cost)
    {
        return currentMoney >= cost;
    }

    /// <summary>돈 소비 (성공 시 true 반환)</summary>
    public bool Spend(int cost)
    {
        if (currentMoney >= cost)
        {
            currentMoney -= cost;
            UpdateMoneyUI();
            return true;
        }
        return false;
    }

    /// <summary>돈 추가</summary>
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }

    // ========== Bag & Cart 관련 ==========
    
    /// <summary>
    /// 가방 UI 업데이트 (인벤토리에서 호출)
    /// 현재 무게와 최대 무게를 함께 표시
    /// </summary>
    public void UpdateBagDisplay(int currentWeight, int maxWeight)
    {
        currentBag = currentWeight;
        maxBag = maxWeight;
        bagText.text = $"{currentBag}/{maxBag}";
    }

    /// <summary>가방 아이템 수 증가 (단순 증가용)</summary>
    public void AddToBag()
    {
        currentBag++;
        bagText.text = $"{currentBag}/{maxBag}";
    }

    /// <summary>카트 아이템 수 증가</summary>
    public void AddToCart()
    {
        currentCart++;
        bagText.text = $"{currentCart}/{maxCart}";
    }

    // ========== UI 업데이트 헬퍼 ==========
    
    private void UpdateUI()
    {
        timerText.text = startTime.ToString();
        UpdateMoneyUI();
        bagText.text = $"{currentBag}/{maxBag}";
        cartText.text = $"{currentCart}/{maxCart}";
    }

    private void UpdateMoneyUI()
    {
        moneyText.text = "$" + currentMoney;
    }
}