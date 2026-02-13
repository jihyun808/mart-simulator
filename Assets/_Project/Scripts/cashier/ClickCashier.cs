using UnityEngine;
using UnityEngine.EventSystems;

public class ClickCashier : MonoBehaviour
{
    public Camera cam;                // 자동 연결
    public LayerMask cashierLayer;    // 캐셔 레이어 선택 (예: cashier)

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; 
        }
        if (Input.GetMouseButtonDown(0)) // 좌클릭
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 3f, cashierLayer))
{
    Debug.Log("🎯 Raycast Hit: " + hit.collider.name);
}
else
{
    Debug.Log("❌ Raycast Missed 캐셔 인식 실패");
}

            // 거리 10m, cashierLayer만 감지
            if (Physics.Raycast(ray, out hit, 3f, cashierLayer))
            {
                Debug.Log("캐셔 클릭됨 → " + hit.collider.name);

                CashierInteraction cashier = hit.collider.GetComponent<CashierInteraction>();

                if (cashier != null)
                {
                    cashier.TryCheckoutByClick();
                }
                else
                {
                    Debug.Log("❌ 캐셔에 CashierInteraction 스크립트 없음");
                }
            }
            else
            {
                Debug.Log("❌ Raycast가 캐셔에 닿지 않음");
            }
        }
    }
}
