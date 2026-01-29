using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMesh 기반 AI 직원 행동 제어
/// 벽을 돌아서 문으로 접근하는 똑똑한 AI
/// 
/// [버그 수정]
/// 1. 문 자동 열기 기능 추가
/// 2. NavMeshObstacle 자동 처리
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
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

    [Header("Stun Settings")]
    public float stunDuration = 3f;

    [Header("NavMesh Settings")]
    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private float doorDetectionRadius = 2f; // ✅ 문 감지 반경

    public enum AIState
    {
        Idle,
        Approach,
        Watching,
        Chase,
        Return
    }

    private AIState currentState = AIState.Idle;
    private Vector3 homePosition;
    private Quaternion homeRotation;
    private PlayerSuspicionDetector suspicionDetector;
    private NavMeshAgent agent;
    private float watchTimer = 0f;
    private bool hasStunnedPlayer = false;
    private Animator anim;

    private void Start()
    {
        homePosition = transform.position;
        homeRotation = transform.rotation;
        anim = GetComponent<Animator>();

        SetupNavMesh();
        FindReferences();
    }

    private void SetupNavMesh()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        agent.speed = approachSpeed;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // ✅ Collider 추가 (문 감지용)
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0, 1, 0);
            capsule.radius = 0.5f;
            capsule.height = 2f;
            capsule.isTrigger = false;
        }
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
        if (hasStunnedPlayer)
        {
            agent.isStopped = true;
            if (anim != null) anim.SetFloat("Speed", 0);
            return;
        }

        // ✅ 매 프레임 문 체크
        CheckAndOpenNearbyDoors();

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
        
        UpdateAnimation();
    }

    // ✅ 주변 문 자동 열기
    private void CheckAndOpenNearbyDoors()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, doorDetectionRadius);
        
        foreach (Collider col in colliders)
        {
            SimpleDoor door = col.GetComponent<SimpleDoor>();
            if (door != null && !door.doorOpened)
            {
                door.OpenDoorNow();
                Debug.Log($"<color=cyan>[AI] 문 열기: {col.name}</color>");
            }
        }
    }

    private void UpdateAnimation()
    {
        if (anim == null) return;

        float currentSpeed = agent.velocity.magnitude;
        anim.SetFloat("Speed", currentSpeed);
    }

    private void HandleIdle()
    {
        agent.isStopped = true;
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

        agent.isStopped = false;
        agent.speed = approachSpeed;
        agent.SetDestination(player.position);
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
            agent.isStopped = false;
            agent.speed = watchSpeed;
            agent.SetDestination(player.position);
        }
        else if (distanceToPlayer < watchDistance * 0.5f)
        {
            Vector3 awayDirection = (transform.position - player.position).normalized;
            Vector3 retreatPosition = transform.position + awayDirection * 1f;
            agent.isStopped = false;
            agent.speed = watchSpeed * 0.5f;
            agent.SetDestination(retreatPosition);
        }
        else
        {
            agent.isStopped = true;
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

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange * 2f)
        {
            TransitionToReturn();
        }
    }

    private void HandleReturn()
    {
        agent.isStopped = false;
        agent.speed = returnSpeed;
        agent.SetDestination(homePosition);

        float distanceToHome = Vector3.Distance(transform.position, homePosition);
        if (distanceToHome < 1f)
        {
            currentState = AIState.Idle;
            transform.rotation = homeRotation;
            agent.isStopped = true;
            Debug.Log("<color=green>[AI] 원위치 복귀 완료</color>");
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
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
        agent.isStopped = true;
        Debug.Log("<color=magenta>[AI] 플레이어 기절!</color>");

        PlayerStunHandler stunHandler = player.GetComponent<PlayerStunHandler>();
        if (stunHandler != null)
        {
            stunHandler.Stun(stunDuration);
        }

        StartCoroutine(StunAndReturn());
    }

    private IEnumerator StunAndReturn()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("<color=yellow>[AI] 즉시 복귀 시작</color>");
        
        hasStunnedPlayer = false;
        TransitionToReturn();
    }

    public void TransitionToApproach()
    {
        if (currentState == AIState.Idle || currentState == AIState.Return)
        {
            currentState = AIState.Approach;
            watchTimer = 0f;
            Debug.Log("<color=green>[AI] Approach 시작</color>");
        }
    }

    private void TransitionToWatching()
    {
        currentState = AIState.Watching;
        watchTimer = 0f;
        suspicionDetector.ResetSuspicion();
        Debug.Log("<color=cyan>[AI] Watching 시작 (의심도 리셋)</color>");
    }

    private void TransitionToChase()
    {
        currentState = AIState.Chase;
        Debug.Log("<color=red>[AI] Chase 시작!</color>");
    }

    private void TransitionToReturn()
    {
        currentState = AIState.Return;
        watchTimer = 0f;
        suspicionDetector.ResetSuspicion();
        Debug.Log("<color=yellow>[AI] Return 시작</color>");
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

        // ✅ 문 감지 범위
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, doorDetectionRadius);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(homePosition, 0.5f);
            Gizmos.DrawLine(transform.position, homePosition);

            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent != null && agent.hasPath)
            {
                Gizmos.color = Color.cyan;
                Vector3[] path = agent.path.corners;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }
    }
}