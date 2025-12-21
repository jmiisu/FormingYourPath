using UnityEngine;
using System;

public class GridStateManager : MonoBehaviour
{
    public static GridStateManager i;

    const int height = 10;
    const int width = 18;
    [SerializeField] private Vector3Int origin;

    MAP_STATE[,] ms = new MAP_STATE[height, width];

    public void SetFirstMap()
    {
        
    }

    public bool canGo(Vector3 destpos) // 진행 가능 판단
    {
        int dest_x = (int)destpos.x;
        int dest_y = (int)destpos.y;

        MAP_STATE prev_dest = ms[dest_y, dest_x]; // 가고자 하는 위치 정보

        if (prev_dest == MAP_STATE.BASIC || prev_dest == MAP_STATE.STAGE_BLOCK) return false;
        
        return true;
    }
}
