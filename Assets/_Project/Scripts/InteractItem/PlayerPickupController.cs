// PlayerPickupController.cs
using UnityEngine;

public class PlayerPickupController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform hand;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer;
    
    private PickupableItem currentItem = null;
    private Camera cam;
    private Inventory inventory;
    private InputHandler inputHandler;

    private void Start()
    {
        cam = Camera.main;
        inventory = GetComponent<Inventory>();
        inputHandler = GetComponent<InputHandler>();
        
        // ✅ 추천: 런타임 AddComponent 제거, 에러 로그로 변경
        if (inventory == null)
        {
            Debug.LogError("❌ Player에 Inventory 컴포넌트가 없습니다! (Inspector에서 추가하세요)");
        }
        
        if (inputHandler == null)
        {
            Debug.LogError("❌ Player에 InputHandler 컴포넌트가 없습니다!");
        }
    }

    private void Update()
    {
        if (GameManager.GameIsPaused)
            return;

        HandleInventoryInput();
        HandlePickupInput();
    }

    public void ForcePickUp(PickupableItem item)
{
    if (item == null) return;

    // 이미 들고 있던 아이템이 있으면 정리
    if (currentItem != null)
    {
        currentItem.Drop();
        currentItem = null;
    }

    // ✅ 핵심: PlayerPickupController의 상태를 갱신
    currentItem = item;

    // ✅ 손에 장착
    item.PickUp(hand);

    Debug.Log($"[ForcePickUp] {item.name} picked up via Cart");
}

    private void HandleInventoryInput()
    {
        // ✅ 1단계: E키 입력 확인 로그
        if (inputHandler != null && inputHandler.IsInteractPressed())
        {
            Debug.Log("✅ Interact(E) pressed!");
        }
        
        // ✅ 2단계: 인벤토리 추가 시도
        if (inputHandler != null && inputHandler.IsInteractPressed() && currentItem != null)
        {
            AddToInventory();
        }
    }

    private void HandlePickupInput()
    {
        if (inputHandler != null && inputHandler.IsGrabPressed())
        {
            if (currentItem == null)
                TryPickup();
            else
                DropItem();
        }
    }

    private void TryPickup()
    {
        Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(centerRay, out RaycastHit hit, pickupRange, pickupLayer))
        {
            PickupableItem item = hit.collider.GetComponent<PickupableItem>();
            if (item != null && !item.IsCarried())
            {
                currentItem = item;
                currentItem.PickUp(hand);
                Debug.Log($"📦 아이템 집음: {currentItem.itemName}");
            }
        }
    }

    private void DropItem()
    {
        if (currentItem != null)
        {
            Debug.Log($"📦 아이템 놓음: {currentItem.itemName}");
            currentItem.Drop();
            currentItem = null;
        }
    }

    private void AddToInventory()
    {
        if (currentItem != null && inventory != null)
        {
            // ✅ AddItem 성공 여부 확인 로그
            bool ok = inventory.AddItem(currentItem);
            Debug.Log($"🧺 AddItem 결과={ok}, item={currentItem.itemName}");
            
            if (ok)
            {
                currentItem = null;
            }
            else
            {
                Debug.LogWarning($"⚠️ 인벤토리에 {currentItem.itemName} 추가 실패!");
            }
        }
        else
        {
            if (currentItem == null)
                Debug.LogWarning("⚠️ currentItem이 null입니다!");
            if (inventory == null)
                Debug.LogWarning("⚠️ inventory가 null입니다!");
        }
    }

    public PickupableItem GetCurrentItem()
    {
        return currentItem;
    }

    public void ClearCurrentItem()
    {
        currentItem = null;
    }
}