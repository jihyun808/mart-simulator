using System.Collections;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("AI States")]
    private Vector3 homePosition;
    private Quaternion homeRotation;

    [Header("Movement Settings")]
    public float approachSpeed = 2.5f;
    public float watchSpeed = 1f;
    public float chaseSpeed = 4f;
    public float returnSpeed = 2f;

    [Header("Detection Settings")]
    public float detectionRange = 15f;
    public float watchDistance = 3f;
    public float catchDistance = 1.5f;
    public Transform player;

    [Header("Watch Settings")]
    public float watchDuration = 10f;

    [Header("Obstacle Avoidance")]
    public float obstacleDetectionDistance = 1.5f;
    public float wallAvoidanceForce = 2f;
    public LayerMask obstacleLayerMask = -1;
    public float smoothTurnSpeed = 5f;

    [Header("Stun Settings")]
    public float stunDuration = 3f;

    [Header("UI Settings")]
    public bool showDebug = true;

    private PlayerSuspicionDetector suspicionDetector;
    private Rigidbody rb;  // ✅ 추가

    private Vector3 lastMoveDirection = Vector3.forward;
    private float stuckTime = 0f;
    private Vector3 lastPosition;
    private float watchTimer = 0f;
    private bool hasStunnedPlayer = false;

    public enum AIState
    {
        Idle,
        Approach,
        Watching,
        Chase,
        Return,
        Stunning
    }

    private AIState currentState = AIState.Idle;

    private void Start()
{
    homePosition = transform.position;
    homeRotation = transform.rotation;
    lastPosition = transform.position;
    lastMoveDirection = transform.forward;

    // --- 🚨 최소 콜라이더 및 Rigidbody 설정 복구 (땅 꺼짐 방지 목적) ---
    rb = GetComponent<Rigidbody>();
    if (rb == null)
    {
        rb = gameObject.AddComponent<Rigidbody>();
    }
    
    // ✅ 핵심 변경: Kinematic 활성화 및 중력 비활성화
    // 이렇게 하면 물리 엔진의 중력 계산을 무시하고, 
    // AI의 움직임을 오직 스크립트(MovePosition)로만 제어하게 되어 땅에 꺼지는 문제가 해결돼.
    rb.isKinematic = true; 
    rb.useGravity = false; 
    
    rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

    // ✅ 충돌 영역 복구 (없으면 벽 통과함)
    CapsuleCollider capsule = GetComponent<CapsuleCollider>();
    if (capsule == null)
    {
        capsule = gameObject.AddComponent<CapsuleCollider>();
    }
    capsule.center = new Vector3(0, 1, 0);
    capsule.radius = 0.5f;
    capsule.height = 2f;
    capsule.isTrigger = false; // 충돌체 역할을 하도록 Trigger 비활성화
    
    // 문 감지용 Sphere Collider는 유지 (필요하다면)
    SphereCollider doorDetector = gameObject.AddComponent<SphereCollider>();
    doorDetector.isTrigger = true;
    doorDetector.radius = 2f;
    doorDetector.center = new Vector3(0, 1f, 0);
    // -------------------------------------------------------------

    FindReferences();
}

    // private void SetupColliders()
    // {
    //     // ✅ Rigidbody 추가/설정
    //     rb = GetComponent<Rigidbody>();
    //     if (rb == null)
    //     {
    //         rb = gameObject.AddComponent<Rigidbody>();
    //     }
    //     rb.isKinematic = false;
    //     rb.useGravity = false;
    //     rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

    //     // ✅ Capsule Collider 설정
    //     CapsuleCollider capsule = GetComponent<CapsuleCollider>();
    //     if (capsule == null)
    //     {
    //         capsule = gameObject.AddComponent<CapsuleCollider>();
    //     }
    //     capsule.center = new Vector3(0, 1, 0);
    //     capsule.radius = 0.5f;
    //     capsule.height = 2f;
    //     capsule.isTrigger = false;  // ✅ 물리 충돌 활성화!

    //     // ✅ 문 감지용 Sphere Collider
    //     SphereCollider doorDetector = gameObject.AddComponent<SphereCollider>();
    //     doorDetector.isTrigger = true;
    //     doorDetector.radius = 2f;
    //     doorDetector.center = new Vector3(0, 1f, 0);
    // }

    private void FindReferences()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("[AI] Player 찾음!");
            }
            else
            {
                Debug.LogError("[AI] Player를 찾을 수 없습니다!");
            }
        }
        
        suspicionDetector = GetComponent<PlayerSuspicionDetector>();
        if (suspicionDetector == null)
        {
            suspicionDetector = gameObject.AddComponent<PlayerSuspicionDetector>();
        }
        suspicionDetector.SetAI(this);
    }

    private void Update()
    {
        if (hasStunnedPlayer) return;

        switch (currentState)
        {
            case AIState.Idle:
                HandleIdle();
                break;
            case AIState.Approach:
                HandleApproach();
                break;
            case AIState.Watching:
                HandleWatching();
                break;
            case AIState.Chase:
                HandleChase();
                break;
            case AIState.Return:
                HandleReturn();
                break;
        }

        // ✅ Chase 상태에서만!
        if (currentState == AIState.Chase)
        {
            CheckCatchPlayer();
        }
    }

    private void HandleIdle()
    {
    }

    private void HandleApproach()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= watchDistance)
        {
            TransitionToWatching();
            return;
        }

        MoveTowardsTarget(player.position, approachSpeed);
    }

    private void HandleWatching()
    {
        if (player == null) return;

        watchTimer += Time.deltaTime;
        // --- 👇 누락된 핵심 로직 추가 👇 ---
    // (AIController가 SuspicionDetector를 가지고 있으니 GetSuspicionLevel()을 호출해야 함)
    if (suspicionDetector.GetSuspicionLevel() >= 100f)
    {
        TransitionToChase(); // 의심도 100 이상이면 추격!
        return;
    }
    // --- 👆 핵심 로직 추가 👆 ---

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > watchDistance * 1.5f)
        {
            MoveTowardsTarget(player.position, watchSpeed);
        }
        else if (distanceToPlayer < watchDistance * 0.5f)
        {
            Vector3 awayDirection = (transform.position - player.position).normalized;
            MoveInDirection(awayDirection, watchSpeed * 0.5f);
        }
        else
        {
            LookAtPlayer();
        }

        if (suspicionDetector.GetSuspicionLevel() >= 100f)
        {
            TransitionToChase();
            return;
        }

        if (watchTimer >= watchDuration && suspicionDetector.GetSuspicionLevel() < 30f)
        {
            TransitionToReturn();
        }
    }

    private void HandleChase()
    {
        if (player == null) return;

        MoveTowardsTarget(player.position, chaseSpeed);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange * 2f)
        {
            TransitionToReturn();
        }
    }

    private void HandleReturn()
    {
        Vector3 direction = (homePosition - transform.position).normalized;
        direction.y = 0;

        // ✅ Rigidbody 사용
        Vector3 movement = direction * returnSpeed;
        rb.MovePosition(rb.position + movement * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, smoothTurnSpeed * Time.deltaTime));
        }

        float distanceToHome = Vector3.Distance(transform.position, homePosition);
        if (distanceToHome < 1f)
        {
            currentState = AIState.Idle;
            rb.MoveRotation(homeRotation);

            if (showDebug)
            {
                Debug.Log("[AI] 원위치 복귀 완료");
            }
        }
    }

    private void MoveTowardsTarget(Vector3 targetPosition, float speed)
    {
        Vector3 currentPos = transform.position;

        if (Vector3.Distance(currentPos, lastPosition) < 0.1f)
        {
            stuckTime += Time.deltaTime;
        }
        else
        {
            stuckTime = 0f;
            lastPosition = currentPos;
        }

        Vector3 targetDirection = (targetPosition - transform.position).normalized;
        targetDirection.y = 0;

        Vector3 finalDirection = GetSmoothAvoidanceDirection(targetDirection);

        if (stuckTime > 1f)
        {
            finalDirection = GetRandomAvoidanceDirection();
            stuckTime = 0f;
        }

        if (finalDirection != Vector3.zero)
        {
            lastMoveDirection = Vector3.Slerp(
                lastMoveDirection,
                finalDirection,
                smoothTurnSpeed * Time.deltaTime
            );

            // ✅ Rigidbody 사용
            Vector3 movement = lastMoveDirection.normalized * speed;
            rb.MovePosition(rb.position + movement * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(lastMoveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, smoothTurnSpeed * Time.deltaTime));
        }
    }

    private void MoveInDirection(Vector3 direction, float speed)
    {
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            // ✅ Rigidbody 사용
            Vector3 movement = direction * speed;
            rb.MovePosition(rb.position + movement * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, smoothTurnSpeed * Time.deltaTime));
        }
    }

    private void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - player.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, smoothTurnSpeed * Time.deltaTime));
        }
    }

    private void CheckCatchPlayer()
    {
        if (player == null || hasStunnedPlayer) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= catchDistance)
        {
            StunPlayer();
        }
    }

    private void StunPlayer()
    {
        if (hasStunnedPlayer) return;

        hasStunnedPlayer = true;
        currentState = AIState.Stunning;

        if (showDebug)
        {
            Debug.Log("[AI] 플레이어 기절!");
        }

        PlayerStunHandler stunHandler = player.GetComponent<PlayerStunHandler>();
        if (stunHandler != null)
        {
            stunHandler.Stun(stunDuration);
        }

        StartCoroutine(StunSequence());
    }

    private IEnumerator StunSequence()
    {
        yield return new WaitForSeconds(stunDuration);

        hasStunnedPlayer = false;
        TransitionToReturn();
    }

    public void TransitionToApproach()
    {
        if (currentState == AIState.Idle || currentState == AIState.Return)
        {
            currentState = AIState.Approach;
            watchTimer = 0f;

            if (showDebug)
            {
                Debug.Log("<color=green>[AI] 접근 시작</color>");
            }
        }
        else
        {
            Debug.Log($"<color=red>[AI] 접근 불가 - 현재 상태: {currentState}</color>");
        }
    }

    private void TransitionToWatching()
    {
        currentState = AIState.Watching;
        watchTimer = 0f;
        suspicionDetector.ResetSuspicion();

        if (showDebug)
        {
            Debug.Log("<color=cyan>[AI] 감시 시작</color>");
        }
    }

    private void TransitionToChase()
    {
        currentState = AIState.Chase;

        if (showDebug)
        {
            Debug.Log("<color=red>[AI] 추격 시작!</color>");
        }
    }

    private void TransitionToReturn()
    {
        currentState = AIState.Return;
        watchTimer = 0f;
        suspicionDetector.ResetSuspicion();

        if (showDebug)
        {
            Debug.Log("<color=yellow>[AI] 복귀 시작</color>");
        }
    }

    private Vector3 GetSmoothAvoidanceDirection(Vector3 targetDirection)
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        float avoidanceWeight = 0f;
        Vector3 avoidanceDirection = Vector3.zero;

        if (Physics.Raycast(rayStart, targetDirection, obstacleDetectionDistance, obstacleLayerMask))
        {
            bool leftClear = !Physics.Raycast(rayStart, -transform.right, obstacleDetectionDistance * 0.8f, obstacleLayerMask);
            bool rightClear = !Physics.Raycast(rayStart, transform.right, obstacleDetectionDistance * 0.8f, obstacleLayerMask);

            if (leftClear && rightClear)
            {
                avoidanceDirection = Random.value > 0.5f ? -transform.right : transform.right;
            }
            else if (leftClear)
            {
                avoidanceDirection = -transform.right;
            }
            else if (rightClear)
            {
                avoidanceDirection = transform.right;
            }
            else
            {
                avoidanceDirection = -transform.forward * 0.5f + (Random.value > 0.5f ? -transform.right : transform.right);
            }

            avoidanceWeight = wallAvoidanceForce;
        }

        Vector3 finalDirection = (targetDirection + avoidanceDirection * avoidanceWeight).normalized;
        return finalDirection;
    }

    private Vector3 GetRandomAvoidanceDirection()
    {
        return Random.value > 0.5f ? transform.right : -transform.right;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleDoorInteraction(other);
    }

    private void OnTriggerStay(Collider other)
    {
        HandleDoorInteraction(other);
    }

    private void HandleDoorInteraction(Collider other)
    {
        SimpleDoor door = other.GetComponent<SimpleDoor>();
        if (door != null && !door.doorOpened)
        {
            door.OpenDoorNow();
            Debug.Log($"[AI] 문 열기: {other.name}");
        }
    }

    public AIState GetCurrentState()
    {
        return currentState;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, watchDistance);

        Gizmos.color = currentState == AIState.Chase ? Color.red : Color.gray;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(homePosition, 0.5f);
            Gizmos.DrawLine(transform.position, homePosition);
        }
    }
}