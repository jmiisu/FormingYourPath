using System;
using System.Collections.Generic;
using UnityEngine;

public class GridStateManager : MonoBehaviour
{
    public static GridStateManager i;

    private Dictionary<Vector2Int, MAP_STATE> _map = new Dictionary<Vector2Int, MAP_STATE>();
    private int _width;
    private int _height;

    public event Action<Vector2Int, MAP_STATE> OnCellChanged;
        
    private void Awake()
    {
        if (i != null && i != this)
        {
            Destroy(gameObject);
            return;
        }
        i = this;
    }

    public void Init(Dictionary<Vector2Int, MAP_STATE> initialMap, int width, int height)
    {
        _map = initialMap ?? new Dictionary<Vector2Int, MAP_STATE>();
        _width = width;
        _height = height;
    }

    // 범위 안에 들어있는지
    public bool IsInside(Vector2Int cell)
    {
        return (cell.x >= 0 && cell.x < _width && cell.y >= 0 && cell.y < _height);
    }

    public bool IsThereFloor(Vector2Int cellBelow)
    {
        if (!TryGetState(cellBelow, out var belowState)) return false; // 맵 밖

        return belowState == MAP_STATE.STAGE_BLOCK || belowState == MAP_STATE.BASIC;
    }

    public bool TryGetState(Vector2Int cell, out MAP_STATE state)
    {
        state = MAP_STATE.EMPTY;

        if (!IsInside(cell)) return false;

        return _map.TryGetValue(cell, out state);
    }

    // 이동 가능 규칙
    public bool IsWalkable(Vector2Int cell)
    {
        if (!TryGetState(cell, out var state)) return false; // 맵 밖

        // 최소 규칙: 막힌 블록은 이동 불가
        if (state == MAP_STATE.STAGE_BLOCK) return false;
        if (state == MAP_STATE.BASIC) return false;

        // EMPTY/EXIT/STAIR 등은 이동 가능

        // 다음 칸 아래에 바닥이 없으면 이동 불가
        Vector2Int below = new Vector2Int(cell.x, cell.y + 1);
        if (!IsThereFloor(below)) return false;

        return true;
    }

    public bool SetState(Vector2Int cell, MAP_STATE newState)
    {
        if (!IsInside(cell)) return false;

        _map[cell] = newState;
        OnCellChanged?.Invoke(cell, newState);
        return true;
    }
}
