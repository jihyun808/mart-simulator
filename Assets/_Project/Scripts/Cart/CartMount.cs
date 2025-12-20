using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CartMount : MonoBehaviour
{
    [Header("References")]
    public Transform handle;
    public Transform playerAnchor;
    public Transform playerRoot;

    [Header("Follow Tuning")]
    public float followGap = 0.25f;
    public float minDistanceFromPlayer = 0.65f;
    public float positionGain = 3.0f;
    public float damping = 0.22f;
    public float maxSpeed = 8.0f;

    [Header("Rotation")]
    public float yawDegPerSec = 420f;
    public Vector3 rotationOffsetEuler = Vector3.zero;

    [Header("Ground Snap (Optional)")]
    public bool snapToGround = true;
    public LayerMask groundMask;
    public float groundRayHeight = 0.6f;
    public float groundRayLen = 1.2f;
    public float groundOffset = 0.02f;

    [Header("Anti-Jitter")]
    public float targetSmoothing = 0.05f;
    public float deadZone = 0.015f;

    [Header("Y Lock When Mounted")]
    public bool lockYOnMount = true;   // ✅ 장착 중 Y 고정
    private float _lockedY;            // ✅ 고정할 Y 값
    private RigidbodyConstraints _origConstraints; // ✅ 원래 제약 저장

    private Rigidbody _rb;
    private bool _mounted;

    private Vector3 _anchorPos;
    private Vector3 _anchorFwd;

    private Vector3 _filteredTarget;
    private Vector3 _filteredVel;

    public bool IsMounted => _mounted;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.mass = Mathf.Max(1f, _rb.mass);
        _rb.linearDamping = Mathf.Max(_rb.linearDamping, 1.0f);
        _rb.angularDamping = Mathf.Max(_rb.angularDamping, 2.0f);
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.isKinematic = false;
        _origConstraints = _rb.constraints; // 저장
    }

    public void Mount(Transform playerAnchorTransform)
    {
        playerAnchor = playerAnchorTransform;
        if (!playerRoot && playerAnchor) playerRoot = playerAnchor.root;

        _mounted = true;

        _anchorPos = playerAnchor ? playerAnchor.position : transform.position;
        Vector3 f = playerAnchor ? playerAnchor.forward : transform.forward;
        f.y = 0f;
        _anchorFwd = f.sqrMagnitude > 1e-6f ? f.normalized : Vector3.forward;

        _filteredTarget = transform.position;
        _filteredVel = Vector3.zero;

        if (lockYOnMount)
        {
            _lockedY = transform.position.y;                     // ✅ 현재 Y 저장
            _rb.constraints = _origConstraints | RigidbodyConstraints.FreezePositionY; // ✅ 물리적으로 Y 고정
        }
    }

    public void Unmount()
    {
        _mounted = false;
        _rb.linearVelocity *= 0.5f;
        _rb.angularVelocity = Vector3.zero;

        // ✅ 원래 제약으로 복원
        _rb.constraints = _origConstraints;
    }

    void Update()
    {
        if (!_mounted || !playerAnchor) return;

        _anchorPos = playerAnchor.position;

        Vector3 f = playerAnchor.forward;
        f.y = 0f;
        _anchorFwd = (f.sqrMagnitude > 1e-6f) ? f.normalized : _anchorFwd;
    }

    void FixedUpdate()
    {
        if (!_mounted || !playerAnchor || !handle) return;

        // 1) 손잡이-루트 오프셋 유지
        Vector3 handleToRoot = transform.position - handle.position;

        // 2) 기본 목표 (앵커 뒤로 followGap만큼)
        Vector3 target = _anchorPos + handleToRoot - _anchorFwd * followGap;

        // 3) 플레이어 최소 거리 보장(수평)
        if (playerRoot)
        {
            Vector3 toCart = transform.position - playerRoot.position; toCart.y = 0f;
            float d = toCart.magnitude;
            if (d < minDistanceFromPlayer && d > 1e-4f)
                target += toCart.normalized * (minDistanceFromPlayer - d);
        }

        // 4) Y 처리
        if (lockYOnMount)
        {
            // ✅ 장착 동안 고정 Y 사용
            target.y = _lockedY;
        }
        else if (snapToGround)
        {
            // 평소처럼 지면 스냅
            Vector3 rayStart = target + Vector3.up * groundRayHeight;
            if (Physics.Raycast(rayStart, Vector3.down, out var hit, groundRayLen, groundMask, QueryTriggerInteraction.Ignore))
                target.y = hit.point.y + groundOffset;
            else
                target.y = transform.position.y;
        }
        else
        {
            target.y = transform.position.y;
        }

        // 5) 타깃 로우패스
        if (_filteredTarget == Vector3.zero) _filteredTarget = transform.position;
        _filteredTarget = Vector3.SmoothDamp(_filteredTarget, target, ref _filteredVel, targetSmoothing);

        // 6) PD 속도 제어(수평만)
        Vector3 posError = _filteredTarget - transform.position; posError.y = 0f;
        if (posError.sqrMagnitude < deadZone * deadZone) posError = Vector3.zero;

        Vector3 curVel = _rb.linearVelocity;
        Vector3 flatVel = new Vector3(curVel.x, 0f, curVel.z);
        Vector3 desiredVel = positionGain * posError - damping * flatVel;

        if (desiredVel.magnitude > maxSpeed)
            desiredVel = desiredVel.normalized * maxSpeed;

        _rb.linearVelocity = new Vector3(desiredVel.x, curVel.y, desiredVel.z);

        // 7) 회전(Yaw)
        Vector3 face = _anchorFwd;
        if (face.sqrMagnitude > 1e-6f)
        {
            Quaternion targetRot = Quaternion.LookRotation(face) * Quaternion.Euler(rotationOffsetEuler);
            Quaternion slerped = Quaternion.RotateTowards(_rb.rotation, targetRot, yawDegPerSec * Time.fixedDeltaTime);
            _rb.MoveRotation(slerped);
        }
    }
}
