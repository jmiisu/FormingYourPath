using UnityEngine;

public class PlacementController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject mouseIndicator;
    [SerializeField] private InteractController interactController;
    [SerializeField] private Grid gridMap;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject blockPrefab;

    private SpriteRenderer indicatorSR;

    private void Awake()
    {
        if (mouseIndicator != null)
        {
            // 자식까지 포함해서 SpriteRenderer 찾기
            indicatorSR = mouseIndicator.GetComponentInChildren<SpriteRenderer>(true);
            mouseIndicator.SetActive(true);  // 항상 켜두고 색으로만 상태 표시
            Debug.Log(indicatorSR);          // 제대로 찾는지 한 번만 로그
        }
    }

    private void Update()
    {
        if (mouseIndicator == null || interactController == null ||
            gridMap == null || player == null)
            return;

        // 1) 마우스 위치(그리드 스냅된 월드 좌표)
        Vector3 worldPos = interactController.GetSelectedMapPosition();
        mouseIndicator.transform.position = worldPos;

        // 2) 셀 좌표 계산
        Vector3Int mouseCell = gridMap.WorldToCell(worldPos);
        Vector3Int playerCell = gridMap.WorldToCell(player.position);

        int dx = mouseCell.x - playerCell.x;
        int dy = mouseCell.y - playerCell.y;

        // 3) 플레이어 주변 6칸만 허용
        // (±1, -1/0/1), 자기 자리(0,0)는 제외
        bool canPlace = (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) <= 1);

        // 4) 색상으로 가능/불가 표시
        if (indicatorSR != null)
        {
            if (canPlace)
                indicatorSR.color = new Color(0.3f, 1f, 0.3f, 0.8f); // 초록: 설치 가능
            else
                indicatorSR.color = new Color(1f, 0.3f, 0.3f, 0.8f); // 빨강: 설치 불가
        }

        // 5) 좌클릭 시 설치
        if (canPlace && Input.GetMouseButtonDown(0) && blockPrefab != null)
        {
            Debug.Log("PLACE!");
            Vector3 placePos = gridMap.GetCellCenterWorld(mouseCell);
            placePos.z = worldPos.z;
            Instantiate(blockPrefab, placePos, Quaternion.identity);
        }
    }
}
