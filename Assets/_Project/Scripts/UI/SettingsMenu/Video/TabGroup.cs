// TabGroup.cs
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    [Header("Tab List")]
    [SerializeField] private TabButton[] tabs;

    [Header("Default Tab")]
    [SerializeField] private TabButton defaultTab;

    private TabButton currentTab;

    private void Start()
    {
        if (defaultTab != null)
        {
            OnTabSelected(defaultTab);
        }
        else if (tabs != null && tabs.Length > 0)
        {
            OnTabSelected(tabs[0]);
        }
    }

    public void OnTabSelected(TabButton selected)
    {
        if (selected == null || tabs == null) return;

        foreach (var tab in tabs)
        {
            if (tab == null) continue;
            tab.SetSelected(tab == selected);
        }

        currentTab = selected;
    }

    public TabButton GetCurrentTab()
    {
        return currentTab;
    }

    public void SelectTabByIndex(int index)
    {
        if (tabs != null && index >= 0 && index < tabs.Length)
        {
            OnTabSelected(tabs[index]);
        }
    }
}