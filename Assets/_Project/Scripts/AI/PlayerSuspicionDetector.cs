using UnityEngine;

/// <summary>
/// 플레이어의 수상한 행동 감지 (달리기, 점프, 물건 파손)
/// 
/// [버그 수정]
/// 1. Idle 상태에서 행동 감지 범위 제한 (detectionRange 내에서만)
/// 2. IncreaseSuspicion에서 상태 체크 제거 (모든 상태에서 증가 가능)
/// </summary>
public class PlayerSuspicionDetector : MonoBehaviour
{
    [Header("Suspicion Settings")]
    [SerializeField] private float maxSuspicion = 100f;
    [SerializeField] private float approachThreshold = 30f;
    [SerializeField] private float suspicionDecayRate = 5f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;

    [Header("Suspicion Increases - Idle/Return")]
    [SerializeField] private float idleRunSuspicionIncrease = 15f;
    [SerializeField] private float idleJumpSuspicionIncrease = 10f;
    [SerializeField] private float idleObjectBreakSuspicion = 20f;

    [Header("Suspicion Increases - Watching")]
    [SerializeField] private float watchRunSuspicionIncrease = 30f;
    [SerializeField] private float watchJumpSuspicionIncrease = 25f;
    [SerializeField] private float watchObjectBreakSuspicion = 40f;

    private float currentSuspicion = 0f;
    private Transform player;
    private bool wasRunning = false;
    private AIController aiController;

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    public void SetAI(AIController ai)
    {
        aiController = ai;
    }

    private void Update()
    {
        if (aiController == null) return;

        DetectPlayerActions();
        DecaySuspicion();
        CheckApproachThreshold();
    }

    private void DetectPlayerActions()
    {
        if (player == null) return;

        AIController.AIState currentState = aiController.GetCurrentState();
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float distanceToPlayer = Vector3.Distance(aiController.transform.position, player.position);
        bool inDetectionRange = distanceToPlayer <= detectionRange;

        if (isRunning && !wasRunning)
        {
            if (currentState == AIController.AIState.Idle || 
                currentState == AIController.AIState.Return)
            {
                if (inDetectionRange) 
                {
                    IncreaseSuspicion(idleRunSuspicionIncrease);
                    Debug.Log($"<color=yellow>[Suspicion] 달리기 감지 (Idle) → 의심도 +{idleRunSuspicionIncrease} (현재: {currentSuspicion:F0})</color>");
                }
            }
            else if (currentState == AIController.AIState.Watching)
            {
                IncreaseSuspicion(watchRunSuspicionIncrease);
                Debug.Log($"<color=yellow>[Suspicion] 달리기 감지 (Watching) → 의심도 +{watchRunSuspicionIncrease} (현재: {currentSuspicion:F0})</color>");
            }
        }

        if (isRunning && currentState == AIController.AIState.Watching)
        {
            IncreaseSuspicion(10f * Time.deltaTime);
        }

        wasRunning = isRunning;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentState == AIController.AIState.Idle || 
                currentState == AIController.AIState.Return)
            {
                if (inDetectionRange) 
                {
                    IncreaseSuspicion(idleJumpSuspicionIncrease);
                    Debug.Log($"<color=yellow>[Suspicion] 점프 감지 (Idle) → 의심도 +{idleJumpSuspicionIncrease} (현재: {currentSuspicion:F0})</color>");
                }
            }
            else if (currentState == AIController.AIState.Watching)
            {
                IncreaseSuspicion(watchJumpSuspicionIncrease);
                Debug.Log($"<color=yellow>[Suspicion] 점프 감지 (Watching) → 의심도 +{watchJumpSuspicionIncrease} (현재: {currentSuspicion:F0})</color>");
            }
        }
    }

    public void OnObjectBreak()
    {
        AIController.AIState currentState = aiController.GetCurrentState();
        float distanceToPlayer = Vector3.Distance(aiController.transform.position, player.position);
        bool inDetectionRange = distanceToPlayer <= detectionRange;

        if (currentState == AIController.AIState.Idle || 
            currentState == AIController.AIState.Return)
        {
            if (inDetectionRange)
            {
                IncreaseSuspicion(idleObjectBreakSuspicion);
                Debug.Log($"<color=yellow>[Suspicion] 물건 파손 감지 (Idle) → 의심도 +{idleObjectBreakSuspicion} (현재: {currentSuspicion:F0})</color>");
            }
        }
        else if (currentState == AIController.AIState.Watching)
        {
            IncreaseSuspicion(watchObjectBreakSuspicion);
            Debug.Log($"<color=yellow>[Suspicion] 물건 파손 감지 (Watching) → 의심도 +{watchObjectBreakSuspicion} (현재: {currentSuspicion:F0})</color>");
        }
    }

    // ✅ 수정: 상태 체크 제거 (모든 상태에서 증가 가능)
    private void IncreaseSuspicion(float amount)
    {
        currentSuspicion = Mathf.Min(currentSuspicion + amount, maxSuspicion);

        if (currentSuspicion >= maxSuspicion)
        {
            Debug.LogWarning($"<color=red>[Suspicion] ⚠️ 의심도 MAX! ({currentSuspicion:F0})</color>");
        }
    }

    private void CheckApproachThreshold()
    {
        AIController.AIState currentState = aiController.GetCurrentState();

        if (currentState == AIController.AIState.Idle || 
            currentState == AIController.AIState.Return)
        {
            if (currentSuspicion >= approachThreshold)
            {
                aiController.TransitionToApproach();
                Debug.Log($"<color=green>[Suspicion] 의심도 {approachThreshold} 도달 → Approach 시작</color>");
            }
        }
    }

    private void DecaySuspicion()
    {
        AIController.AIState currentState = aiController.GetCurrentState();

        if (currentState == AIController.AIState.Watching)
        {
            if (currentSuspicion > 0 && currentSuspicion < maxSuspicion)
            {
                currentSuspicion = Mathf.Max(0, currentSuspicion - suspicionDecayRate * Time.deltaTime);
            }
        }
        else if (currentState == AIController.AIState.Idle || 
                 currentState == AIController.AIState.Return)
        {
            if (currentSuspicion > 0)
            {
                currentSuspicion = Mathf.Max(0, currentSuspicion - (suspicionDecayRate * 0.5f) * Time.deltaTime);
            }
        }
    }

    public float GetSuspicionLevel()
    {
        return currentSuspicion;
    }

    public void ResetSuspicion()
    {
        currentSuspicion = 0f;
        Debug.Log("<color=cyan>[Suspicion] 의심도 리셋 (Watching 진입)</color>");
    }
}