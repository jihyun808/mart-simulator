using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [SerializeField]
    private float rotCamXAxisSpeed = 5f;   // 카메라 x축 회전속도 (위/아래)
    [SerializeField]
    private float rotCamYAxisSpeed = 3f;   // 카메라 y축 회전속도 (좌/우)

    private float limitMinX = -80f;        // 카메라 x축 회전 범위 (최소)
    private float limitMaxX =  50f;        // 카메라 x축 회전 범위 (최대)
    private float eulerAngleX;
    private float eulerAngleY;

    /// <summary>
    /// PlayerController에서 넘겨준 마우스 입력값으로 회전 처리
    /// </summary>
    public void UpdateRotate(float mouseX, float mouseY)
    {
        // 좌/우는 Y 축 회전, 위/아래는 X 축 회전
        eulerAngleY += mouseX * rotCamYAxisSpeed;
        eulerAngleX -= mouseY * rotCamXAxisSpeed;

        // X축(위/아래) 회전 각도 제한
        eulerAngleX = ClampAngle(eulerAngleX, limitMinX, limitMaxX);

        transform.rotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0f);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle >  360f) angle -= 360f;

        return Mathf.Clamp(angle, min, max);
    }
}