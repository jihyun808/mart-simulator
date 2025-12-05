// PlayerSuspicionDetector.cs
using UnityEngine;

public class PlayerSuspicionDetector : MonoBehaviour
{
    [Header("Suspicion Settings")]
    [SerializeField] private float maxSuspicion = 100f;
    [SerializeField] private float suspicionDecayRate = 5f;

    [Header("Suspicion Increases")]
    [SerializeField] private float runSuspicionIncrease = 30f;
    [SerializeField] private float jumpSuspicionIncrease = 25f;
    [SerializeField] private float objectBreakSuspicion = 40f;

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
            Debug.Log("[Suspicion] Player 찾음!");
        }
        else
        {
            Debug.LogError("[Suspicion] Player를 찾을 수 없습니다! Player 태그 확인!");
        }
    }

    public void SetAI(AIController ai)
    {
        aiController = ai;
        Debug.Log("[Suspicion] AI 연결 완료!");
    }

    private void Update()
    {
        // ✅ AI 연결 확인
        if (aiController == null)
        {
            Debug.LogError("[Suspicion] AIController가 연결되지 않았습니다!");
            return;
        }

        DetectPlayerActions();
        DecaySuspicion();
    }

    private void DetectPlayerActions()
    {
        if (player == null) return;

        // ✅ 달리기 감지
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        
        if (isRunning && !wasRunning)
        {
            Debug.Log($"<color=yellow>[Suspicion] 달리기 시작! AI 상태: {aiController.GetCurrentState()}</color>");
            
            if (aiController.GetCurrentState() == AIController.AIState.Idle || 
                aiController.GetCurrentState() == AIController.AIState.Return)
            {
                Debug.Log("<color=green>[Suspicion] AI에게 접근 명령 전달!</color>");
                aiController.TransitionToApproach();
            }
            
            if (aiController.GetCurrentState() == AIController.AIState.Watching)
            {
                IncreaseSuspicion(runSuspicionIncrease);
            }
        }
        wasRunning = isRunning;

        // ✅ 점프 감지
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"<color=cyan>[Suspicion] 점프! AI 상태: {aiController.GetCurrentState()}</color>");
            
            if (aiController.GetCurrentState() == AIController.AIState.Watching)
            {
                IncreaseSuspicion(jumpSuspicionIncrease);
            }
        }
    }

    public void OnObjectBreak()
    {
        Debug.Log($"<color=red>[Suspicion] 물건 파손! AI 상태: {aiController.GetCurrentState()}</color>");
        
        if (aiController.GetCurrentState() == AIController.AIState.Idle || 
            aiController.GetCurrentState() == AIController.AIState.Return)
        {
            Debug.Log("<color=green>[Suspicion] AI에게 접근 명령 전달!</color>");
            aiController.TransitionToApproach();
        }

        if (aiController.GetCurrentState() == AIController.AIState.Watching)
        {
            IncreaseSuspicion(objectBreakSuspicion);
        }
    }

    private void IncreaseSuspicion(float amount)
    {
        if (aiController.GetCurrentState() != AIController.AIState.Watching)
            return;

        currentSuspicion = Mathf.Min(currentSuspicion + amount, maxSuspicion);
        Debug.Log($"<color=orange>[Suspicion] 의심도 증가! {currentSuspicion}/{maxSuspicion}</color>");
    }

    private void DecaySuspicion()
    {
        if (aiController.GetCurrentState() == AIController.AIState.Watching)
        {
            if (currentSuspicion > 0)
            {
                currentSuspicion = Mathf.Max(0, currentSuspicion - suspicionDecayRate * Time.deltaTime);
            }
        }
        else
        {
            currentSuspicion = 0f;
        }
    }

    public float GetSuspicionLevel()
    {
        return currentSuspicion;
    }

    public void ResetSuspicion()
    {
        currentSuspicion = 0f;
    }
}