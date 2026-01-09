// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    // 게임 상태 enum 추가
    public enum GameState { Playing, Paused, GameOver }
    public static GameState State { get; private set; } = GameState.Playing;
    
    // 편의 속성들
    public static bool GameIsPaused => State == GameState.Paused;
    public static bool IsGameOver => State == GameState.GameOver;

    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Scene Names")]
    [SerializeField] private string mainSceneName = "MainMenu";
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
            return;
        }
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
        // 게임오버 상태면 ESC로 pause 못하게 막기
        if (State == GameState.GameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == GameState.Playing)
            {
                Pause();
            }
            else if (State == GameState.Paused)
            {
                // Settings가 켜져 있으면 설정 닫기
                if (settingsPanel != null && settingsPanel.activeSelf)
                    CloseSettings();
                else
                    Resume();
            }
        }
    }

    private void ResetGameState()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        SetCursorState(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void Pause()
    {
        State = GameState.Paused;
        Time.timeScale = 0f;
        SetCursorState(true);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void Resume()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        StartCoroutine(LockCursorNextFrame());
    }

    // 게임오버 함수 추가
    public void GameOver()
    {
        State = GameState.GameOver;
        Time.timeScale = 0f;
        SetCursorState(true);

        // pause 메뉴는 닫고, gameover 패널 표시
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    private System.Collections.IEnumerator LockCursorNextFrame()
    {
        yield return null;
        SetCursorState(false);
    }

    public void OnClickResume() => Resume();

    // 재시작 버튼용 (GameOver Panel에서 호출)
    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        State = GameState.Playing;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToSettingMenu()
    {
        SceneHistory.LastSceneName = SceneManager.GetActiveScene().name;
        SetCursorState(true);
        Time.timeScale = 1f;
        Instance = null;
        SceneManager.LoadScene(settingsSceneName);
    }

    public void OpenSettings()
    {
        // Pause 상태로 만들기
        State = GameState.Paused;
        Time.timeScale = 0f;

        SetCursorState(true);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        State = GameState.Paused;
        Time.timeScale = 0f;

        SetCursorState(true);
    }

    public void ToMain()
    {
        Time.timeScale = 1f;
        State = GameState.Playing;
        SetCursorState(true);
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
}