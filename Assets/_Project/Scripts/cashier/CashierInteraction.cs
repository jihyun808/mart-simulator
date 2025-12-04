// CashierInteraction.cs
using UnityEngine;

public class CashierInteraction : MonoBehaviour
{
    [Header("Quest Check")]
    [SerializeField] private QuestItemChecker questChecker;

    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private string cashierTag = "cashier";

    [Header("Events")]
    public System.Action OnQuestComplete;
    public System.Action OnItemsMissing;
    public System.Action OnValueExceeded;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (GameManager.GameIsPaused)
            return;

        if (Input.GetKeyDown(InputSettings.Keys[Action.Interact]))
        {
            TryInteractWithCashier();
        }
    }

    private void TryInteractWithCashier()
    {
        if (cam == null || questChecker == null)
            return;

        Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(centerRay, out RaycastHit hit, interactDistance))
        {
            if (hit.collider.CompareTag(cashierTag))
            {
                HandleCashierInteraction();
            }
        }
    }

    private void HandleCashierInteraction()
    {
        if (!questChecker.HasAllRequiredItems())
        {
            OnItemsMissing?.Invoke();
            return;
        }

        if (!questChecker.IsWithinValueLimit())
        {
            OnValueExceeded?.Invoke();
            return;
        }

        OnQuestComplete?.Invoke();
    }

    public bool CheckQuestStatus()
    {
        return questChecker != null && questChecker.IsQuestComplete();
    }
}