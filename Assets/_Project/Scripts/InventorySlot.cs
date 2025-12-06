using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    // 이 슬롯에서 아이템 아이콘을 표시할 이미지
    public Image iconImage;

    // 비어 있는지 여부
    public bool IsEmpty => iconImage.sprite == null;

    // 아이템 넣기
    public void SetItem(Sprite sprite)
    {
        iconImage.sprite = sprite;
        iconImage.enabled = true; // 아이콘 보이게
    }

    // 아이템 빼기
    public void Clear()
    {
        iconImage.sprite = null;
        iconImage.enabled = false; // 아이콘 숨기기
    }
}
