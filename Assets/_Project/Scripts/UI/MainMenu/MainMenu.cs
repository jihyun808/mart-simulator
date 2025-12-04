// MainMenu.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string settingsSceneName = "SettingsScene";

    private void Start()
    {
        SetCursorState(true);
        Time.timeScale = 1f;
    }

    public void OnClickStartGame()
    {
        SceneHistory.Clear();
        LoadScene(gameSceneName);
    }

    public void OnClickSettings()
    {
        SceneHistory.LastSceneName = SceneManager.GetActiveScene().name;
        LoadScene(settingsSceneName);
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}