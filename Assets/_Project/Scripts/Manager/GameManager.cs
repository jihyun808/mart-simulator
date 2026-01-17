// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // ==================== Singleton ====================
    public static GameManager Instance { get; private set; }

    // ==================== Game State ====================
    public enum GameState { Playing, Paused, GameOver, Clear }
    public static GameState State { get; private set; } = GameState.Playing;

    // 편의 속성들
    public static bool GameIsPaused => State == GameState.Paused;
    public static bool IsGameOver => State == GameState.GameOver;
    public static bool IsGameClear => State == GameState.Clear;
    public static bool IsGameStopped => State == GameState.GameOver || State == GameState.Clear;

    // ==================== UI Panels ====================
    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Clear Panel Text")]
    [SerializeField] private TMPro.TextMeshProUGUI clearStageText;

    // ==================== Scene Settings ====================
    [Header("Scene Names")]
    [SerializeField] private string mainSceneName = "MainMenu";
    [SerializeField] private string settingsSceneName = "Settings";

    // ==================== Unity Lifecycle ====================
    private void Awake()
    {
        InitializeSingleton();
    }

    private void OnEnable()
    {
        ResetGameState();
    }

    private void Start()
    {
        ResetGameState();
    }

    private void Update()
    {
        HandleEscapeInput();
        
        // 🔥 강제 디버깅: Clear/GameOver 상태 체크
        if (State == GameState.Clear || State == GameState.GameOver)
        {
            // Time.timeScale이 0이 아니면 강제로 0으로
            if (Time.timeScale != 0f)
            {
                Debug.LogError($"⚠️⚠️⚠️ {State} 상태인데 Time.timeScale = {Time.timeScale}! 강제로 0 설정!");
                Time.timeScale = 0f;
            }
            
            // 커서가 안 보이면 강제로 보이게
            if (!Cursor.visible)
            {
                Debug.LogError("⚠️⚠️⚠️ 커서가 안 보임! 강제로 보이게 설정!");
                SetCursorState(true);
            }
        }
    }

    // ==================== Initialization ====================
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ResetGameState()
    {
        Debug.Log("🔄 ResetGameState 호출됨");
        State = GameState.Playing;
        Time.timeScale = 1f;
        SetCursorState(false);
        DeactivateAllPanels();
        Debug.Log("✅ ResetGameState 완료");
    }

    private void DeactivateAllPanels()
    {
        Debug.Log("--- 모든 패널 비활성화 시작 ---");
        
        if (pauseMenuPanel != null)
            Debug.Log($"PauseMenuPanel: {pauseMenuPanel.activeSelf} → false");
        SetPanelActive(pauseMenuPanel, false);
        
        if (gameOverPanel != null)
            Debug.Log($"GameOverPanel: {gameOverPanel.activeSelf} → false");
        SetPanelActive(gameOverPanel, false);
        
        if (clearPanel != null)
            Debug.Log($"ClearPanel: {clearPanel.activeSelf} → false");
        SetPanelActive(clearPanel, false);
        
        if (settingsPanel != null)
            Debug.Log($"SettingsPanel: {settingsPanel.activeSelf} → false");
        SetPanelActive(settingsPanel, false);
        
        Debug.Log("--- 모든 패널 비활성화 완료 ---");
    }

    // ==================== Input Handling ====================
    private void HandleEscapeInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // 게임 종료 상태에서는 ESC 무시
        if (IsGameStopped) return;

        if (State == GameState.Playing)
        {
            Pause();
        }
        else if (State == GameState.Paused)
        {
            HandlePausedEscape();
        }
    }

    private void HandlePausedEscape()
    {
        // Settings가 열려있으면 Settings 닫기
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
        }
        else
        {
            Resume();
        }
    }

    // ==================== Game State Control ====================
    public void Pause()
    {
        State = GameState.Paused;
        StopGame();
        SetPanelActive(pauseMenuPanel, true);
    }

    public void Resume()
    {
        State = GameState.Playing;
        ResumeGame();
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, false);
        StartCoroutine(LockCursorNextFrame());
    }

    public void GameOver()
    {
        Debug.Log("========================================");
        Debug.Log("🎮 GameOver() 호출됨!");
        Debug.Log($"현재 State: {State}");
        
        State = GameState.GameOver;
        Debug.Log($"변경된 State: {State}");
        
        StopGame();
        Debug.Log($"Time.timeScale: {Time.timeScale}");
        Debug.Log($"Cursor.visible: {Cursor.visible}");
        Debug.Log($"Cursor.lockState: {Cursor.lockState}");
        
        Debug.Log("--- 패널 상태 변경 시작 ---");
        
        if (pauseMenuPanel != null)
        {
            Debug.Log($"PauseMenuPanel 끄기 전: {pauseMenuPanel.activeSelf}");
            pauseMenuPanel.SetActive(false);
            Debug.Log($"PauseMenuPanel 끈 후: {pauseMenuPanel.activeSelf}");
        }
        
        if (gameOverPanel != null)
        {
            Debug.Log($"GameOverPanel 켜기 전: {gameOverPanel.activeSelf}");
            gameOverPanel.SetActive(true);
            Debug.Log($"GameOverPanel 켠 후: {gameOverPanel.activeSelf}");
            Debug.Log($"GameOverPanel 이름: {gameOverPanel.name}");
            Debug.Log($"GameOverPanel Transform: {gameOverPanel.transform.position}");
        }
        else
        {
            Debug.LogError("❌❌❌ gameOverPanel이 NULL입니다! Inspector에서 연결 확인 필요! ❌❌❌");
        }
        
        Debug.Log("========================================");
    }

    public void GameClear()
    {
        GameClear(1); // 기본값 스테이지 1
    }
    
    public void GameClear(int stageNumber)
    {
        Debug.Log("========================================");
        Debug.Log($"🎉 GameClear() 호출됨! Stage: {stageNumber}");
        Debug.Log($"현재 State: {State}");
        
        State = GameState.Clear;
        Debug.Log($"변경된 State: {State}");
        
        StopGame();
        Debug.Log($"Time.timeScale: {Time.timeScale}");
        Debug.Log($"Cursor.visible: {Cursor.visible}");
        Debug.Log($"Cursor.lockState: {Cursor.lockState}");
        
        Debug.Log("--- 패널 상태 변경 시작 ---");
        
        if (pauseMenuPanel != null)
        {
            Debug.Log($"PauseMenuPanel 끄기 전: {pauseMenuPanel.activeSelf}");
            pauseMenuPanel.SetActive(false);
            Debug.Log($"PauseMenuPanel 끈 후: {pauseMenuPanel.activeSelf}");
        }
        
        if (gameOverPanel != null)
        {
            Debug.Log($"GameOverPanel 끄기 전: {gameOverPanel.activeSelf}");
            gameOverPanel.SetActive(false);
            Debug.Log($"GameOverPanel 끈 후: {gameOverPanel.activeSelf}");
        }
        
        // Clear Text 업데이트
        if (clearStageText != null)
        {
            clearStageText.text = $"STAGE {stageNumber} CLEAR!";
            Debug.Log($"ClearText 업데이트: STAGE {stageNumber} CLEAR!");
        }
        else
        {
            Debug.LogWarning("⚠️ clearStageText가 연결 안 됨!");
        }
        
        if (clearPanel != null)
        {
            Debug.Log($"ClearPanel 켜기 전: {clearPanel.activeSelf}");
            clearPanel.SetActive(true);
            Debug.Log($"ClearPanel 켠 후: {clearPanel.activeSelf}");
            Debug.Log($"ClearPanel 이름: {clearPanel.name}");
            Debug.Log($"ClearPanel Transform: {clearPanel.transform.position}");
        }
        else
        {
            Debug.LogError("❌❌❌ clearPanel이 NULL입니다! Inspector에서 연결 확인 필요! ❌❌❌");
        }
        
        Debug.Log("========================================");
    }

    // ==================== Settings Management ====================
    public void OpenSettings()
    {
        State = GameState.Paused;
        StopGame();
        
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, true);
    }

    public void CloseSettings()
    {
        SetPanelActive(settingsPanel, false);
        SetPanelActive(pauseMenuPanel, true);
        
        State = GameState.Paused;
        StopGame();
    }

    // ==================== Scene Management ====================
    public void OnClickRestart()
    {
        PrepareSceneTransition();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void OnClickNextStage()
    {
        PrepareSceneTransition();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ToSettingMenu()
    {
        SceneHistory.LastSceneName = SceneManager.GetActiveScene().name;
        PrepareSceneTransition();
        Instance = null;
        SceneManager.LoadScene(settingsSceneName);
    }

    public void ToMain()
    {
        PrepareSceneTransition();
        Instance = null;
        SceneManager.LoadScene(mainSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ==================== Helper Methods ====================
    private void StopGame()
    {
        Time.timeScale = 0f;
        SetCursorState(true);
        Debug.Log($"🛑 StopGame 실행 - Time.timeScale: {Time.timeScale}, Cursor.visible: {Cursor.visible}");
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    private void PrepareSceneTransition()
    {
        Time.timeScale = 1f;
        State = GameState.Playing;
        SetCursorState(true);
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private void LogPanelState(string panelName, GameObject panel)
    {
        if (panel != null)
        {
            Debug.Log($"✅ {panelName} 활성화 시도 - activeSelf: {panel.activeSelf}");
        }
        else
        {
            Debug.LogError($"❌ {panelName}이 NULL입니다! Inspector에서 연결 확인 필요!");
        }
    }

    private IEnumerator LockCursorNextFrame()
    {
        yield return null;
        SetCursorState(false);
    }

    // ==================== Public Button Callbacks ====================
    public void OnClickResume() => Resume();
}