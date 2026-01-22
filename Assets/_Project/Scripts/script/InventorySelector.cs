using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 슬롯 선택 관리 (F1~F4 키로 슬롯 전환)
/// CurrentIndex를 통해 현재 선택된 슬롯 인덱스 제공
/// </summary>
public class InventorySelector : MonoBehaviour
{
    [Header("Slot Images - Inspector에서 연결")]
    public Image[] slots;
    
    [Header("Slot Colors")]
    public Color normalColor = new Color(1, 1, 1, 0.3f);
    public Color selectedColor = Color.white;

    public int CurrentIndex { get; private set; } = 0;

    private void Start()
    {
        HighlightSlot(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) HighlightSlot(0);
        else if (Input.GetKeyDown(KeyCode.F2)) HighlightSlot(1);
        else if (Input.GetKeyDown(KeyCode.F3)) HighlightSlot(2);
        else if (Input.GetKeyDown(KeyCode.F4)) HighlightSlot(3);
    }

    /// <summary>선택된 슬롯 하이라이트 처리</summary>
    private void HighlightSlot(int index)
    {
        CurrentIndex = index;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].color = (i == index) ? selectedColor : normalColor;
        }
    }
}