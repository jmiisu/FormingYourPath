using TMPro;
using UnityEngine;

public class Check8DirectionComponent : MonoBehaviour
{
    private readonly MAP_STATE[,] curStateArea = new MAP_STATE[3, 3];

    [Header("References")]
    [SerializeField] private Grid gridMap;

    // 임시로 디버깅용 UI
    [SerializeField] TMP_Text[] arrState;

    private void OnEnable()
    {
        if (GridStateManager.i != null)
            GridStateManager.i.OnCellChanged += HandleCellChanged;
    }

    private void OnDisable()
    {
        if (GridStateManager.i != null)
            GridStateManager.i.OnCellChanged -= HandleCellChanged;
    }

    private void HandleCellChanged(Vector2Int changedCell, MAP_STATE newState)
    {
        if (gridMap == null) return;

        // 이 컴포넌트가 "플레이어에 붙어있다"는 전제
        Vector3Int playerCell3 = gridMap.WorldToCell(transform.position);
        var playerCell = new Vector2Int(playerCell3.x, playerCell3.y);

        // 주변 1칸(3x3)에 영향이 있으면 갱신
        int dx = changedCell.x - playerCell.x;
        int dy = -changedCell.y + playerCell.y;

        if (Mathf.Abs(dx) <= 1 && Mathf.Abs(dy) <= 1)
        {
            Update8Direction(playerCell);
            DumpArea();
        }
    }

    /// <summary>
    /// 플레이어 셀 좌표 3 x 3 상태 스냅샷을 갱신
    /// </summary>    

    public void Update8Direction(Vector2Int curPlayerPos)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int arrY = dy + 1;
                int arrX = dx + 1;

                if (dx == 0 && dy == 0)
                {
                    curStateArea[arrY, arrX] = MAP_STATE.PLAYER_POS;
                    continue;
                }

                Vector2Int targetCell = new Vector2Int(curPlayerPos.x + dx, curPlayerPos.y + dy);

                // 맵 밖 또는 상태가 없으면 BASIC 처리
                if (!GridStateManager.i.TryGetState(targetCell, out var state))
                {
                    curStateArea[arrY, arrX] = MAP_STATE.STAGE_BLOCK;
                }
                else
                {
                    curStateArea[arrY, arrX] = state;
                }
            }
        }
    }

    public void DumpArea()
    {
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                arrState[y * 3 + x].text = curStateArea[y, x].ToString();
            }
        }
    }
}
