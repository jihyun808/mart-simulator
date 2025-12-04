// SettingsMenu.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private string defaultSceneName = "MainMenu";

    private void Start()
    {
        SetCursorState(true);
    }

    public void OnClickClose()
    {
        string targetScene = SceneHistory.HasHistory() 
            ? SceneHistory.LastSceneName 
            : defaultSceneName;

        LoadScene(targetScene);
    }

    private void LoadScene(string sceneName)
    {
        SceneHistory.Clear();
        SceneManager.LoadScene(sceneName);
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}