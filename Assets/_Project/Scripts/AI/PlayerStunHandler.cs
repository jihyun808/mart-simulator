// PlayerStunHandler.cs
using UnityEngine;
using System.Collections;

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

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        DropRandomItems();

        Debug.Log($"[Player] 기절! {duration}초 동안 움직일 수 없습니다.");

        yield return new WaitForSeconds(duration);

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        isStunned = false;

        Debug.Log("[Player] 기절에서 회복!");
    }

    private void DropRandomItems()
    {
        if (inventory == null) return;

        int itemCount = inventory.GetItemCount();
        if (itemCount == 0) return;

        int itemsToDrop = Mathf.Max(1, itemCount / 2);

        for (int i = 0; i < itemsToDrop; i++)
        {
            if (inventory.GetItemCount() == 0) break;

            int randomIndex = Random.Range(0, inventory.GetItemCount());
            PickupableItem item = inventory.GetItem(randomIndex);

            if (item != null)
            {
                inventory.RemoveItem(item);

                item.gameObject.SetActive(true);
                item.transform.position = transform.position + Random.insideUnitSphere * 2f;
                item.transform.position = new Vector3(
                    item.transform.position.x,
                    transform.position.y,
                    item.transform.position.z
                );

                Rigidbody rb = item.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.AddForce(Random.insideUnitSphere * 3f, ForceMode.Impulse);
                }

                Debug.Log($"[Player] 아이템 떨어뜨림: {item.gameObject.name}");
            }
        }
    }

    public bool IsStunned()
    {
        return isStunned;
    }
}