// PlayerThrowController.cs
using UnityEngine;

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

    private InputHandler inputHandler;
    private float throwChargeTimer = 0f;
    private bool isCharging = false;
    private bool isSwinging = false;
    private float swingTimer = 0f;

    private void Start()
    {
        inputHandler = GetComponent<InputHandler>();
        
        if (pickupController == null)
            pickupController = GetComponent<PlayerPickupController>();

        if (throwPoint == null)
            throwPoint = transform;
    }

    private void Update()
    {
        if (GameManager.GameIsPaused)
            return;

        HandleThrowInput();
        UpdateSwing();
    }

    private void HandleThrowInput()
    {
        if (inputHandler == null) return;

        PickupableItem currentItem = pickupController?.GetCurrentItem();
        if (currentItem == null) return;

        if (inputHandler.IsThrowPressed())
        {
            isCharging = true;
            throwChargeTimer = 0f;
        }

        if (Input.GetKey(InputSettings.Keys[Action.Throw]) && isCharging)
        {
            throwChargeTimer += Time.deltaTime;
        }

        if (Input.GetKeyUp(InputSettings.Keys[Action.Throw]) && isCharging)
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

    public float GetChargePercent()
    {
        if (!isCharging) return 0f;
        return Mathf.Clamp01(throwChargeTimer / throwChargeTime);
    }
}