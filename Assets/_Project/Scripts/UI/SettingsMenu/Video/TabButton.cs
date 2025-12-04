// TabButton.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class TabButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TabGroup tabGroup;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private GameObject targetPanel;

    [Header("Colors")]
    [SerializeField] private Color normalBgColor = new Color(1f, 1f, 1f, 0.1f);
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color selectedBgColor = Color.black;
    [SerializeField] private Color selectedTextColor = Color.white;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (background != null)
            background.color = isSelected ? selectedBgColor : normalBgColor;

        if (label != null)
            label.color = isSelected ? selectedTextColor : normalTextColor;

        if (targetPanel != null)
            targetPanel.SetActive(isSelected);
    }

    private void OnClick()
    {
        if (tabGroup != null)
            tabGroup.OnTabSelected(this);
    }

    public GameObject GetTargetPanel()
    {
        return targetPanel;
    }

    public bool IsSelected()
    {
        return targetPanel != null && targetPanel.activeSelf;
    }
}