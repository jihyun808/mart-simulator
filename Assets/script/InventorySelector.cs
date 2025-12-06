using UnityEngine;
using UnityEngine.UI;

public class InventorySelector : MonoBehaviour
{
    public Image[] slots;  // 4개의 슬롯 (Slot1~Slot4)
    private int currentIndex = 0;

    public Color normalColor = new Color(1, 1, 1, 0.3f); // 기본 색
    public Color selectedColor = Color.white; // 선택된 슬롯 색

    void Start()
    {
        HighlightSlot(0);  
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) HighlightSlot(0);
        if (Input.GetKeyDown(KeyCode.F2)) HighlightSlot(1);
        if (Input.GetKeyDown(KeyCode.F3)) HighlightSlot(2);
        if (Input.GetKeyDown(KeyCode.F4)) HighlightSlot(3);
    }

    void HighlightSlot(int index)
    {
        currentIndex = index;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].color = (i == index) ? selectedColor : normalColor;
        }
    }
}
