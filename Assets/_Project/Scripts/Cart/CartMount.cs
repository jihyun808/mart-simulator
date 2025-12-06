using System.Collections.Generic;
using UnityEngine;

public class CartMount : MonoBehaviour
{
    [Header("Follow")]
    public Transform handle;                      // 카트 손잡이(자식)
    public Transform playerAnchor;                // 플레이어 앞 고정점(필수)
    public float smoothTime = 0.12f;              // 스무스 추종 감쇠 시간
    public float maxFollowSpeed = 6f;             // 최대 추종 속도
    public float rotationSpeed = 6f;              // 회전 스무스 속도
    public float minDistanceFromPlayer = 0.55f;   // 겹침 방지 최소 거리

    [Header("Orientation Fix")]
    public Vector3 rotationOffsetEuler = new(0, 0, 0); // 모델 축 보정(Y=±90 등)

    [Header("Ground Snap")]
    public bool snapToGround = true;
    public LayerMask groundMask;                  // 바닥 레이어
    public float groundOffset = 0.02f;

    private Transform _player;                    // Mount 시 받은 pivot
    private bool _mounted;
    private Rigidbody _rb;

    private Vector3 _vel;                         // SmoothDamp 내부 속도 상태
    private readonly List<(Collider a, Collider b)> _ignoredPairs = new();

    public bool IsMounted => _mounted;

    public void Mount(Transform player)
    {
        _player = player;
        _mounted = true;

        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_rb)
        {
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;         // ← linearVelocity 아님!
            _rb.angularVelocity = Vector3.zero;
        }

        TogglePlayerCartCollision(true);
    }

    public void Unmount()
    {
        _mounted = false;
        TogglePlayerCartCollision(false);
        _ignoredPairs.Clear();
        if (_rb) _rb.isKinematic = false;
        _player = null;
        _vel = Vector3.zero;
    }

    void LateUpdate()
    {
        if (!_mounted || _player == null || handle == null || playerAnchor == null) return;

        // 1) 앵커의 평면 위치(XZ)만 목표로 사용
        Vector3 anchorPos = playerAnchor.position;

        // handle->root 오프셋 유지(손잡이가 앵커로 오게 루트 이동)
        Vector3 handleToRoot = transform.position - handle.position;
        Vector3 desired = anchorPos + handleToRoot;

        // Y는 현재 높이 유지(이후 바닥 스냅으로 보정)
        float keepY = transform.position.y;
        desired.y = keepY;

        // 최소 거리 보장(앞으로 못 가는 느낌 방지)
        Vector3 flatToPlayer = desired - _player.position; flatToPlayer.y = 0f;
        float d = flatToPlayer.magnitude;
        if (d < minDistanceFromPlayer && d > 1e-3f)
        {
            desired = _player.position + flatToPlayer.normalized * minDistanceFromPlayer;
            desired.y = keepY;
        }

        // 바닥 스냅
        if (snapToGround)
        {
            if (Physics.Raycast(desired + Vector3.up * 2f, Vector3.down, out var hit, 5f, groundMask, QueryTriggerInteraction.Ignore))
                desired.y = hit.point.y + groundOffset;
        }

        // 2) 위치 스무딩
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime, maxFollowSpeed);

        // 3) 회전 스무딩 (수평만, 모델 보정 포함)
        Vector3 fwd = _player.forward; fwd.y = 0f;
        if (fwd.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(fwd) * Quaternion.Euler(rotationOffsetEuler);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
    }

    // 플레이어와 카트 충돌 임시 on/off
    void TogglePlayerCartCollision(bool ignore)
    {
        if (_player == null) return;
        var playerCols = _player.GetComponentsInChildren<Collider>(true);
        var cartCols   = GetComponentsInChildren<Collider>(true);

        foreach (var pc in playerCols)
        {
            if (!pc || !pc.enabled) continue;
            foreach (var cc in cartCols)
            {
                if (!cc || !cc.enabled) continue;
                if (cc.isTrigger) continue;
                Physics.IgnoreCollision(pc, cc, ignore);
                if (ignore) _ignoredPairs.Add((pc, cc));
            }
        }

        if (!ignore)
            foreach (var p in _ignoredPairs)
                if (p.a && p.b) Physics.IgnoreCollision(p.a, p.b, false);
    }
}
