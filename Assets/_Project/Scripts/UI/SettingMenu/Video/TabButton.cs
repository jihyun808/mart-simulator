using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabButton : MonoBehaviour
{
    [Header("참조")]
    public TabGroup tabGroup;             // TabButtons 부모에 있는 TabGroup
    public Image background;              // 버튼 배경 Image
    public TextMeshProUGUI label;         // 버튼 안 글자
    public GameObject targetPanel;        // 이 탭이 켜질 때 보여줄 패널

    [Header("색상 세팅")]
    public Color normalBgColor = new Color(1f, 1f, 1f, 0.1f);  // 비활성 배경
    public Color normalTextColor = Color.black;                // 비활성 텍스트
    public Color selectedBgColor = Color.black;                // 선택 배경
    public Color selectedTextColor = Color.white;              // 선택 텍스트

    // TabGroup에서 호출
    public void SetSelected(bool isSelected)
    {
        if (background != null)
            background.color = isSelected ? selectedBgColor : normalBgColor;

        if (label != null)
            label.color = isSelected ? selectedTextColor : normalTextColor;

        if (targetPanel != null)
            targetPanel.SetActive(isSelected);
    }

    // 버튼 OnClick에 이 함수만 연결하면 됨
    public void OnClick()
    {
        if (tabGroup != null)
            tabGroup.OnTabSelected(this);
    }
}
