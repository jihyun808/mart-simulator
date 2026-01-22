using UnityEngine;

/// <summary>
/// 플레이어 이동 속도 데이터
/// Inspector에서 걷기/달리기/앉기 속도 설정
/// </summary>
public class Status : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeed = 1.5f;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float CrouchSpeed => crouchSpeed;
}