using UnityEngine;

/// <summary>
/// 플레이어 아이템 투척 및 휘두르기 제어
/// 마우스 버튼 길게 누르기: 투척 / 짧게 누르기: 휘두르기
/// 키 설정: Mouse 0 (좌클릭)
/// </summary>
public class PlayerThrowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform throwPoint;
    [SerializeField] private PlayerPickupController pickupController;

    [Header("Swing Settings")]
    [SerializeField] private float swingDuration = 0.3f;
    [SerializeField] private float swingRotation = 45f;

    [Header("Throw Settings")]
    [SerializeField] private float throwChargeTime = 0.5f;
    [SerializeField] private float minThrowForce = 5f;
    [SerializeField] private float maxThrowForce = 20f;

    private float throwChargeTimer = 0f;
    private bool isCharging = false;
    private bool isSwinging = false;
    private float swingTimer = 0f;

    private void Start()
    {
        if (pickupController == null)
            pickupController = GetComponent<PlayerPickupController>();

        if (throwPoint == null)
            throwPoint = transform;
    }

    private void Update()
    {
        if (GameManager.GameIsPaused) return;

        HandleThrowInput();
        UpdateSwing();
    }

    private void HandleThrowInput()
    {
        PickupableItem currentItem = pickupController?.GetCurrentItem();
        if (currentItem == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            throwChargeTimer = 0f;
        }

        if (Input.GetMouseButton(0) && isCharging)
        {
            throwChargeTimer += Time.deltaTime;
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            isCharging = false;

            if (throwChargeTimer >= throwChargeTime)
            {
                ThrowItem(currentItem);
            }
            else
            {
                SwingItem();
            }

            throwChargeTimer = 0f;
        }
    }

    private void SwingItem()
    {
        if (isSwinging) return;

        isSwinging = true;
        swingTimer = 0f;
    }

    private void UpdateSwing()
    {
        if (!isSwinging) return;

        swingTimer += Time.deltaTime;

        PickupableItem currentItem = pickupController?.GetCurrentItem();
        if (currentItem != null)
        {
            float progress = swingTimer / swingDuration;
            float angle = Mathf.Sin(progress * Mathf.PI) * swingRotation;
            
            currentItem.transform.localRotation = Quaternion.Euler(angle, 0, 0);
        }

        if (swingTimer >= swingDuration)
        {
            isSwinging = false;
            
            if (currentItem != null)
            {
                currentItem.transform.localRotation = Quaternion.identity;
            }
        }
    }

    private void ThrowItem(PickupableItem item)
    {
        if (item == null) return;

        float chargePercent = Mathf.Clamp01(throwChargeTimer / throwChargeTime);
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargePercent);

        item.Drop();
        
        Vector3 throwDirection = throwPoint.forward;
        
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }

        if (pickupController != null)
        {
            pickupController.ClearCurrentItem();
        }
    }

    /// <summary>현재 충전 진행도 (0~1) - UI 표시용</summary>
    public float GetChargePercent()
    {
        if (!isCharging) return 0f;
        return Mathf.Clamp01(throwChargeTimer / throwChargeTime);
    }
}