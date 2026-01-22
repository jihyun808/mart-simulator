using UnityEngine;

/// <summary>
/// 마우스 입력으로 카메라(플레이어) 회전 제어
/// X축(상하), Y축(좌우) 회전 및 각도 제한
/// </summary>
public class RotateToMouse : MonoBehaviour
{
    [Header("Rotation Speed")]
    [SerializeField] private float rotCamXAxisSpeed = 5f;
    [SerializeField] private float rotCamYAxisSpeed = 3f;

    [Header("X Axis Limits (Up/Down)")]
    [SerializeField] private float limitMinX = -80f;
    [SerializeField] private float limitMaxX = 50f;

    private float eulerAngleX;
    private float eulerAngleY;

    /// <summary>마우스 입력으로 회전 업데이트 (PlayerController에서 호출)</summary>
    public void UpdateRotate(float mouseX, float mouseY)
    {
        eulerAngleY += mouseX * rotCamYAxisSpeed;
        eulerAngleX -= mouseY * rotCamXAxisSpeed;
        eulerAngleX = ClampAngle(eulerAngleX, limitMinX, limitMaxX);

        transform.rotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0f);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;

        return Mathf.Clamp(angle, min, max);
    }
}