using UnityEngine;

public class ShoppingListToggle : MonoBehaviour
{
    public GameObject shoppingListUI; // 품목 리스트 패널

    private bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleUI();
        }
    }

    void ToggleUI()
    {
        isOpen = !isOpen;
        shoppingListUI.SetActive(isOpen);
    }
}
