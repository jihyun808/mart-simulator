using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("이동할 씬 이름들")]
    [SerializeField] private string gameSceneName     = "GameScene";      // 게임 플레이 씬
    [SerializeField] private string settingsSceneName = "SettingsScene";  // 옵션/설정 씬
    [SerializeField] private string creditSceneName   = "CreditScene";    // 크레딧 씬

    // "게임 시작" 버튼
    public void OnClickStartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // "설정" 버튼
    public void OnClickSettings()
    {
        SceneManager.LoadScene(settingsSceneName);
    }

    // "크레딧" 버튼
    public void OnClickCredit()
    {
        SceneManager.LoadScene(creditSceneName);
    }

    // 선택: 게임 종료 버튼 만들고 싶을 때
    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
