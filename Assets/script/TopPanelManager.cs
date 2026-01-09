using UnityEngine;
using TMPro;

public class TopPanelManager : MonoBehaviour
{
    [Header("UI Text")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI bagText;
    public TextMeshProUGUI cartText;

    [Header("Game Settings")]
    public int startTime = 150;   // 시작 시간(초)
    public int maxBag = 4;        // 가방 최대 용량
    public int maxCart = 30;      // 카트 최대 용량

    private float currentTime;
    private int currentMoney = 50;
    private int currentBag = 0;
    private int currentCart = 0;
    
    // 게임오버 중복 호출 방지
    private bool isTimeOver = false;

    void Start()
    {
        currentTime = startTime;
        UpdateUI();
    }

    void Update()
    {
        HandleTimer();
    }

    private void HandleTimer()
    {
        // 이미 게임오버 처리됐으면 더 이상 실행 안 함
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

            // ✅ 게임오버 호출
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    // ------------------------------
    //   💰 Money 관련 함수들
    // ------------------------------

    public int GetCurrentBudget()
    {
        return currentMoney;
    }

    public bool CanAfford(int cost)
    {
        return currentMoney >= cost;
    }

    public bool Spend(int cost)
    {
        if (currentMoney >= cost)
        {
            currentMoney -= cost;
            moneyText.text = "$" + currentMoney;
            return true;
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        moneyText.text = "$" + currentMoney;
    }

    // ------------------------------
    //   🎒 Bag & Cart 관련 함수들
    // ------------------------------

    public void AddToBag()
    {
        currentBag++;
        bagText.text = currentBag + "/" + maxBag;
    }

    public void AddToCart()
    {
        currentCart++;
        cartText.text = currentCart + "/" + maxCart;
    }

    // ------------------------------
    //   🎛 UI 초기 업데이트
    // ------------------------------

    private void UpdateUI()
    {
        timerText.text = startTime.ToString();
        moneyText.text = "$" + currentMoney;
        bagText.text = currentBag + "/" + maxBag;
        cartText.text = currentCart + "/" + maxCart;
    }
}