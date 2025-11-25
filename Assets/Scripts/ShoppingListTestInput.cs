using UnityEngine;

public class ShoppingListTestInput : MonoBehaviour
{
    public ShoppingListManager listManager;
    public ShoppingListUI listUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            listManager.MarkPurchased("우유");
            listUI.RefreshUI();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            listManager.MarkPurchased("빵");
            listUI.RefreshUI();
        }
    }
}
