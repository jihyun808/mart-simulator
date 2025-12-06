using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    public void SetItem(PickupableItem item)
    {
        itemNameText.text = item.gameObject.name;
    }
}
