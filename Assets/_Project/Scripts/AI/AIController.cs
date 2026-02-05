using System.Collections;
using UnityEngine;

/// <summary>
/// AI 직원 행동 제어 (접근, 감시, 추격, 복귀)
/// 플레이어의 수상한 행동을 감지하고 대응
/// </summary>
public class AIController : MonoBehaviour
{
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
    private Vector3 homePosition;
    private Quaternion homeRotation;
    private PlayerSuspicionDetector suspicionDetector;
    private Rigidbody rb;
    private Vector3 lastMoveDirection = Vector3.forward;
    private float stuckTime = 0f;
    private Vector3 lastPosition;
    private float watchTimer = 0f;
    private bool hasStunnedPlayer = false;

    private void Start()
    {
        homePosition = transform.position;
        homeRotation = transform.rotation;
        lastPosition = transform.position;
        lastMoveDirection = transform.forward;

        SetupPhysics();
        FindReferences();
    }

    private void SetupPhysics()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            capsule = gameObject.AddComponent<CapsuleCollider>();
        }
        capsule.center = new Vector3(0, 1, 0);
        capsule.radius = 0.5f;
        capsule.height = 2f;
        capsule.isTrigger = false;

        SphereCollider doorDetector = gameObject.AddComponent<SphereCollider>();
        doorDetector.isTrigger = true;
        doorDetector.radius = 2f;
        doorDetector.center = new Vector3(0, 1f, 0);
    }

    private void FindReferences()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
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

        UpdateMovementSound();

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

        if (currentState == AIState.Chase)
        {
            CheckCatchPlayer();
        }
    }

    private void UpdateMovementSound()
    {
        if (AudioManager.Instance == null) return;

        if (currentState == AIState.Chase)
        {
            AudioManager.Instance.PlayLoopSFX(SFXType.AIRun);
        }
        else if (currentState == AIState.Approach || currentState == AIState.Return)
        {
            AudioManager.Instance.PlayLoopSFX(SFXType.AIWalk);
        }
        else
        {
            AudioManager.Instance.StopLoopSFX();
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

        if (suspicionDetector.GetSuspicionLevel() >= 100f)
        {
            TransitionToChase();
            return;
        }

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
            Vector3 movement = direction * speed;
            rb.MovePosition(rb.position + movement * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, smoothTurnSpeed * Time.deltaTime));
        }
    }

    private void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
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

    /// <summary>접근 상태로 전이 (PlayerSuspicionDetector에서 호출)</summary>
    public void TransitionToApproach()
    {
        if (currentState == AIState.Idle || currentState == AIState.Return)
        {
            currentState = AIState.Approach;
            watchTimer = 0f;
        }
    }

    private void TransitionToWatching()
    {
        currentState = AIState.Watching;
        watchTimer = 0f;
        suspicionDetector.ResetSuspicion();
    }

    private void TransitionToChase()
    {
        currentState = AIState.Chase;
    }

    private void TransitionToReturn()
    {
        currentState = AIState.Return;
        watchTimer = 0f;
        suspicionDetector.ResetSuspicion();
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