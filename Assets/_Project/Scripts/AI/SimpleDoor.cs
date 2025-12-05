using UnityEngine;

public class SimpleDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = -100f;
    [SerializeField] private float closeAngle = 2.912f;
    [SerializeField] private float openSpeed = 2f;

    [Header("State")]
    public bool doorOpened = false;

    [Header("Auto Close")]
    [SerializeField] private bool autoClose = true;
    [SerializeField] private float autoCloseDelay = 3f;

private float closeTimer = 0f;

    private bool isOpening = false;
    private bool isClosing = false;
    private Quaternion targetRotation;

    private void Start()
    {
        // ✅ Z축 회전값 저장
        closeAngle = transform.localEulerAngles.z;
    }

    private void Update()
    {
        if (isOpening)
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
        else if (isClosing)
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
        else if (doorOpened && autoClose)
        {
            closeTimer += Time.deltaTime;
            if (closeTimer >= autoCloseDelay)
            {
                CloseDoorNow();
                closeTimer = 0f;
            }
        }
    }

    public void OpenDoorNow()
    {
        if (doorOpened || isOpening) return;

        // ✅ Z축으로 회전
        targetRotation = Quaternion.Euler(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            openAngle
        );
        isOpening = true;
        isClosing = false;
    }

    public void CloseDoorNow()
    {
        if (!doorOpened || isClosing) return;

        // ✅ Z축으로 회전
        targetRotation = Quaternion.Euler(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            closeAngle
        );
        isClosing = true;
        isOpening = false;
    }
}