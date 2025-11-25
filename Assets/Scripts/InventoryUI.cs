using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public InventorySlot[] slots;  // 4칸 슬롯들

    // 아이템 추가 (Sprite 아이콘만 넣어서 표시)
    public bool AddItem(Sprite icon)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.SetItem(icon);
                return true;
            }
        }

        Debug.Log("InventoryUI: 인벤토리가 가득 찼습니다.");
        return false;
    }

    // index 번째 슬롯 비우기 (원하면 나중에 사용)
    public void RemoveItem(int index)
    {
        if (index < 0 || index >= slots.Length)
        {
            Debug.LogWarning("InventoryUI: 잘못된 슬롯 인덱스");
            return;
        }

        slots[index].Clear();
    }
}
