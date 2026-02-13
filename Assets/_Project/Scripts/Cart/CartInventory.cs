using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class CartInventory : MonoBehaviour
{
    [Header("Settings")]
    public int maxCapacity = 30; // 0인지 꼭 확인하세요!
    
    [Header("Status (Read Only)")]
    public int currentLoad = 0;  
    public int itemCount = 0;    

    public int StoredCount => itemCount;
    public bool IsAbandoned => cartMount != null && !cartMount.IsMounted;

    [Header("References")]
    public Transform itemContainer;

    [Space]
    [Header("Events")]
    public UnityEvent onCartUpdated; // 쇼핑리스트 갱신용

    private List<PickupableItem> items = new List<PickupableItem>();
    private CartMount cartMount;

    private void Awake()
    {
        cartMount = GetComponentInParent<CartMount>();
        if (itemContainer == null) itemContainer = transform;
    }

    private void Start()
    {
        currentLoad = 0;
        itemCount = 0;
    }

    // 손을 넣고 있다가 놓는 순간 감지
    private void OnTriggerStay(Collider other)
    {
        PickupableItem item = other.GetComponent<PickupableItem>();
        if (item != null && !items.Contains(item))
        {
            if (item.IsCarried()) return; // 아직 잡고 있으면 대기
            AddItem(item);
        }
    }

    public void AddItem(PickupableItem item)
    {
        int itemSize = item.GetItemSize();

        if (currentLoad + itemSize > maxCapacity) return;
        if (items.Contains(item)) return;

        items.Add(item);
        currentLoad += itemSize;
        itemCount++;             
        
        item.transform.SetParent(itemContainer);

        // ⭐ 1. 넣을 때는 숨깁니다 (물리 충돌 방지 & 깔끔함)
        item.gameObject.SetActive(false); 

        Debug.Log($"[Cart] {item.name}(크기:{itemSize}) 담김! 용량: {currentLoad}/{maxCapacity}");
        
        UpdateUI();
        onCartUpdated?.Invoke(); 
    }

    public void RemoveItem(PickupableItem item)
    {
        if (items.Contains(item))
        {
            int itemSize = item.GetItemSize();

            items.Remove(item);
            currentLoad -= itemSize; 
            itemCount--;             
            if (currentLoad < 0) currentLoad = 0;

            UpdateUI();
            onCartUpdated?.Invoke();
        }
    }

    // ⭐ 2. 꺼낼 때 로직 (여기가 중요!)
    public void TryTakeOutToHand(Transform hand)
    {
        if (items.Count == 0) return;
        
        // 마지막에 넣은 물건 (LIFO)
        PickupableItem itemToTake = items[items.Count - 1];
        
        // A. 먼저 보이게 켭니다.
        itemToTake.gameObject.SetActive(true);
        
        // B. 카트에서 분리하고 손 위치로 이동
        itemToTake.transform.SetParent(null);
        itemToTake.transform.position = hand.position;
        itemToTake.transform.rotation = hand.rotation;

        // C. 리스트에서 데이터 삭제
        RemoveItem(itemToTake);
        
        // D. 물리 상태 복구 (숨겨져 있을 때 꺼졌던 물리를 다시 켬)
        Rigidbody rb = itemToTake.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero; // 튀어나감 방지
        }

        // E. 플레이어 손에 쥐어주기 시도
        var pickupController = hand.GetComponentInParent<PlayerPickupController>();
        if (pickupController != null) 
        {
            pickupController.ForcePickUp(itemToTake);
        }
        else 
        {
            // 컨트롤러 없으면 그냥 손 위치에서 PickUp 실행
            itemToTake.PickUp(hand);
        }
        
        Debug.Log($"[Cart] {itemToTake.name} 꺼냄!");
    }

    public bool TryStealOne(out PickupableItem item)
    {
        item = null;
        if (items.Count == 0) return false;
        
        item = items[items.Count - 1];
        item.gameObject.SetActive(true); // 훔칠 때도 보이게 켜줌
        
        RemoveItem(item); 
        item.transform.SetParent(null);

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if(rb) rb.isKinematic = false;

        return true;
    }
    
    public List<PickupableItem> GetAllItems() => items;

    public void ClearCart()
    {
        foreach (var item in new List<PickupableItem>(items))
        {
            // 결제 후 완전히 제거 (삭제하거나 풀링)
            // 여기선 리스트에서만 빼고 비활성화 유지
        }
        items.Clear();
        currentLoad = 0;
        itemCount = 0;
        UpdateUI();
        onCartUpdated?.Invoke();
    }

    public int GetCurrentCount() => currentLoad; 

    private void UpdateUI()
    {
        var topPanel = FindObjectOfType<TopPanelManager>();
        if (topPanel != null)
        {
            topPanel.UpdateCartDisplay(currentLoad, maxCapacity);
        }
    }
}