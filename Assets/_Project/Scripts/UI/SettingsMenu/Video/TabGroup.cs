using UnityEngine;

public class TabGroup : MonoBehaviour
{
    [Header("탭 목록")]
    public TabButton[] tabs;

    [Header("시작 시 기본으로 선택할 탭 (선택)")]
    public TabButton defaultTab;

    private TabButton current;

    private void Start()
    {
        // 씬 시작할 때 기본 탭 자동 선택
        if (defaultTab != null)
        {
            OnTabSelected(defaultTab);
        }
    }

    public void OnTabSelected(TabButton selected)
    {
        // 모든 탭을 돌면서 선택/비선택 상태 갱신
        foreach (var tab in tabs)
        {
            bool isSelected = (tab == selected);
            tab.SetSelected(isSelected);
        }

        current = selected;
    }
}
