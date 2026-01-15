using UnityEngine;
using UnityEngine.UI;

public class InventorySelector : MonoBehaviour
{
    public Image[] slots;  // Slot1~Slot4 배경 이미지
    
    // 현재 선택된 인덱스를 외부에서 가져갈 수 있게 함
    public int CurrentIndex { get; private set; } = 0;

    public Color normalColor = new Color(1, 1, 1, 0.3f);
    public Color selectedColor = Color.white;

    void Start()
    {
        HighlightSlot(0);  
    }

    void Update()
    {
        // 키 입력에 따라 슬롯 변경
        if (Input.GetKeyDown(KeyCode.F1)) HighlightSlot(0);
        if (Input.GetKeyDown(KeyCode.F2)) HighlightSlot(1);
        if (Input.GetKeyDown(KeyCode.F3)) HighlightSlot(2);
        if (Input.GetKeyDown(KeyCode.F4)) HighlightSlot(3);
    }

    void HighlightSlot(int index)
    {
        CurrentIndex = index; // 선택된 인덱스 저장

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].color = (i == index) ? selectedColor : normalColor;
        }
    }
}