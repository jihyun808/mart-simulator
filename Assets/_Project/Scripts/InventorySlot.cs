using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    // 슬롯 안에 있는 "아이템 그림 보여줄 Image 컴포넌트"
    public Image iconImage; 

    public void SetItem(Sprite sprite)
    {
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
            
            // 색상을 하얀색(불투명)으로 변경 (중요!)
            iconImage.color = new Color(1, 1, 1, 1); 
            
            // 켜기
            iconImage.enabled = true;
        }
    }

    public void Clear()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            
            // 끄거나 투명하게 (보통 끄는 게 성능상 좋음)
            iconImage.enabled = false; 
            // 혹은 iconImage.color = new Color(1, 1, 1, 0);
        }
    }
}