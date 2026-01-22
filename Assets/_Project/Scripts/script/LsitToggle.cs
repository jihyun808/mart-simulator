using UnityEngine;

/// <summary>
/// 리스트 패널 토글 (Tab 키로 열기/닫기)
/// </summary>
public class ListToggle : MonoBehaviour
{
    [Header("Panel - Inspector에서 연결")]
    public GameObject listPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            listPanel.SetActive(!listPanel.activeSelf);
        }
    }
}