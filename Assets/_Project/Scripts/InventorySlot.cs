using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 슬롯 하나를 담당 (아이템 아이콘 표시/숨김)
/// </summary>
public class InventorySlot : MonoBehaviour
{
    [Header("Icon Image - Inspector에서 연결")]
    public Image iconImage;

    /// <summary>아이템 아이콘 설정 및 표시</summary>
    public void SetItem(Sprite sprite)
    {
        if (iconImage == null) return;

        iconImage.sprite = sprite;
        iconImage.color = Color.white;
        iconImage.enabled = true;
    }

    /// <summary>슬롯 비우기 (아이콘 숨김)</summary>
    public void Clear()
    {
        if (iconImage == null) return;

        iconImage.sprite = null;
        iconImage.enabled = false;
    }
}