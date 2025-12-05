// PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private RotateToMouse rotateToMouse;
    private MovementCharacterController movement;
    private Status status;
    private InputHandler inputHandler;
    private bool isCrouchToggled = false;

    private void Awake()
    {
        rotateToMouse = GetComponent<RotateToMouse>();
        movement = GetComponent<MovementCharacterController>();
        status = GetComponent<Status>();
        inputHandler = GetComponent<InputHandler>();
    }

    private void Update()
    {
        if (GameManager.GameIsPaused)
            return;

        UpdateRotate();
        UpdateMove();
        UpdateJump();
        UpdateCrouch();
    }

    private void UpdateRotate()
    {
        if (rotateToMouse == null || inputHandler == null) return;

        Vector2 mouseInput = inputHandler.GetMouseInput();
        rotateToMouse.UpdateRotate(mouseInput.x, mouseInput.y);
    }

    private void UpdateMove()
    {
        if (movement == null || inputHandler == null) return;

        Vector2 moveInput = inputHandler.GetMoveInput();
        Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        float targetSpeed = 0f;

        if (dir.sqrMagnitude > 0f && status != null)
        {
            if (movement.IsCrouching)
            {
                targetSpeed = status.CrouchSpeed;
            }
            else
            {
                bool isRun = dir.z > 0f && inputHandler.IsRunning();
                targetSpeed = isRun ? status.RunSpeed : status.WalkSpeed;
            }
        }
        else if (status != null)
        {
            targetSpeed = status.WalkSpeed;
        }

        movement.MoveSpeed = targetSpeed;
        movement.MoveTo(dir);
    }

    private void UpdateJump()
    {
        if (movement == null || inputHandler == null) return;

        if (inputHandler.IsJumpPressed())
        {
            movement.Jump();
        }
    }

    private void UpdateCrouch()
    {
        if (movement == null || inputHandler == null) return;

        if (inputHandler.IsCrouchPressed())
        {
            isCrouchToggled = !isCrouchToggled;
            movement.SetCrouch(isCrouchToggled);
        }
    }
}