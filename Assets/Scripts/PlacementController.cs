using UnityEngine;

public class PlacementController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject mouseIndicator;
    [SerializeField] private InteractController interactController;
    [SerializeField] private Grid gridMap;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private LevelManager _level;

    [Header("Placement")]
    [SerializeField] private MAP_STATE placedState = MAP_STATE.BASIC;

    private SpriteRenderer indicatorSR;

    private void Awake()
    {
        _level = FindAnyObjectByType<LevelManager>();
        if (mouseIndicator != null)
        {
            // 자식까지 포함해서 SpriteRenderer 찾기
            indicatorSR = mouseIndicator.GetComponentInChildren<SpriteRenderer>(true);
            mouseIndicator.SetActive(true);  // 항상 켜두고 색으로만 상태 표시
            //Debug.Log(indicatorSR);          // 제대로 찾는지 한 번만 로그
        }
    }

    private Vector2Int WorldToMapCell(Vector3 world)
    {
        float ts = _level.TileSize;
        Vector3 ws = _level.WorldStart;

        int x = Mathf.RoundToInt((world.x - ws.x) / ts);
        int y = Mathf.RoundToInt((ws.y - world.y) / ts); // y 반전 포함 (아래로 갈수록 +)
        return new Vector2Int(x, y);
    }

    private void Update()
    {
        if (mouseIndicator == null || interactController == null ||
            gridMap == null || player == null)
            return;

        if (GridStateManager.i == null) return;

        // 1) 마우스 위치(그리드 스냅된 월드 좌표)
        Vector3 worldPos = interactController.GetSelectedMapPosition();
        mouseIndicator.transform.position = worldPos;

        // 2) 셀 좌표 계산
        Vector3Int mouseCell_3 = gridMap.WorldToCell(worldPos);
        Vector3Int playerCell_3 = gridMap.WorldToCell(player.position);

        Vector2Int mouseCell = WorldToMapCell(worldPos);
        Vector2Int playerCell = WorldToMapCell(player.position);

        int dx = mouseCell.x - playerCell.x;
        int dy = mouseCell.y - playerCell.y;

        // 3) 플레이어 주변 6칸만 허용
        // (±1, -1/0/1), 자기 자리(0,0)는 제외
        bool nearRule = (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) <= 1);

        bool inside = GridStateManager.i.IsInside(mouseCell);
        bool hasState = GridStateManager.i.TryGetState(mouseCell, out var state);
        bool isEmpty = hasState && (state == MAP_STATE.EMPTY);

        bool canPlace = nearRule && inside && isEmpty;

        // 4) 색상으로 가능/불가 표시
        if (indicatorSR != null)
        {
            indicatorSR.color = canPlace ? Color.green : Color.red;
        }

        // 5) 좌클릭 시 설치
        if (canPlace && Input.GetMouseButtonDown(0) && blockPrefab != null)
        {
            Debug.Log("PLACE!");
            Vector3 placePos = gridMap.GetCellCenterWorld(mouseCell_3);

            GameObject placed = Instantiate(blockPrefab, placePos, Quaternion.identity);
            placed.transform.SetParent(_level.transform);

            GridStateManager.i.SetState(mouseCell, placedState);

            Check8DirectionComponent check = player.GetComponent<Check8DirectionComponent>();
            if (check != null)
            {
                check.Update8Direction(playerCell);
                check.DumpArea();
            }
        }

        //Debug.Log($"mouseCell={mouseCell}, inside={inside}, hasState={hasState}, state={(hasState ? state.ToString() : "NONE")}, nearRule={nearRule}");
    }
}
