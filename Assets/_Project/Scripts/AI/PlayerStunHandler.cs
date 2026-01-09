using UnityEngine;
using System.Collections;
using System.Collections.Generic; // 리스트 사용

public class PlayerStunHandler : MonoBehaviour
{
    [Header("Stun Settings")]
    [SerializeField] private float stunDuration = 3f;

    private bool isStunned = false;
    private PlayerController playerController;
    private Inventory inventory;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        inventory = GetComponent<Inventory>();
    }

    public void Stun(float duration)
    {
        if (isStunned) return;
        StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;

        if (playerController != null) playerController.enabled = false;

        DropRandomItems(); // 아이템 떨구기

        Debug.Log($"[Player] 기절! {duration}초 간 불능");

        yield return new WaitForSeconds(duration);

        if (playerController != null) playerController.enabled = true;

        isStunned = false;
        Debug.Log("[Player] 기절 회복!");
    }

    private void DropRandomItems()
    {
        if (inventory == null) return;

        // 1. 현재 가지고 있는 모든 아이템을 리스트로 가져옵니다.
        List<PickupableItem> currentItems = inventory.GetAllItems();
        int itemCount = currentItems.Count;

        if (itemCount == 0) return;

        // 절반 정도 떨어뜨림 (최소 1개)
        int itemsToDropCount = Mathf.Max(1, itemCount / 2);

        Debug.Log($"충격으로 인해 아이템 {itemsToDropCount}개를 떨어뜨립니다!");

        for (int i = 0; i < itemsToDropCount; i++)
        {
            // 다시 목록 갱신 (하나 떨구면 리스트가 변하니까)
            currentItems = inventory.GetAllItems();
            if (currentItems.Count == 0) break;

            // 2. 랜덤하게 하나 고름 (빈 슬롯 걱정 없음)
            int randomIndex = Random.Range(0, currentItems.Count);
            PickupableItem itemToDrop = currentItems[randomIndex];

            // 3. 인벤토리에서 제거
            inventory.RemoveItem(itemToDrop);

            // 4. 월드에 다시 뿌리기
            itemToDrop.gameObject.SetActive(true);
            
            // 위치: 플레이어 주변 랜덤 위치
            itemToDrop.transform.position = transform.position + Random.insideUnitSphere * 1.5f;
            // 높이 보정 (땅 밑으로 안 꺼지게)
            itemToDrop.transform.position = new Vector3(
                itemToDrop.transform.position.x,
                transform.position.y + 1.0f,
                itemToDrop.transform.position.z
            );

            // 물리 힘 가하기 (튕겨나가는 효과)
            Rigidbody rb = itemToDrop.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(Vector3.up * 2f + Random.insideUnitSphere * 2f, ForceMode.Impulse);
            }
        }
    }

    public bool IsStunned() => isStunned;
}