using System;
using System.Collections.Generic;
using UnityEditor.MPE;
using UnityEngine;

public class GridStateManager : MonoBehaviour
{
    public static GridStateManager i;

    private Dictionary<Vector2Int, MAP_STATE> _map = new();
    private Dictionary<Vector2Int, GameObject> _placedObjByCell = new(); // 설치한 블록 데이터
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

        return belowState == MAP_STATE.STAGE_BLOCK 
            || belowState == MAP_STATE.BASIC 
            || belowState == MAP_STATE.STAIR;
    }

    public bool TryGetState(Vector2Int cell, out MAP_STATE state)
    {
        state = MAP_STATE.EMPTY;

        if (!IsInside(cell)) return false;

        return _map.TryGetValue(cell, out state);
    }

    public bool IsThereBlockYouPlaced(Vector2Int cell)
    {
        if (!TryGetState(cell, out var state)) return false;

        return state == MAP_STATE.BASIC || state == MAP_STATE.STAIR;
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

    public bool RegisterPlacedBlock(Vector2Int cell, MAP_STATE placedState, GameObject placedObj, out GameObject prevObj, out MAP_STATE prevState)
    {
        prevObj = null;
        prevState = MAP_STATE.EMPTY;

        if (!IsInside(cell)) return false;

        if (placedObj == null) return false;

        if (placedState != MAP_STATE.BASIC && placedState != MAP_STATE.STAIR) return false;

        // 이전 정보는 밖으로 넘김
        if (_placedObjByCell.TryGetValue(cell, out var prev) && prev != null)
        {
            prevObj = prev;
            _map.TryGetValue(cell, out prevState);
        }

        _placedObjByCell[cell] = placedObj;
        _map[cell] = placedState;
        OnCellChanged?.Invoke(cell, placedState);
        return true;
    }

    public bool TryRemovePlacedBlock(Vector2Int cell, out GameObject removedObj, out MAP_STATE removedState)
    {
        removedObj = null;
        removedState = MAP_STATE.EMPTY;

        if (!IsInside(cell))
        {
            Debug.Log("OUTSIDE!");
            return false;
        }
            
        if (!IsThereBlockYouPlaced(cell))
        {
            Debug.Log("NO BLOCK YOU PLACED!");
            return false;
        }

        if (!_placedObjByCell.TryGetValue(cell, out removedObj))
        {
            Debug.Log("NO VALUE");
            return false;
        }

        _map.TryGetValue(cell, out removedState);

        _placedObjByCell.Remove(cell);
        _map[cell] = MAP_STATE.EMPTY;
        OnCellChanged?.Invoke(cell, MAP_STATE.EMPTY);
        return true;
    }
}
