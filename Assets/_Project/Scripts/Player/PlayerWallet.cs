using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [Header("Money Settings")]
    public int currentMoney = 50; // 기본 소지금 50달러

    [Header("References")]
    public TopPanelManager topPanelManager; // 상단 UI 연결

    private void Start()
    {
        // 게임 시작 시 UI 갱신
        UpdateUI();
    }

    // 돈을 사용할 때 호출 (성공하면 true, 부족하면 false 반환)
    public bool TrySpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            return true;
        }
        else
        {
            Debug.Log("잔액이 부족합니다!");
            return false;
        }
    }

    // 돈을 획득할 때 호출
    public void EarnMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (topPanelManager != null)
        {
            topPanelManager.UpdateMoneyDisplay(currentMoney);
        }
    }
}