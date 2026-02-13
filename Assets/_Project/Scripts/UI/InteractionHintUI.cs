using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어가 바라보는 오브젝트에 따라 상호작용 힌트 표시
/// </summary>
public class InteractionHintUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hintText;
    
    [Header("Raycast Settings")]
    [SerializeField] private float raycastDistance = 3f;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private LayerMask cashierLayer;
    [SerializeField] private LayerMask cartLayer;
    
    [Header("References")]
    [SerializeField] private PlayerPickupController pickupController;
    [SerializeField] private Inventory playerInventory;
    
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        
        if (hintText != null)
        {
            hintText.text = "";
        }
    }

    private void Update()
    {
        if (GameManager.GameIsPaused || GameManager.IsGameOver || GameManager.IsGameClear)
        {
            SetHintText("");
            return;
        }

        UpdateHint();
    }

    private void UpdateHint()
    {
        Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        
        // 1. PickupableItem 체크
        if (Physics.Raycast(centerRay, out RaycastHit pickupHit, raycastDistance, pickupLayer))
        {
            PickupableItem item = pickupHit.collider.GetComponent<PickupableItem>();
            
            if (item != null && !item.IsCarried())
            {
                // 손에 아무것도 없을 때
                if (pickupController.GetCurrentItem() == null)
                {
                    SetHintText("우클릭으로 들기");
                    return;
                }
            }
        }
        
        // 2. 손에 물건 들고 있을 때
        if (pickupController.GetCurrentItem() != null)
        {
            SetHintText("우클릭: 내려놓기 | 좌클릭 짧게: 흔들기 | 좌클릭 길게: 던지기");
            return;
        }
        
        // 3. Cashier 체크
        if (Physics.Raycast(centerRay, out RaycastHit cashierHit, raycastDistance, cashierLayer))
        {
            CashierInteraction cashier = cashierHit.collider.GetComponent<CashierInteraction>();
            
            if (cashier != null)
            {
                SetHintText("좌클릭해 계산하기");
                return;
            }
        }
        
        // 4. Cart 체크
        if (Physics.Raycast(centerRay, out RaycastHit cartHit, raycastDistance, cartLayer))
        {
            CartMount cart = cartHit.collider.GetComponentInParent<CartMount>();
            
            if (cart != null)
            {
                SetHintText("E키로 장착하기");
                return;
            }
        }
        
        // 아무것도 안 가리킴
        SetHintText("");
    }

    /// <summary>
    /// 인벤토리가 꽉 찼을 때 외부에서 호출 (Inventory 스크립트에서)
    /// </summary>
    public void ShowInventoryFull()
    {
        SetHintText("인벤토리 꽉 찼!");
        Invoke(nameof(ClearHint), 2f); // 2초 후 자동으로 사라짐
    }

    private void ClearHint()
    {
        SetHintText("");
    }

    private void SetHintText(string text)
    {
        if (hintText != null)
        {
            hintText.text = text;
        }
    }
}