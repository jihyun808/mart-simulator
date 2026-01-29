using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 플레이어 기절 처리 (AI에게 잡혔을 때)
/// 기절 시 인벤토리 아이템 절반 드롭 + 화면 암전
/// </summary>
public class PlayerStunHandler : MonoBehaviour
{
    [Header("Stun Settings")]
    [SerializeField] private float stunDuration = 3f;

    [Header("Item Drop Settings")]
    [SerializeField] private float dropRadius = 1.5f;
    [SerializeField] private float dropHeight = 1.0f;
    [SerializeField] private float dropForce = 2f;

    private bool isStunned = false;
    private PlayerController playerController;
    private Inventory inventory;
    private StunScreenEffect screenEffect;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        inventory = GetComponent<Inventory>();
        
        // ✅ 화면 암전 컴포넌트 찾기/생성
        screenEffect = FindObjectOfType<StunScreenEffect>();
        if (screenEffect == null)
        {
            GameObject effectObj = new GameObject("Stun Screen Effect");
            screenEffect = effectObj.AddComponent<StunScreenEffect>();
        }
    }

    /// <summary>기절 시작 (AIController에서 호출)</summary>
    public void Stun(float duration)
    {
        if (isStunned) return;
        StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;

        // ✅ 화면 암전 시작
        if (screenEffect != null)
        {
            screenEffect.StartStun(duration);
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        DropRandomItems();

        yield return new WaitForSeconds(duration);

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        isStunned = false;
    }

    private void DropRandomItems()
    {
        if (inventory == null) return;

        List<PickupableItem> currentItems = inventory.GetAllItems();
        int itemCount = currentItems.Count;

        if (itemCount == 0) return;

        int itemsToDropCount = Mathf.Max(1, itemCount / 2);

        for (int i = 0; i < itemsToDropCount; i++)
        {
            currentItems = inventory.GetAllItems();
            if (currentItems.Count == 0) break;

            int randomIndex = Random.Range(0, currentItems.Count);
            PickupableItem itemToDrop = currentItems[randomIndex];

            inventory.RemoveItem(itemToDrop);

            DropItemToWorld(itemToDrop);
        }
    }

    private void DropItemToWorld(PickupableItem item)
    {
        item.gameObject.SetActive(true);

        Vector3 dropPosition = transform.position + Random.insideUnitSphere * dropRadius;
        dropPosition.y = transform.position.y + dropHeight;
        item.transform.position = dropPosition;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector3.up * dropForce + Random.insideUnitSphere * dropForce, ForceMode.Impulse);
        }
    }

    public bool IsStunned() => isStunned;
}