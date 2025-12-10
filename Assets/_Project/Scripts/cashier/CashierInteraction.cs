using UnityEngine;

public class CashierInteraction : MonoBehaviour
{
    [Header("Quest Check")]
    [SerializeField] private QuestItemChecker questChecker;

    [Header("Trigger Tag Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("Events")]
    public System.Action OnQuestComplete;
    public System.Action OnItemsMissing;
    public System.Action OnValueExceeded;

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 캐셔 영역에 들어왔을 때만 작동
        if (!other.CompareTag(playerTag))
            return;

        HandleCashierInteraction();
    }

    private void HandleCashierInteraction()
    {
        if (questChecker == null)
        {
            Debug.LogError("❌ QuestItemChecker가 캐릭터에 연결되지 않음!");
            return;
        }

        // 필요한 아이템 체크
        if (!questChecker.HasAllRequiredItems())
        {
            Debug.Log("❌ 필요한 아이템이 부족합니다!");
            OnItemsMissing?.Invoke();
            return;
        }

        // 예산 초과 체크
        if (!questChecker.IsWithinValueLimit())
        {
            Debug.Log("❌ 예산을 초과했습니다!");
            OnValueExceeded?.Invoke();
            return;
        }

        // 모든 조건 충족
        Debug.Log("✅ 계산 성공! 스테이지 클리어 가능!");
        OnQuestComplete?.Invoke();
    }
}
