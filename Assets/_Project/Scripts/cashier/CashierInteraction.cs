using UnityEngine;
using UnityEngine.Events;

public class CashierInteraction : MonoBehaviour
{
    [Header("Quest check")]
    [SerializeField] private QuestItemChecker questChecker;

    [Header("Interaction settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private string cashierTag = "cashier";

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteractWithCashier();
        }
    }

    private void TryInteractWithCashier()
    {
        if (cam == null || questChecker == null)
            return;

        Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(centerRay, out hit, interactDistance))
        {
            if (hit.collider.CompareTag(cashierTag))
            {
                HandleCashierInteraction();
            }
        }
    }

    private void HandleCashierInteraction()
    {
        // 1. 필요한 아이템을 모두 가지고 있는지 확인
        if (!questChecker.HasAllRequiredItems())
        {
            Debug.Log("아이템 부족");
            return;
        }

        // 2. 값 제한을 지키고 있는지 확인
        if (!questChecker.IsWithinValueLimit())
        {
            Debug.Log("값 초과");
            return;
        }

        // 모든 조건 만족 - 퀘스트 완료!
        Debug.Log("퀘스트 완료!");
    }

    public bool CheckQuestStatus()
    {
        return questChecker.IsQuestComplete();
    }
}