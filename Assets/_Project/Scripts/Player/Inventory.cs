using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxCapacity = 4;

    private PickupableItem[] slots = new PickupableItem[4]; 
    private int currentCapacity = 0; 

    [Header("References")]
    public InventoryUI inventoryUI;           
    public TopPanelManager topPanelManager;   
    public ShoppingListToggle shoppingListToggle; 
    
    [Header("Drop Settings")]
    public Transform dropPoint;

    // ⭐ 추가: 힌트 UI 참조
    private InteractionHintUI hintUI;

    private void Start()
    {
        if (inventoryUI == null) inventoryUI = FindObjectOfType<InventoryUI>();
        if (topPanelManager == null) topPanelManager = FindObjectOfType<TopPanelManager>();
        if (shoppingListToggle == null) shoppingListToggle = FindObjectOfType<ShoppingListToggle>();
        
        // ⭐ 힌트 UI 찾기
        hintUI = FindObjectOfType<InteractionHintUI>();

        UpdateUI();
    }

    public bool CheckCanAdd(int slotIndex, PickupableItem item) {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        if (slots[slotIndex] != null) return false; 
        if (currentCapacity + item.GetItemSize() > maxCapacity) return false; 
        return true;
    }

    public bool AddItem(PickupableItem item) {
        for (int i = 0; i < slots.Length; i++) {
            if (slots[i] == null) return TryAddItemToSlot(i, item); 
        }
        return false;
    }

    public List<PickupableItem> GetAllItems() {
        List<PickupableItem> activeItems = new List<PickupableItem>();
        foreach (var item in slots) { if (item != null) activeItems.Add(item); }
        return activeItems;
    }

    public void RemoveItem(PickupableItem item) {
        for (int i = 0; i < slots.Length; i++) {
            if (slots[i] == item) { DropItem(i, false); return; }
        }
    }

    public int GetItemCount() {
        int count = 0;
        foreach (var item in slots) { if (item != null) count++; }
        return count;
    }

    public PickupableItem GetItem(int index) {
        if (index >= 0 && index < slots.Length) return slots[index];
        return null;
    }

    public bool TryAddItemToSlot(int slotIndex, PickupableItem item)
    {
        if (!CheckCanAdd(slotIndex, item))
        {
            // ⭐ 인벤토리 꽉 찼을 때 힌트 표시
            if (currentCapacity + item.GetItemSize() > maxCapacity)
            {
                if (hintUI != null)
                {
                    hintUI.ShowInventoryFull();
                }
            }
            return false;
        }

        slots[slotIndex] = item;
        currentCapacity += item.GetItemSize();
        
        item.gameObject.SetActive(false); 

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFXType.ItemAdd);

        UpdateUI(); 
        Debug.Log($"{slotIndex + 1}번 슬롯에 저장 완료!");
        return true;
    }

    public PickupableItem DropItem(int slotIndex, bool dropToWorld = true)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;
        if (slots[slotIndex] == null) return null;

        PickupableItem item = slots[slotIndex];

        currentCapacity -= item.GetItemSize();
        slots[slotIndex] = null; 

        if (dropToWorld)
        {
            item.gameObject.SetActive(true);
            item.transform.SetParent(null); 

            if (dropPoint != null)
            {
                item.transform.position = dropPoint.position;
                item.transform.rotation = dropPoint.rotation;
            }
            else
            {
                item.transform.position = transform.position + transform.forward * 1.5f + Vector3.up * 1.0f;
            }

            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFXType.ItemRemove); 
        
        UpdateUI();
        return item;
    }

    private void UpdateUI()
    {
        if (inventoryUI != null) inventoryUI.UpdateUI(slots);
        if (topPanelManager != null) topPanelManager.UpdateBagDisplay(currentCapacity, maxCapacity);

        if (shoppingListToggle != null)
        {
            shoppingListToggle.RefreshUI();
        }
    }

    public int GetCurrentCapacity() => currentCapacity;
    public int GetMaxCapacity() => maxCapacity;
}