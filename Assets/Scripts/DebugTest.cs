using UnityEngine;

public class DebugTest : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[DebugTest] Awake on " + gameObject.name);
    }

    private void Update()
    {
        // 매 프레임 찍히는지 확인
        Debug.Log("[DebugTest] Update");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[DebugTest] Space pressed!");
            transform.position += Vector3.up * 0.5f;
        }
    }
}
