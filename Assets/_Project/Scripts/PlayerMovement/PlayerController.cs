using UnityEngine;

/// <summary>
/// 플레이어 입력 처리 및 캐릭터 제어
/// 이동, 회전, 점프, 앉기 통합 관리
/// 키 설정: W/A/S/D(이동), Space(점프), Left Shift(달리기), Left Ctrl(앉기)
/// </summary>
public class PlayerController : MonoBehaviour
{
    private RotateToMouse rotateToMouse;
    private MovementCharacterController movement;
    private Status status;
    private bool isCrouchToggled = false;

    private void Awake()
    {
        rotateToMouse = GetComponent<RotateToMouse>();
        movement = GetComponent<MovementCharacterController>();
        status = GetComponent<Status>();
    }

    private void Update()
    {
        if (GameManager.GameIsPaused || GameManager.IsGameOver) return;

        UpdateRotate();
        UpdateMove();
        UpdateJump();
        UpdateCrouch();
    }

    private void UpdateRotate()
    {
        if (rotateToMouse == null) return;

        float mouseX = Input.GetAxis("Mouse X") * ControlsSettings.MouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * ControlsSettings.MouseSensitivity;

        if (ControlsSettings.InvertY) mouseY = -mouseY;

        rotateToMouse.UpdateRotate(mouseX, mouseY);
    }

    private void UpdateMove()
    {
        if (movement == null) return;

        Vector2 moveInput = GetMoveInput();
        Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (dir.sqrMagnitude > 1f) dir.Normalize();

        float targetSpeed = CalculateSpeed(dir);
        
        movement.MoveSpeed = targetSpeed;
        movement.MoveTo(dir);
    }

    private Vector2 GetMoveInput()
    {
        float x = 0f, z = 0f;
        if (Input.GetKey(KeyCode.W)) z += 1f;
        if (Input.GetKey(KeyCode.S)) z -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        return new Vector2(x, z);
    }

    private float CalculateSpeed(Vector3 direction)
    {
        if (status == null) return 0f;

        if (direction.sqrMagnitude == 0f) return status.WalkSpeed;

        if (movement.IsCrouching) return status.CrouchSpeed;

        bool isRunning = direction.z > 0f && Input.GetKey(KeyCode.LeftShift);
        return isRunning ? status.RunSpeed : status.WalkSpeed;
    }

    private void UpdateJump()
    {
        if (movement == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            movement.Jump();
        }
    }

    private void UpdateCrouch()
    {
        if (movement == null) return;

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouchToggled = !isCrouchToggled;
            movement.SetCrouch(isCrouchToggled);
        }
    }
}