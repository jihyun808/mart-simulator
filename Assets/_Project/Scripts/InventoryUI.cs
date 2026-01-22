using UnityEngine;

/// <summary>
/// 인벤토리 UI 전체 관리 (슬롯 업데이트)
/// Inventory 데이터 변경 시 UpdateUI() 호출 필요
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI Slots - Inspector에서 연결 (4개)")]
    public InventorySlot[] uiSlots;

    /// <summary>인벤토리 아이템 배열을 받아 UI 업데이트</summary>
    public void UpdateUI(PickupableItem[] items)
    {
        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (i < items.Length && items[i] != null)
            {
                uiSlots[i].SetItem(items[i].itemIcon);
            }
            else
            {
                uiSlots[i].Clear();
            }
        }
    }
}