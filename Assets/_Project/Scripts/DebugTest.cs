using UnityEngine;

public class DebugTest : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    public bool enableVisualDebug = true;
    [Range(0.05f, 0.5f)]
    public float debugRayLength = 0.1f;
    public LayerMask debugLayerMask = ~0;

    private CharacterController controller;
    private MovementCharacterController movement;
    private Camera playerCamera;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<MovementCharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (controller == null)
        {
            Debug.LogWarning("[DebugTest] CharacterController를 찾을 수 없습니다!");
        }

        if (movement == null)
        {
            Debug.LogWarning("[DebugTest] MovementCharacterController를 찾을 수 없습니다!");
        }
    }

    private void Update()
    {
        if (enableDebugLogs)
        {
            CheckForObstacles();
        }
    }

    private void CheckForObstacles()
    {
        if (controller == null || playerCamera == null) return;

        Vector3 playerPosition = transform.position;
        Vector3 playerCenter = playerPosition + Vector3.up * (controller.height * 0.5f);

        // Shift 키 눌렀을 때 머리 위 장애물 체크
        if (Input.GetKey(KeyCode.LeftShift))
        {
            CheckHeadObstacle();
        }

        // 이동 입력 감지
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f)
        {
            Vector3 moveDirection = (transform.right * horizontal + transform.forward * vertical).normalized;

            CheckObstacleAtHeight(playerCenter, moveDirection, "중간 높이");
            CheckObstacleAtHeight(playerCenter + Vector3.up * 0.3f, moveDirection, "상단");
            CheckObstacleAtHeight(playerCenter - Vector3.up * 0.3f, moveDirection, "하단");
        }
    }

    private void CheckObstacleAtHeight(Vector3 startPos, Vector3 direction, string heightInfo)
    {
        RaycastHit hit;
        float checkDistance = controller.radius + debugRayLength;

        if (Physics.Raycast(startPos, direction, out hit, checkDistance, debugLayerMask))
        {
            float distanceToPlayer = hit.distance;

            if (distanceToPlayer <= checkDistance)
            {
                LogObstacle(hit, heightInfo, startPos, direction);
            }
        }
    }

    private void LogObstacle(RaycastHit hit, string checkType, Vector3 rayStart, Vector3 rayDirection)
    {
        if (!enableDebugLogs) return;

        GameObject obstacle = hit.collider.gameObject;
        string obstacleName = obstacle.name;
        string obstacleTag = obstacle.tag;
        int obstacleLayer = obstacle.layer;
        string layerName = LayerMask.LayerToName(obstacleLayer);
        float distance = hit.distance;

        Debug.Log($"<color=yellow>[장애물 감지]</color> {obstacleName} (태그: {obstacleTag}, 레이어: {layerName}, 거리: {distance:F2}m)");

        CheckSpecialCases(obstacle);

        if (enableVisualDebug)
        {
            Debug.DrawRay(rayStart, rayDirection * debugRayLength, Color.red, 0.1f);
            Debug.DrawLine(rayStart, hit.point, Color.yellow, 0.1f);
        }
    }

    private void CheckHeadObstacle()
    {
        if (controller == null) return;

        Vector3 playerHead = transform.position + Vector3.up * controller.height;
        float checkHeight = 0.5f;

        RaycastHit hit;
        if (Physics.Raycast(playerHead, Vector3.up, out hit, checkHeight, debugLayerMask))
        {
            GameObject obstacle = hit.collider.gameObject;
            Debug.Log($"<color=cyan>[머리 위 장애물]</color> {obstacle.name} (태그: {obstacle.tag}, 거리: {hit.distance:F2}m)");

            if (enableVisualDebug)
            {
                Debug.DrawRay(playerHead, Vector3.up * checkHeight, Color.cyan, 0.1f);
                Debug.DrawLine(playerHead, hit.point, Color.magenta, 0.1f);
            }
        }
    }

    private void CheckSpecialCases(GameObject obstacle)
    {
        // 문 체크
        if (obstacle.GetComponent<SimpleDoor>() != null)
        {
            SimpleDoor door = obstacle.GetComponent<SimpleDoor>();
            Debug.Log($"<color=green>[문 감지]</color> 상태: {(door.doorOpened ? "열림" : "닫힘")}");
        }

        // AI 체크
        // if (obstacle.GetComponent<AIController>() != null)
        // {
        //     AIController ai = obstacle.GetComponent<AIController>();
        //     Debug.Log($"<color=red>[AI 감지]</color> 상태: {ai.GetCurrentState()}");
        // }

        // Static 오브젝트 체크
        if (obstacle.isStatic)
        {
            Debug.Log($"<color=gray>[Static]</color> {obstacle.name}");
        }

        // PickupableItem 체크
        if (obstacle.GetComponent<PickupableItem>() != null)
        {
            Debug.Log($"<color=orange>[아이템]</color> {obstacle.name}");
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!enableDebugLogs) return;

        GameObject hitObject = hit.collider.gameObject;
        Debug.Log($"<color=red>[실제 충돌!]</color> {hitObject.name} (태그: {hitObject.tag})");
    }

    private void OnDrawGizmosSelected()
    {
        if (controller == null) return;

        // CharacterController 영역
        Gizmos.color = Color.green;
        Vector3 center = transform.position + Vector3.up * (controller.height * 0.5f);
        Gizmos.DrawWireSphere(center, controller.radius);

        // 감지 영역
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, controller.radius + debugRayLength);

        // 머리 위 체크 영역
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Gizmos.color = Color.cyan;
            Vector3 headPos = transform.position + Vector3.up * controller.height;
            Gizmos.DrawWireSphere(headPos, 0.2f);
            Gizmos.DrawLine(headPos, headPos + Vector3.up * 0.5f);
        }
    }
}