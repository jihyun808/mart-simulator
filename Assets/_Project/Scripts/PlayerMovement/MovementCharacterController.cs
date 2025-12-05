// MovementCharacterController.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class MovementCharacterController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private float crouchingHeight = 1.2f;
    [SerializeField] private float crouchSpeed = 1.0f;

    [Header("Slide Settings")]
    [SerializeField] private float slideDeceleration = 5f; 
    [SerializeField] private float minSlideSpeed = 0.1f;   

    private Vector3 moveForce;
    private Vector3 slideVelocity; 
    private CharacterController characterController;
    private bool isCrouching = false;
    private bool isTransitioning = false;
    private Transform cameraTransform;
    private float originalCameraY;

    public float MoveSpeed
    {
        set => moveSpeed = Mathf.Max(0, value);
        get => moveSpeed;
    }

    public bool IsCrouching => isCrouching;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        Camera cam = Camera.main;
        if (cam != null)
        {
            cameraTransform = cam.transform;
            originalCameraY = cameraTransform.localPosition.y;
        }
    }

    private void Update()
    {
        if (!characterController.isGrounded)
        {
            moveForce.y += gravity * Time.deltaTime;
        }
        else if (moveForce.y < 0)
        {
            moveForce.y = -2f;
        }

        if (slideVelocity.magnitude > minSlideSpeed)
        {
            slideVelocity = Vector3.Lerp(slideVelocity, Vector3.zero, slideDeceleration * Time.deltaTime);
        }
        else
        {
            slideVelocity = Vector3.zero;
        }

        Vector3 finalMove = moveForce + slideVelocity;
        characterController.Move(finalMove * Time.deltaTime);
    }

    public void MoveTo(Vector3 direction)
    {
        direction = transform.rotation * new Vector3(direction.x, 0, direction.z);
        
        float currentSpeed = isCrouching ? crouchSpeed : moveSpeed;
        
        if (direction.magnitude > 0.01f)
        {
            moveForce = new Vector3(direction.x * currentSpeed, moveForce.y, direction.z * currentSpeed);
            slideVelocity = new Vector3(moveForce.x, 0, moveForce.z);  
        }
        else
        {
            moveForce = new Vector3(0, moveForce.y, 0);
        }
    }

    public void Jump()
    {
        if (characterController.isGrounded && !isCrouching)
        {
            moveForce.y = jumpForce;
        }
    }

    public void SetCrouch(bool crouch)
    {
        if (isTransitioning) return;
        if (isCrouching == crouch) return;

        if (crouch)
        {
            StartCoroutine(CrouchDownSmoothly());
        }
        else
        {
            if (CanStandUp())
            {
                StartCoroutine(StandUpSmoothly());
            }
        }
    }

    private IEnumerator CrouchDownSmoothly()
    {
        isTransitioning = true;
        isCrouching = true;

        float crouchRatio = crouchingHeight / standingHeight;
        float startScale = 1f;
        float targetScale = crouchRatio;
        float duration = 0.2f;
        float elapsed = 0f;

        Vector3 startCameraPos = cameraTransform != null ? cameraTransform.localPosition : Vector3.zero;
        float heightDifference = standingHeight - crouchingHeight;
        Vector3 targetCameraPos = cameraTransform != null ?
            new Vector3(startCameraPos.x, originalCameraY - (heightDifference * 0.7f), startCameraPos.z) : Vector3.zero;

        moveForce.y = -3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float smoothProgress = Mathf.Pow(progress, 2f);

            float currentScale = Mathf.Lerp(startScale, targetScale, smoothProgress);
            transform.localScale = new Vector3(transform.localScale.x, currentScale, transform.localScale.z);

            if (cameraTransform != null)
            {
                Vector3 newCameraPos = Vector3.Lerp(startCameraPos, targetCameraPos, smoothProgress);
                cameraTransform.localPosition = newCameraPos;
            }

            yield return null;
        }

        transform.localScale = new Vector3(transform.localScale.x, crouchRatio, transform.localScale.z);
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = targetCameraPos;
        }

        isTransitioning = false;
    }

    private IEnumerator StandUpSmoothly()
    {
        isTransitioning = true;

        float crouchRatio = crouchingHeight / standingHeight;
        float startScale = crouchRatio;
        float targetScale = 1f;
        float duration = 0.3f;
        float elapsed = 0f;

        Vector3 startCameraPos = cameraTransform != null ? cameraTransform.localPosition : Vector3.zero;
        Vector3 targetCameraPos = cameraTransform != null ?
            new Vector3(startCameraPos.x, originalCameraY, startCameraPos.z) : Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float smoothProgress = 1f - Mathf.Pow(1f - progress, 3f);

            float currentScale = Mathf.Lerp(startScale, targetScale, smoothProgress);
            transform.localScale = new Vector3(transform.localScale.x, currentScale, transform.localScale.z);

            if (cameraTransform != null)
            {
                Vector3 newCameraPos = Vector3.Lerp(startCameraPos, targetCameraPos, smoothProgress);
                cameraTransform.localPosition = newCameraPos;
            }

            yield return null;
        }

        transform.localScale = new Vector3(transform.localScale.x, 1f, transform.localScale.z);
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = targetCameraPos;
        }

        isCrouching = false;
        isTransitioning = false;
    }

    private bool CanStandUp()
    {
        Vector3 headPosition = transform.position + Vector3.up * standingHeight;
        float checkRadius = characterController.radius * 0.8f;

        bool canStand = !Physics.CheckSphere(headPosition, checkRadius);

        return canStand;
    }

    private void OnDrawGizmosSelected()
    {
        if (characterController != null && isCrouching)
        {
            Gizmos.color = Color.cyan;
            Vector3 headPos = transform.position + Vector3.up * standingHeight;
            Gizmos.DrawWireSphere(headPos, characterController.radius * 0.8f);
        }
    }
}