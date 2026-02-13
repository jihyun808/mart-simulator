using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 게임 전체 상태 관리 (일시정지, 게임오버, 클리어 등)
/// Singleton 패턴 사용 - Scene당 하나만 존재
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, GameOver, Clear }
    public static GameState State { get; private set; } = GameState.Playing;

    // 외부에서 상태 체크용
    public static bool GameIsPaused => State == GameState.Paused;
    public static bool IsGameOver => State == GameState.GameOver;
    public static bool IsGameClear => State == GameState.Clear;
    public static bool IsGameStopped => State == GameState.GameOver || State == GameState.Clear;

    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Clear Panel")]
    [SerializeField] private TMPro.TextMeshProUGUI clearStageText;

    [Header("Scene Names")]
    [SerializeField] private string mainSceneName = "MainMenu";
    [SerializeField] private string nextStageSceneName = "Stage-2";
    [SerializeField] private string settingsSceneName = "Settings";

    private void Awake()
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

    private void Start()
    {
        ResetGameState();
    }

    private void Update()
    {
        HandleEscapeInput();
        EnforceStoppedState();
    }

    // 게임 시작 시 초기 상태로 리셋
    private void ResetGameState()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        SetCursorState(false);
        DeactivateAllPanels();
    }

    private void DeactivateAllPanels()
    {
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(clearPanel, false);
        SetPanelActive(settingsPanel, false);
    }

    // GameOver/Clear 상태에서 Time.timeScale과 커서 강제 유지
    private void EnforceStoppedState()
    {
        if (IsGameStopped)
        {
            if (Time.timeScale != 0f) Time.timeScale = 0f;
            if (!Cursor.visible) SetCursorState(true);
        }
    }

    // ESC 키 입력 처리
    private void HandleEscapeInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || IsGameStopped) return;

        if (State == GameState.Playing)
        {
            Pause();
        }
        else if (State == GameState.Paused)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                Resume();
            }
        }
    }

    // ========== Public Methods - UI 버튼에서 호출 ==========
    
    /// <summary>일시정지 (ESC 또는 버튼)</summary>
    public void Pause()
    {
        State = GameState.Paused;
        StopGame();
        SetPanelActive(pauseMenuPanel, true);
    }

    /// <summary>게임 재개</summary>
    public void Resume()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, false);
        StartCoroutine(LockCursorNextFrame());
    }

    /// <summary>게임 오버 처리</summary>
    public void GameOver()
    {
        State = GameState.GameOver;
        StopGame();
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(gameOverPanel, true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SFXType.GameOver);
        }
    }

    /// <summary>스테이지 클리어 (기본 Stage 1)</summary>
    public void GameClear()
    {
        GameClear(1);
    }
    
    /// <summary>스테이지 클리어 (스테이지 번호 지정)</summary>
    public void GameClear(int stageNumber)
    {
        State = GameState.Clear;
        StopGame();
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(gameOverPanel, false);
        
        if (clearStageText != null)
        {
            clearStageText.text = $"STAGE {stageNumber} CLEAR!";
        }
        
        SetPanelActive(clearPanel, true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SFXType.GameClear);
        }
    }

    /// <summary>설정 창 열기</summary>
    public void OpenSettings()
    {
        State = GameState.Paused;
        StopGame();
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, true);
    }

    /// <summary>설정 창 닫기</summary>
    public void CloseSettings()
    {
        SetPanelActive(settingsPanel, false);
        SetPanelActive(pauseMenuPanel, true);
    }

    // ========== Scene 전환 메서드 ==========
    
    /// <summary>현재 씬 재시작</summary>
    public void OnClickRestart()
    {
        PrepareSceneTransition();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>다음 스테이지로 이동</summary>
    public void OnClickNextStage()
    {
        PrepareSceneTransition();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /// <summary>설정 메뉴로 이동 (현재 씬 이름 저장)</summary>
    public void ToSettingMenu()
    {
        SceneHistory.LastSceneName = SceneManager.GetActiveScene().name;
        PrepareSceneTransition();
        Instance = null;
        SceneManager.LoadScene(settingsSceneName);
    }

    /// <summary>메인 메뉴로 이동</summary>
    public void ToMain()
    {
        PrepareSceneTransition();
        Instance = null;
        SceneManager.LoadScene(mainSceneName);
    }

    /// <summary>게임 종료</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ========== Private Helper Methods ==========
    
    // 게임 정지 (Time.timeScale = 0, 커서 표시)
    private void StopGame()
    {
        Time.timeScale = 0f;
        SetCursorState(true);
    }

    // 씬 전환 준비 (상태 초기화)
    private void PrepareSceneTransition()
    {
        Time.timeScale = 1f;
        State = GameState.Playing;
        SetCursorState(true);
    }

    // 커서 표시/잠금 상태 설정
    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // 패널 활성화/비활성화 (null 체크 포함)
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    // 다음 프레임에 커서 잠금 (Resume 시 사용)
    private IEnumerator LockCursorNextFrame()
    {
        yield return null;
        SetCursorState(false);
    }
}