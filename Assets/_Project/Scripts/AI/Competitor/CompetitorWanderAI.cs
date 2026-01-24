using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CompetitorWanderAI : MonoBehaviour
{
    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float idleTimeMin = 1.5f;
    [SerializeField] private float idleTimeMax = 3.5f;

    private NavMeshAgent agent;

    private float idleTimer;
    private float currentIdleTime;
    private bool isWandering = false;

    private Vector3 originPosition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originPosition = transform.position;
    }

    private void Update()
    {
        if (!isWandering || !agent.enabled) return;

        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= currentIdleTime)
            {
                MoveToRandomPoint();
            }
        }
    }

    // ─────────────────────────────
    // 외부 제어 (Controller에서 호출)
    // ─────────────────────────────

public void StartWander()
{
    Debug.Log("[Wander] StartWander called");
    Debug.Log($"[Wander] isOnNavMesh = {agent.isOnNavMesh}");

    isWandering = true;
    idleTimer = 0f;
    currentIdleTime = Random.Range(idleTimeMin, idleTimeMax);

    agent.isStopped = false;
    MoveToRandomPoint();
}



    public void StopWander()
    {
        isWandering = false;
        agent.ResetPath();
        agent.isStopped = true;
    }

    // ─────────────────────────────
    // 내부 로직
    // ─────────────────────────────

private void MoveToRandomPoint()
{
    Vector3 randomPoint;

    if (TryGetRandomNavMeshPoint(originPosition, wanderRadius, out randomPoint))
    {
        Debug.Log($"[Wander] MoveTo {randomPoint}");
        agent.SetDestination(randomPoint);
    }
    else
    {
        Debug.LogWarning("[Wander] Failed to find NavMesh point");
    }

    idleTimer = 0f;
    currentIdleTime = Random.Range(idleTimeMin, idleTimeMax);
}

    private bool TryGetRandomNavMeshPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;
            randomPos.y = center.y;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
#endif
}
