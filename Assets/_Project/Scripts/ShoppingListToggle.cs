using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 쇼핑 리스트 통합 관리 (토글 + UI 표시)
/// Tab 키로 열기/닫기, 열릴 때마다 자동으로 최신 상태로 갱신
/// </summary>
public class ShoppingListToggle : MonoBehaviour
{
    [Header("UI References - Inspector에서 연결")]
    public GameObject shoppingListPanel;
    public TextMeshProUGUI itemListText;
    public ShoppingListManager listManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool nextState = !shoppingListPanel.activeSelf;
            shoppingListPanel.SetActive(nextState);
            
            if (nextState)
            {
                RefreshUI();
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("1. 탭 키 입력 감지됨!"); // 로그 출력 1
            
            bool nextState = !shoppingListPanel.activeSelf;
            shoppingListPanel.SetActive(nextState);
            
            Debug.Log($"2. 패널 상태 변경: {nextState}"); // 로그 출력 2

            if (nextState)
            {
                RefreshUI();
                Debug.Log("3. UI 갱신 완료"); // 로그 출력 3
            }
        }
    }

    /// <summary>쇼핑 리스트 UI 갱신 (외부에서도 호출 가능)</summary>
    public void RefreshUI()
    {
        if (itemListText == null || listManager == null) return;

        listManager.UpdateCheckList();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<size=120%><b>구매 목록</b></size>");
        sb.AppendLine();

        foreach (var item in listManager.items)
        {
            string check = item.IsComplete ? "<color=green>✔</color>" : "□";
            string status = $"{item.itemName} <color=yellow>({item.currentAmount}/{item.requiredAmount})</color>";
            
            if (item.IsComplete)
            {
                status = $"<s>{status}</s>";
            }

            sb.AppendLine($"{check} {status}");
        }

        itemListText.text = sb.ToString();
    }
}