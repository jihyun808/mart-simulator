using System.Text;
using TMPro;
using UnityEngine;

public class ShoppingListToggle : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shoppingListPanel;
    public TextMeshProUGUI itemListText;
    public ShoppingListManager listManager;

    private void Update()
    {
        // 중복된 Input 코드를 하나로 합침
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool nextState = !shoppingListPanel.activeSelf;
            shoppingListPanel.SetActive(nextState);
            
            if (nextState)
            {
                RefreshUI();
            }
        }
    }

    public void RefreshUI()
    {
        if (itemListText == null || listManager == null) return;

        // ⭐ 여기서 매니저에게 "가방이랑 카트 다 뒤져봐!" 라고 명령함
        listManager.UpdateCheckList();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<size=120%><b>구매 목록</b></size>");
        sb.AppendLine();

        foreach (var item in listManager.items)
        {
            string check = item.IsComplete ? "<color=green>✔</color>" : "□";
            
            // 완료되면 취소선, 아니면 노란색 강조
            string status = item.IsComplete 
                ? $"<s>{item.itemName} ({item.currentAmount}/{item.requiredAmount})</s>"
                : $"{item.itemName} <color=yellow>({item.currentAmount}/{item.requiredAmount})</color>";

            sb.AppendLine($"{check} {status}");
        }

        itemListText.text = sb.ToString();
    }
}