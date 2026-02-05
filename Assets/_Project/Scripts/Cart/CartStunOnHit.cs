using UnityEngine;

public class CartStunOnHit : MonoBehaviour
{
    [SerializeField] private CartMount cartMount;

    private void Awake()
    {
        if (cartMount == null)
            cartMount = GetComponent<CartMount>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1️⃣ 플레이어가 조종 중인 카트가 아니면 무시
        if (cartMount == null || !cartMount.IsMounted)
            return;

        // 2️⃣ 경쟁자와 충돌했는지
        if (!collision.collider.CompareTag("Competitor"))
            return;

        var ai = collision.collider.GetComponent<CompetitorAIController>();
        if (ai == null)
            return;

        Debug.Log("💥 [Cart] 경쟁자와 충돌 → 기절!");

        ai.Stun();
    }

    private void OnTriggerEnter(Collider other)
{
    if (!other.CompareTag("Competitor"))
        return;

    CompetitorAIController ai = other.GetComponent<CompetitorAIController>();
    if (ai != null)
    {
        ai.Stun();
    }
}
}
