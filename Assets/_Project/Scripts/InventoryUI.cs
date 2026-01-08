using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public InventorySlot[] uiSlots; // 에디터에서 4개 연결

    // 배열 버전을 받습니다.
    public void UpdateUI(PickupableItem[] items)
    {
        for (int i = 0; i < uiSlots.Length; i++)
        {
            // 데이터가 있고(null 아님) & 슬롯 인덱스가 범위 내라면
            if (i < items.Length && items[i] != null)
            {
                uiSlots[i].SetItem(items[i].itemIcon);
            }
            else
            {
                uiSlots[i].Clear(); // 데이터가 없으면 비움
            }
        }
    }
}