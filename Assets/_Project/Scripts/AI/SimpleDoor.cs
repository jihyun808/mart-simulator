using UnityEngine;

/// <summary>
/// 자동 여닫이 문 (Z축 회전)
/// AI와 플레이어가 트리거로 열 수 있음
/// </summary>
public class SimpleDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = -100f;
    [SerializeField] private float closeAngle = 2.912f;
    [SerializeField] private float openSpeed = 2f;

    [Header("Auto Close")]
    [SerializeField] private bool autoClose = true;
    [SerializeField] private float autoCloseDelay = 3f;

    public bool doorOpened = false;

    private float closeTimer = 0f;
    private bool isOpening = false;
    private bool isClosing = false;
    private Quaternion targetRotation;

    private void Start()
    {
        closeAngle = transform.localEulerAngles.z;
    }

    private void Update()
    {
        if (isOpening)
        {
            UpdateOpening();
        }
        else if (isClosing)
        {
            UpdateClosing();
        }
        else if (doorOpened && autoClose)
        {
            UpdateAutoClose();
        }
    }

    private void UpdateOpening()
    {
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            openSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(transform.localRotation, targetRotation) < 1f)
        {
            transform.localRotation = targetRotation;
            isOpening = false;
            doorOpened = true;
        }
    }

    private void UpdateClosing()
    {
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            openSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(transform.localRotation, targetRotation) < 1f)
        {
            transform.localRotation = targetRotation;
            isClosing = false;
            doorOpened = false;
        }
    }

    private void UpdateAutoClose()
    {
        closeTimer += Time.deltaTime;
        if (closeTimer >= autoCloseDelay)
        {
            CloseDoorNow();
            closeTimer = 0f;
        }
    }

    /// <summary>문 열기 (AI/플레이어 트리거에서 호출)</summary>
    public void OpenDoorNow()
    {
        if (doorOpened || isOpening) return;

        targetRotation = Quaternion.Euler(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            openAngle
        );
        isOpening = true;
        isClosing = false;
        closeTimer = 0f;
    }

    /// <summary>문 닫기 (자동 닫기에서 호출)</summary>
    public void CloseDoorNow()
    {
        if (!doorOpened || isClosing) return;

        targetRotation = Quaternion.Euler(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            closeAngle
        );
        isClosing = true;
        isOpening = false;
    }
}