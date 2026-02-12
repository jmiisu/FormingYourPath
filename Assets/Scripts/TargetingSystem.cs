using UnityEngine;

public struct GridTargetContext
{
    public Vector2Int cell;
    public Vector3 worldPos;
    public Vector2Int playerCell;

    public bool inside;
    public bool nearRule;

    public bool hasState;
    public MAP_STATE state;
    public bool isEmpty;

    public bool isPlacedBlock;
}

public interface IGridTargetProvider
{
    bool TryGetTarget(out GridTargetContext info);
}

public class TargetingSystem : MonoBehaviour, IGridTargetProvider
{
    [SerializeField] private InteractController _interact;
    [SerializeField] private Transform _player;
    [SerializeField] private LevelManager _level;

    private Vector2Int WorldToMapCell(Vector3 world)
    {
        float ts = _level.TileSize;
        Vector3 ws = _level.WorldStart;

        int x = Mathf.RoundToInt((world.x - ws.x) / ts);
        int y = Mathf.RoundToInt((ws.y - world.y) / ts); // y 반전 포함 (아래로 갈수록 +)
        return new Vector2Int(x, y);
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        Vector3 placePos = new Vector3(
            _level.WorldStart.x + _level.TileSize * cell.x,
            _level.WorldStart.y - _level.TileSize * cell.y,
            0
        );
        return placePos;
    }

    public bool TryGetTarget(out GridTargetContext info)
    {
        info = default;
        if (GridStateManager.i == null || _interact == null || _level == null) return false;

        // 1) 마우스 위치(그리드 스냅된 월드 좌표)
        Vector3 worldPos = _interact.GetSelectedMapPosition();

        // 2) 셀 좌표 계산
        Vector2Int mouseCell = WorldToMapCell(worldPos);
        Vector2Int playerCell = WorldToMapCell(_player.position);

        //mouseIndicator.transform.position = placePos;

        int dx = mouseCell.x - playerCell.x;
        int dy = mouseCell.y - playerCell.y;

        // 3) 플레이어 주변 6칸만 허용
        // (±1, -1/0/1), 자기 자리(0,0)는 제외
        bool nearRule = (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) <= 1);

        bool inside = GridStateManager.i.IsInside(mouseCell);
        bool hasState = GridStateManager.i.TryGetState(mouseCell, out var state);
        bool isEmpty = hasState && (state == MAP_STATE.EMPTY);

        bool isPlacedBlock = hasState && GridStateManager.i.IsThereBlockYouPlaced(mouseCell);

        info = new GridTargetContext
        {
            cell = mouseCell,
            worldPos = CellToWorld(mouseCell),
            playerCell = playerCell,

            inside = inside,
            nearRule = nearRule,

            hasState = hasState,
            state = state,
            isEmpty = isEmpty,

            isPlacedBlock = isPlacedBlock,
        };
        return true;
    }
}
