using TMPro;
using UnityEngine;

public class Check8DirectionComponent : MonoBehaviour
{
    private readonly MAP_STATE[,] curStateArea = new MAP_STATE[3, 3];
    [Header("References")]
    [SerializeField] private Grid gridMap;

    // 임시로 디버깅용 UI
    [SerializeField] TMP_Text[] arrState;

    /// <summary>
    /// 플레이어 셀 좌표 3 x 3 상태 스냅샷을 갱신
    /// </summary>    

    public void Update8Direction(Vector3Int curPlayerPos)
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

    /// <summary>
    /// (dx, dy) 칸으로 이동 가능한지 3x3 스냅샷 기준으로 판단
    /// </summary>
    /// 

    public bool CanMove(Vector3Int curPlayerPos, Vector2Int dir)
    {
        if (GridStateManager.i == null) return false;

        Vector2Int next = new Vector2Int(curPlayerPos.x + dir.x, curPlayerPos.y + dir.y);
        return GridStateManager.i.IsWalkable(next);
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
        //return
        //    $"{curStateArea[0, 0]}\t{curStateArea[0, 1]}\t{curStateArea[0, 2]}\n" +
        //    $"{curStateArea[1, 0]}\t{curStateArea[1, 1]}\t{curStateArea[1, 2]}\n" +
        //    $"{curStateArea[2, 0]}\t{curStateArea[2, 1]}\t{curStateArea[2, 2]}\n";
    }
}
