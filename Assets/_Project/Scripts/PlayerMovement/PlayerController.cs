using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private RotateToMouse rotateToMouse;               // 마우스 이동으로 카메라 회전
    private MovementCharacterController movement;      // 키보드 입력으로 플레이어 이동, 점프
    private Status status;                             // 걷기/달리기 속도 값

    private void Awake()
    {
        rotateToMouse = GetComponent<RotateToMouse>();
        movement      = GetComponent<MovementCharacterController>();
        status        = GetComponent<Status>();
    }

    private void Update()
    {
        // 게임이 일시정지 상태면 플레이어 입력을 받지 않음
        if (GameManager.GameIsPaused)       // 너가 GameManager.GameIsPaused 쓰는 중이면 그걸로 교체
            return;

        UpdateRotate();
        UpdateMove();
        UpdateJump();
        
    }

    /// <summary>
    /// 마우스 이동으로 카메라 회전 (감도 + Y축 반전 적용)
    /// </summary>
    private void UpdateRotate()
    {
        // 기본 마우스 입력 * 설정에서 정한 감도
        float mouseX = Input.GetAxis("Mouse X") * InputSettings.MouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * InputSettings.MouseSensitivity;

        // Y축 반전 옵션 적용
        if (InputSettings.InvertY)
            mouseY = -mouseY;

        if (rotateToMouse != null)
            rotateToMouse.UpdateRotate(mouseX, mouseY);
    }

    /// <summary>
    /// 이동 + 달리기 처리 (조작 설정의 키 바인딩 사용)
    /// </summary>
    private void UpdateMove()
    {
        float x = 0f;
        float z = 0f;

        // 키 바인딩에 따라 이동 방향 계산
        if (Input.GetKey(InputSettings.Keys[Action.MoveForward]))  z += 1f;
        if (Input.GetKey(InputSettings.Keys[Action.MoveBackward])) z -= 1f;
        if (Input.GetKey(InputSettings.Keys[Action.MoveRight]))    x += 1f;
        if (Input.GetKey(InputSettings.Keys[Action.MoveLeft]))     x -= 1f;

        Vector3 dir = new Vector3(x, 0f, z);

        // 대각선 이동 속도 보정 (정규화)
        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        float targetSpeed = status != null ? status.WalkSpeed : 0f;

        // 이동 중일 때만 속도 계산
        if (dir.sqrMagnitude > 0f && status != null)
        {
            bool isRun = false;

            // 앞으로 가는 중 + 달리기 키 → 달리기 속도
            if (dir.z > 0f && Input.GetKey(InputSettings.Keys[Action.Run]))
                isRun = true;

            targetSpeed = isRun ? status.RunSpeed : status.WalkSpeed;
        }

        // 항상 MoveSpeed를 최신 값으로 유지
        if (movement != null)
        {
            movement.MoveSpeed = targetSpeed;
            movement.MoveTo(dir);    // 실제 이동은 MovementCharacterController가 처리
        }
    }

    /// <summary>
    /// 점프 처리 (조작 설정의 Jump 키 사용)
    /// </summary>
    private void UpdateJump()
    {
        if (movement == null) return;

        if (Input.GetKeyDown(InputSettings.Keys[Action.Jump]))
        {
            movement.Jump();
        }
    }
}
