using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClearUIManager : MonoBehaviour
{
    public GameObject clearPanel;
    public TMP_Text clearText;
    public Button nextStageButton;

    void Start()
    {
        clearPanel.SetActive(false);

        nextStageButton.onClick.AddListener(() =>
        {
            LoadNextStage();
        });
    }

    public void ShowClearUI(int stage)
    {
        clearPanel.SetActive(true);
        clearText.text = $"STAGE {stage} CLEAR!";
        Time.timeScale = 0f;   // 게임 일시정지
    }

    void LoadNextStage()
    {
        Time.timeScale = 1f; // 다시 풀기
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}