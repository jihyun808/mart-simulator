using UnityEngine;

public class CashierTriggerZone : MonoBehaviour
{
    [SerializeField] private QuestItemChecker questChecker;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player entered cashier zone");

        if (questChecker == null)
        {
            Debug.LogError("QuestItemChecker is NOT assigned!");
            return;
        }

        if (!questChecker.HasAllRequiredItems())
        {
            Debug.Log("❌ 필요한 물건이 부족합니다!");
            return;
        }

        if (!questChecker.IsWithinValueLimit())
        {
            Debug.Log("❌ 예산 초과!");
            return;
        }

        Debug.Log("✅ 모든 조건 충족! 스테이지 클리어!");
    }
}
