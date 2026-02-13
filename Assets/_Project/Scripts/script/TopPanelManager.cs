using UnityEngine;
using TMPro;

public class TopPanelManager : MonoBehaviour
{
    [Header("UI Text")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI bagText;
    public TextMeshProUGUI cartText;

    [Header("Settings")]
    public int startTime = 150;
    public int maxBag = 4;
    // public int maxCart = 30; // 삭제 (개별 카트마다 용량이 다를 수 있으므로)

    private float currentTime;
    private bool isTimeOver = false;

    private void Start()
    {
        currentTime = startTime;
        timerText.text = startTime.ToString();
        
        // 초기화
        bagText.text = $"0/{maxBag}";
        cartText.text = "0/30"; // 기본값
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
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }
    }

    public void UpdateMoneyDisplay(int amount)
    {
        if (moneyText != null) moneyText.text = "$" + amount;
    }

    public void UpdateBagDisplay(int currentWeight, int maxWeight)
    {
        bagText.text = $"{currentWeight}/{maxWeight}";
    }

    // ⭐ [수정됨] 카트 용량 업데이트 함수 (단순 증가 X -> 실제 값 표시 O)
    public void UpdateCartDisplay(int currentCount, int maxCapacity)
    {
        if (cartText != null)
        {
            cartText.text = $"{currentCount}/{maxCapacity}";
        }
    }
}