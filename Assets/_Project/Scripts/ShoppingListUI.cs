using System.Text;
using TMPro;
using UnityEngine;

public class ShoppingListUI : MonoBehaviour
{
    public TextMeshProUGUI itemListText;
    public ShoppingListManager listManager;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (itemListText == null || listManager == null) return;

        // UI 켜질 때 최신 상태로 갱신
        listManager.UpdateCheckList();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<size=120%><b>구매 목록</b></size>");
        sb.AppendLine();

        foreach (var item in listManager.items)
        {
            // 다 모았으면 체크(✔), 아니면 빈칸(□)
            string check = item.IsComplete ? "<color=green>✔</color>" : "□";
            
            // 이름과 개수 표시 (예: 와인잔 (1/2))
            string status = $"{item.itemName} <color=yellow>({item.currentAmount}/{item.requiredAmount})</color>";
            
            // 완료되면 취소선 긋기
            if (item.IsComplete) status = $"<s>{status}</s>";

            sb.AppendLine($"{check} {status}");
        }

        itemListText.text = sb.ToString();
    }
}