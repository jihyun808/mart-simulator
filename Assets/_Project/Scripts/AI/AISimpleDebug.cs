using UnityEngine;

/// <summary>
/// AI 시스템 콘솔 디버그 매니저 (간단 버전)
/// AI 상태 변화와 주요 이벤트를 콘솔에 로그로 출력
/// 
/// [수정 사항]
/// - Stunning 상태 제거 (NavMesh 버전에 맞춤)
/// </summary>
public class AISimpleDebug : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AIController aiController;
    [SerializeField] private PlayerSuspicionDetector suspicionDetector;
    [SerializeField] private Transform player;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebug = true;
    [SerializeField] private float logInterval = 1f; // 상태 로그 출력 주기 (초)

    [Header("Log Options")]
    [SerializeField] private bool logStateChanges = true;      // 상태 변경 로그
    [SerializeField] private bool logSuspicionChanges = true;  // 의심도 변경 로그
    [SerializeField] private bool logDistanceInfo = true;      // 거리 정보 로그
    [SerializeField] private bool logPeriodicStatus = true;    // 주기적 상태 로그

    private AIController.AIState lastState = AIController.AIState.Idle;
    private float lastSuspicion = 0f;
    private float logTimer = 0f;

    private void Start()
    {
        FindReferences();
        
        if (enableDebug)
        {
            Debug.Log("<color=cyan>========================================</color>");
            Debug.Log("<color=cyan>   AI Simple Debug System Started</color>");
            Debug.Log("<color=cyan>========================================</color>");
        }
    }

    private void FindReferences()
    {
        if (aiController == null)
            aiController = FindObjectOfType<AIController>();

        if (suspicionDetector == null && aiController != null)
            suspicionDetector = aiController.GetComponent<PlayerSuspicionDetector>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (aiController == null)
            Debug.LogError("[Debug] AIController를 찾을 수 없습니다!");
        if (player == null)
            Debug.LogError("[Debug] Player를 찾을 수 없습니다!");
    }

    private void Update()
    {
        if (!enableDebug || aiController == null) return;

        // 상태 변경 감지
        if (logStateChanges)
            CheckStateChange();

        // 의심도 변경 감지
        if (logSuspicionChanges && suspicionDetector != null)
            CheckSuspicionChange();

        // 주기적 상태 로그
        if (logPeriodicStatus)
        {
            logTimer += Time.deltaTime;
            if (logTimer >= logInterval)
            {
                LogPeriodicStatus();
                logTimer = 0f;
            }
        }
    }

    private void CheckStateChange()
    {
        AIController.AIState currentState = aiController.GetCurrentState();
        
        if (currentState != lastState)
        {
            LogStateChange(lastState, currentState);
            lastState = currentState;
        }
    }

    private void LogStateChange(AIController.AIState from, AIController.AIState to)
    {
        string fromColor = GetStateColor(from);
        string toColor = GetStateColor(to);
        
        Debug.Log($"[AI 상태 변경] <color={fromColor}>{from}</color> → <color={toColor}><b>{to}</b></color>");
        
        // 특별한 상태 전환에 대한 추가 정보
        switch (to)
        {
            case AIController.AIState.Approach:
                Debug.Log($"  └─ <color=green>플레이어 감지! 접근 시작</color>");
                break;
            case AIController.AIState.Watching:
                Debug.Log($"  └─ <color=cyan>감시 시작 (거리: {GetDistance():F2}m)</color>");
                break;
            case AIController.AIState.Chase:
                Debug.Log($"  └─ <color=red>⚠️ 추격 시작! 의심도 100 도달!</color>");
                break;
            case AIController.AIState.Return:
                Debug.Log($"  └─ <color=yellow>복귀 시작</color>");
                break;
            // ✅ Stunning 상태 제거됨
        }
    }

    private void CheckSuspicionChange()
    {
        float currentSuspicion = suspicionDetector.GetSuspicionLevel();
        
        // 의심도가 10 이상 변경되었을 때만 로그
        if (Mathf.Abs(currentSuspicion - lastSuspicion) >= 10f)
        {
            LogSuspicionChange(lastSuspicion, currentSuspicion);
            lastSuspicion = currentSuspicion;
        }
    }

    private void LogSuspicionChange(float from, float to)
    {
        string arrow = to > from ? "▲" : "▼";
        string color = GetSuspicionColor(to);
        
        Debug.Log($"[의심도 {arrow}] <color={color}>{from:F0} → {to:F0}</color> / 100");
        
        // 경고 메시지
        if (to >= 80f && from < 80f)
            Debug.LogWarning("  └─ ⚠️ 위험! 의심도 80 이상!");
        else if (to >= 50f && from < 50f)
            Debug.Log("  └─ ⚡ 주의! 의심도 50 이상");
    }

    private void LogPeriodicStatus()
    {
        AIController.AIState state = aiController.GetCurrentState();
        float distance = GetDistance();
        float suspicion = suspicionDetector != null ? suspicionDetector.GetSuspicionLevel() : 0f;
        
        string stateColor = GetStateColor(state);
        string suspicionColor = GetSuspicionColor(suspicion);
        
        Debug.Log($"[상태] <color={stateColor}>{state}</color> | " +
                  $"[거리] {distance:F2}m | " +
                  $"[의심도] <color={suspicionColor}>{suspicion:F0}</color>");
    }

    private float GetDistance()
    {
        if (aiController == null || player == null) return 0f;
        return Vector3.Distance(aiController.transform.position, player.position);
    }

    private string GetStateColor(AIController.AIState state)
    {
        switch (state)
        {
            case AIController.AIState.Idle: return "white";
            case AIController.AIState.Approach: return "green";
            case AIController.AIState.Watching: return "cyan";
            case AIController.AIState.Chase: return "red";
            case AIController.AIState.Return: return "yellow";
            // ✅ Stunning 제거
            default: return "white";
        }
    }

    private string GetSuspicionColor(float suspicion)
    {
        if (suspicion < 30f) return "green";
        else if (suspicion < 70f) return "yellow";
        else return "red";
    }

    // ====== 유용한 디버그 메서드 (Inspector에서 우클릭으로 호출 가능) ======

    [ContextMenu("현재 상태 출력")]
    public void PrintCurrentStatus()
    {
        if (aiController == null) return;

        Debug.Log("========== AI 현재 상태 ==========");
        Debug.Log($"상태: {aiController.GetCurrentState()}");
        Debug.Log($"의심도: {suspicionDetector?.GetSuspicionLevel() ?? 0f}");
        Debug.Log($"플레이어 거리: {GetDistance():F2}m");
        Debug.Log($"AI 위치: {aiController.transform.position}");
        Debug.Log("===================================");
    }

    [ContextMenu("의심도 리셋")]
    public void ResetSuspicion()
    {
        if (suspicionDetector != null)
        {
            suspicionDetector.ResetSuspicion();
            Debug.Log("<color=green>[Debug] 의심도 리셋 완료</color>");
        }
    }

    [ContextMenu("디버그 켜기/끄기")]
    public void ToggleDebug()
    {
        enableDebug = !enableDebug;
        Debug.Log($"<color=cyan>[Debug] 디버그 로그 {(enableDebug ? "활성화" : "비활성화")}</color>");
    }
}