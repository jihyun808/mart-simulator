using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 전역 일시정지 상태
    public static bool GameIsPaused { get; private set; } = false;

    [Header("UI")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("이동할 씬 이름들")]
    [SerializeField] private string mainSceneName     = "MainMenu";
    [SerializeField] private string settingsSceneName = "Settings";

    private void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.LogWarning($"@@@ [{sceneName}] GameManager Awake 시작 @@@");
        
        if (Instance == null)
        {
            Instance = this;
            Debug.LogWarning($"@@@ [{sceneName}] GameManager Instance 생성 완료 @@@");
        }
        else
        {
            Debug.LogError($"@@@ [{sceneName}] GameManager 중복 감지 - 파괴됨! @@@");
            Destroy(gameObject);
            return;
        }
        
        Debug.LogWarning($"@@@ [{sceneName}] GameManager Awake 종료 @@@");
    }

    private void OnEnable()
    {
        // 🔹 씬이 로드될 때마다 강제로 초기화
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[{sceneName}] GameManager OnEnable - 강제 초기화 시작");
        
        GameIsPaused = false;
        Time.timeScale = 1f;
        
        Debug.Log($"[{sceneName}] Time.timeScale = {Time.timeScale}, GameIsPaused = {GameIsPaused}");
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        Debug.Log($"[{sceneName}] GameManager OnEnable - 강제 초기화 완료!");
    }

    private void Start()
    {
        Debug.Log("GameManager Start 시작");
        
        // 🔹 게임 시작은 항상 "플레이 중" 상태로 강제 초기화
        GameIsPaused = false;
        Time.timeScale = 1f;
        
        Debug.Log($"Time.timeScale 설정 완료: {Time.timeScale}");
        
        // 커서는 게임 중에는 항상 숨김
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            Debug.Log("PauseMenuPanel 비활성화 완료");
        }
        else
        {
            Debug.LogWarning("PauseMenuPanel이 할당되지 않음!");
        }
            
        Debug.Log("GameManager Start 완료 - 게임 상태 초기화 완료");
    }

    private void Update()
    {
        // 🔹 ESC 키 처리는 여기서만!
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                // 🔹 ESC로 닫을 때는 즉시 커서 잠금 후 Resume
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                
                GameIsPaused = false;
                Time.timeScale = 1f;

                if (pauseMenuPanel != null)
                    pauseMenuPanel.SetActive(false);

                Debug.Log("ESC로 Resume - 커서 즉시 잠금");
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        GameIsPaused = true;
        Time.timeScale = 0f;
        
        // 🔹 일시정지 시 커서 보이기
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        Debug.Log("게임 일시정지");
    }

    public void Resume()
    {
        GameIsPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        // 🔹 UI 끈 다음 프레임에 커서 잠금
        StartCoroutine(LockCursorNextFrame());

        Debug.Log("게임 재개 - 커서 잠김 예약");
    }

    // 🔹 다음 프레임에 커서 잠그기
    private System.Collections.IEnumerator LockCursorNextFrame()
    {
        yield return null; // 1프레임 대기
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        Debug.Log("커서 잠김 완료!");
    }

    // 버튼 연결용 public 메서드들
    public void OnClickResume() => Resume();

    public void ToSettingMenu()
    {
        // 🔹 설정 씬으로 가기 전에 "현재 씬 이름" 저장
        SceneHistory.LastSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"설정 메뉴로 이동, 이전 씬: {SceneHistory.LastSceneName}");

        // 설정 화면에서는 커서 보이기
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Time.timeScale은 1로 복구 (설정 화면에서 정상 작동하도록)
        Time.timeScale = 1f;
        
        // 🔹 Instance 초기화 (다음 씬에서 새로운 GameManager가 생성되도록)
        Instance = null;

        SceneManager.LoadScene(settingsSceneName);
    }

    public void ToMain()
    {
        // 메인으로 나갈 땐 항상 정상 상태
        Time.timeScale = 1f;
        GameIsPaused = false;

        // 메인 메뉴에서는 커서 보이기
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // 🔹 Instance 초기화
        Instance = null;

        SceneManager.LoadScene(mainSceneName);
        Debug.Log("메인 메뉴로 이동");
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}