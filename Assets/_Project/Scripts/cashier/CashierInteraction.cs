using UnityEngine;

public class CashierInteraction : MonoBehaviour
{
    [Header("Quest Check")]
    [SerializeField] private QuestItemChecker questChecker;

    [Header("Events")]
    public System.Action OnQuestComplete;
    public System.Action OnItemsMissing;
    public System.Action OnValueExceeded;

    public void TryCheckoutByClick()
    {
        Debug.Log("🧾 계산 시도 (클릭)");

        if (questChecker == null)
        {
            Debug.LogError("❌ QuestItemChecker 연결 안됨!");
            return;
        }

        // 필요한 아이템 부족
        if (!questChecker.HasAllRequiredItems())
        {
            Debug.Log("❌ 필요한 아이템 부족!");
            OnItemsMissing?.Invoke();
            return;
        }

        // 예산 초과
        if (!questChecker.IsWithinValueLimit())
        {
            Debug.Log("❌ 예산 초과!");
            OnValueExceeded?.Invoke();
            return;
        }

        // 성공
        Debug.Log("✅ 계산 성공! 스테이지 클리어 가능!");
        OnQuestComplete?.Invoke();
    }
}
