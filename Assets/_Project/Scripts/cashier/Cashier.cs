using UnityEngine;

public class Cashier : MonoBehaviour
{
    public Stage1Data stageData;   // 요구사항 연결
    public Inventory inventory;    // 플레이어 인벤토리 연결
    public ShoppingListManager shoppingList; // 플레이어가 구매해야 할 리스트

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("👉 캐셔 근처 도착 - 검증 시작!");

        if (CheckRequirements())
        {
            Debug.Log("🎉 스테이지 클리어!");
            // TODO: 클리어 UI 띄우기
        }
        else
        {
            Debug.Log("❌ 조건 부족 - 리스트 확인 필요!");
            // TODO: 부족한 아이템 UI 표시
        }
    }


    bool CheckRequirements()
    {
        foreach (var req in stageData.requirements)
        {
            int count = 0;

            foreach (var item in inventory.GetAllItems())
            {
                if (item.itemName == req.itemName)
                {
                    count++;
                }
            }

            if (count < req.requiredCount)
            {
                Debug.Log($"❌ {req.itemName} 부족함. 필요: {req.requiredCount}, 가진 것: {count}");
                return false;
            }
        }

        return true; // 모든 조건 만족
    }
}
