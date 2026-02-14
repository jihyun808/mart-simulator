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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!isWandering || !agent.enabled)
            return;

        // 도착했을 때만 idle 시작
        if (!agent.pathPending &&
            (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance))
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= currentIdleTime)
            {
                TryMoveToRandomPoint();
            }
        }
    }

    public void StartWander()
    {
        isWandering = true;

        idleTimer = 0f;
        currentIdleTime = Random.Range(idleTimeMin, idleTimeMax);

        agent.isStopped = false;

        TryMoveToRandomPoint();
    }

    public void StopWander()
    {
        isWandering = false;
        agent.ResetPath();
        agent.isStopped = true;
    }

    private void TryMoveToRandomPoint()
    {
        Vector3 randomPoint;

        if (!TryGetRandomNavMeshPoint(transform.position, wanderRadius, out randomPoint))
            return;

        float dist = Vector3.Distance(transform.position, randomPoint);

        // 너무 가까우면 그냥 다시 시도 (idle 유지)
        if (dist <= 1.0f)
            return;

        agent.SetDestination(randomPoint);

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
