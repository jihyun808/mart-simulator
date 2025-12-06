using System.Text;
using TMPro;
using UnityEngine;

public class ShoppingListUI : MonoBehaviour
{
    public TextMeshProUGUI itemListText;      // 품목을 보여줄 텍스트
    public ShoppingListManager listManager;   // 방금 만든 ShoppingSystem

    private void OnEnable()
    {
        RefreshUI();   // Panel이 열릴 때마다 갱신
    }

    public void RefreshUI()
    {
        if (itemListText == null || listManager == null)
        {
            Debug.LogWarning("ShoppingListUI: 참조가 설정되지 않았습니다.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("구매 목록");
        sb.AppendLine();

        foreach (var item in listManager.items)
        {
            string check = item.purchased ? "✔" : "□";
            sb.AppendLine($"{check} {item.itemName}");
        }

        Debug.Log("ShoppingListUI 문자열:\n" + sb.ToString());

        itemListText.text = sb.ToString();
    }
}
