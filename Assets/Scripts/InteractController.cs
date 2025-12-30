using UnityEngine;

public class InteractController : MonoBehaviour
{
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private Grid gridMap;   

    public Vector3 GetSelectedMapPosition()
    {
        if (sceneCamera == null) return Vector3.zero;

        // 화면 좌표 -> 월드 좌표
        Vector3 w = sceneCamera.ScreenToWorldPoint(Input.mousePosition);

        // 그리드가 있으면 셀 중심으로 스냅
        if (gridMap != null)
        {
            Vector3Int cell = gridMap.WorldToCell(w);
            w = gridMap.GetCellCenterWorld(cell);
        }

        return w;
    }
}
