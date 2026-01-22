using UnityEngine;

public class CashierInteraction : MonoBehaviour
{
    [Header("Quest Check")]
    [SerializeField] private QuestItemChecker questChecker;

    [Header("Events")]
    public System.Action OnQuestComplete;
    public System.Action OnItemsMissing;
    public System.Action OnValueExceeded;

    private void Start()
    {
        // GameManager만 호출 (ClearUIManager는 사용 안 함)
        OnQuestComplete += () => 
        {
            Debug.Log("⭐⭐⭐ OnQuestComplete 실행됨!");
            
            if (GameManager.Instance != null)
            {
                Debug.Log("⭐ GameManager.GameClear() 호출 시작");
                GameManager.Instance.GameClear(1); // 스테이지 1
                Debug.Log("⭐ GameManager.GameClear() 호출 완료");
                Debug.Log($"⭐ Time.timeScale 확인: {Time.timeScale}");
            }
            else
            {
                Debug.LogError("❌ GameManager.Instance가 NULL!");
            }
        };
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