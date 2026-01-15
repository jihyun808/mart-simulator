using UnityEngine;

public class CashierInteraction : MonoBehaviour
{
    [Header("Quest Check")]
    [SerializeField] private QuestItemChecker questChecker;

    [Header("Clear UI")]
    [SerializeField] private ClearUIManager clearUI;

    [Header("Events")]
    public System.Action OnQuestComplete;
    public System.Action OnItemsMissing;
    public System.Action OnValueExceeded;

    private void Start()
    {
        if (clearUI != null)
        {
            OnQuestComplete += () => 
            {
                clearUI.ShowClearUI(1);
                
                // ✅ 게임 클리어 처리 (커서 풀고, 시간 멈추기)
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameClear();
                }
            };
        }
        else
        {
            Debug.LogWarning("⚠️ clearUI가 연결 안 됨! (CashierInteraction 인스펙터에서 연결하세요)");
        }
    }

    public void TryCheckoutByClick()
    {
        Debug.Log("🧾 계산 시도 (클릭)");
        Debug.Log($"[Cashier] questChecker={questChecker?.gameObject.name}");

        if (questChecker == null)
        {
            Debug.LogError("❌ QuestItemChecker 연결 안됨!");
            return;
        }

        if (!questChecker.HasAllRequiredItems())
        {
            Debug.Log("❌ 필요한 아이템 부족!");
            OnItemsMissing?.Invoke();
            return;
        }

        if (!questChecker.IsWithinValueLimit())
        {
            Debug.Log("❌ 예산 초과!");
            OnValueExceeded?.Invoke();
            return;
        }

        Debug.Log("✅ 계산 성공! 스테이지 클리어 가능!");
        OnQuestComplete?.Invoke();
    }
}