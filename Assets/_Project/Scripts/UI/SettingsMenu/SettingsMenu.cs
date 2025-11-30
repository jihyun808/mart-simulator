using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private string defaultSceneName = "MainMenu";

    public void OnClickClose()
    {
        string target = SceneHistory.LastSceneName;

        if (string.IsNullOrEmpty(target))
        {
            target = defaultSceneName;
        }
        SceneManager.LoadScene(target);
    }
}
