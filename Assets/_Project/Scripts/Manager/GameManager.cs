// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool GameIsPaused { get; private set; } = false;

    [Header("UI")]
    [SerializeField] private GameObject pauseMenuPanel;

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                SetCursorState(false);
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    private void ResetGameState()
    {
        GameIsPaused = false;
        Time.timeScale = 1f;
        SetCursorState(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void Pause()
    {
        GameIsPaused = true;
        Time.timeScale = 0f;
        SetCursorState(true);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void Resume()
    {
        GameIsPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        StartCoroutine(LockCursorNextFrame());
    }

    private System.Collections.IEnumerator LockCursorNextFrame()
    {
        yield return null;
        SetCursorState(false);
    }

    public void OnClickResume() => Resume();

    public void ToSettingMenu()
    {
        SceneHistory.LastSceneName = SceneManager.GetActiveScene().name;
        SetCursorState(true);
        Time.timeScale = 1f;
        Instance = null;
        SceneManager.LoadScene(settingsSceneName);
    }

    public void ToMain()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
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