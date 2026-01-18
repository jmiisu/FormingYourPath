using System;
using UnityEngine;

public enum DIRECTION
{
    LEFT, RIGHT, NONE
};

public class MoveController : MonoBehaviour
{
    [SerializeField] float speed = 1f;

    Vector2Int cellPos = Vector2Int.zero;
    bool _isMoving = false;
    bool _isJumping = false;
    bool _waitingReachExit = false; // 출구 도착까지 대기

    public Vector3 curPos { get; private set; } // 현재 위치 3 * 3 맵에 필요
    DIRECTION _dir = DIRECTION.LEFT;

    private LevelManager _level;        // LevelManager 참조
    private Check8DirectionComponent _check8Dir;

    // LevelManager가 스폰 셀을 알려줄 때 호출
    public void SetStartCell(Vector2Int startCell, LevelManager level)
    {
        _level = level;
        cellPos = startCell;

        Vector2 pos = CellToWorld(cellPos);
        transform.position = pos;
        curPos = pos;

        _isMoving = false;
        _isJumping = false;

        if (_check8Dir == null) _check8Dir = GetComponent<Check8DirectionComponent>();
        if (_check8Dir != null)
        {
            _check8Dir.Update8Direction(cellPos); 
            _check8Dir.DumpArea();
        }
    }

    void Start()
    {
        // 최소 수정: LevelManager가 SetStartCell로 덮어쓰는 구조지만,
        // 혹시 호출 타이밍이 꼬일 때를 대비해 참조만 잡아둠
        if (_level == null)
        {
            _level = FindAnyObjectByType<LevelManager>();
        }

        if (_check8Dir == null)
        {
            _check8Dir = GetComponent<Check8DirectionComponent>();
        }

        if (_check8Dir != null)
        {
            _check8Dir.Update8Direction(new Vector2Int(cellPos.x, cellPos.y));
            _check8Dir.DumpArea();
        }
    }

    void Update()
    {
        GetDirInput();
        if (!_isMoving && !_isJumping)
        {
            if (!TryJumpMove())
            {
                if (!TryStairDownFromUnderfoot()) TryWalkMove();
            }
        }
        UpdatePosition();
    }

    private void GetDirInput()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _dir = DIRECTION.LEFT;
            GetComponentInChildren<SpriteRenderer>().flipX = true;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _dir = DIRECTION.RIGHT;
            GetComponentInChildren<SpriteRenderer>().flipX = false;
        }
        else
        {
            _dir = DIRECTION.NONE;
        }
    }

    // LevelManager의 WorldStart/TileSize 기준으로 셀→월드 변환 (하드 오프셋 제거)
    private Vector2 CellToWorld(Vector2Int cell)
    {
        if (_level == null)
        {
            Debug.LogError("LevelManager가 없습니다!");
        }

        return new Vector2(
            _level.WorldStart.x + (_level.TileSize * cell.x),
            _level.WorldStart.y - (_level.TileSize * cell.y)
        );
    }

    void UpdatePosition()
    {
        if (!_isMoving) return;
        if (_level == null) return;

        Vector3 destPos = CellToWorld(cellPos);
        Vector3 moveDir = destPos - transform.position;

        // 기존 GridStateManager 의존 제거: LevelManager 규칙 사용
        curPos = destPos;

        float dist = moveDir.magnitude;
        if (dist < speed * Time.deltaTime)
        {
            transform.position = destPos;
            _isMoving = false;
            _isJumping = false;

            // 출구 도착 이후 클리어 처리
            if (_waitingReachExit)
            {
                _waitingReachExit = false;
                _level?.OnPlayerReachedExit();
            }
        }
        else
        {
            transform.position += moveDir.normalized * speed * Time.deltaTime;
            _isMoving = true;
        }

        GetComponentInChildren<Animator>().SetBool("isWalking", _isMoving);
    }

    private void TryWalkMove()
    {
        Vector2Int next = cellPos;

        switch (_dir)
        {
            case DIRECTION.LEFT:
                next += Vector2Int.left;
                break;
            case DIRECTION.RIGHT:
                next += Vector2Int.right;
                break;
            default:
                return;
        }

        // 이동 가능 여부는 GridStateManager에게 질의
        if (GridStateManager.i == null) return;

        if (!GridStateManager.i.IsWalkable(next)) return;

        MoveTo(next);
    }

    private bool TryJumpMove()
    {
        Vector2Int stairCell = cellPos;
        Vector2Int jumpCell = cellPos;

        switch (_dir)
        {
            case DIRECTION.LEFT:
                stairCell += Vector2Int.left;
                jumpCell += new Vector2Int(-1, -1);
                break;
            case DIRECTION.RIGHT:
                stairCell += Vector2Int.right;
                jumpCell += new Vector2Int(1, -1);
                break;
            default:
                return false;
        }

        // 이동 가능 여부는 GridStateManager에게 질의
        if (GridStateManager.i == null) return false;

        if (!GridStateManager.i.TryGetState(stairCell, out var stairState)) return false;
        if (stairState != MAP_STATE.STAIR) return false;

        if (!GridStateManager.i.IsWalkable(jumpCell)) return false;

        // 계단 처리
        if (!GridStateManager.i.TryGetStairDir(stairCell, out var sdir)) return false;
        if (_dir == DIRECTION.LEFT && sdir != STAIR_DIR.LEFT) return false;
        if (_dir == DIRECTION.RIGHT && sdir != STAIR_DIR.RIGHT) return false;

        _isJumping = true;
        MoveTo(jumpCell);
        return true;
    }

    private bool IsPassableIgnoreFloor(Vector2Int cell)
    {
        if (GridStateManager.i == null) return false;
        if (!GridStateManager.i.TryGetState(cell, out var s)) return false;

        // 막힌 타일만 제외
        return s != MAP_STATE.STAGE_BLOCK && s != MAP_STATE.BASIC;
    }

    private bool TryStairDownFromUnderfoot()
    {
        if (_dir == DIRECTION.NONE) return false;
        if (GridStateManager.i == null) return false;

        // 발밑이 STAIR인가
        Vector2Int under = cellPos + new Vector2Int(0, 1);
        if (!GridStateManager.i.TryGetState(under, out var underState)) return false;
        if (underState != MAP_STATE.STAIR) return false;

        // 내려갈 목적지: 대각선 아래 (y+1)
        Vector2Int downCell = cellPos + (_dir == DIRECTION.LEFT ? new Vector2Int(-1, 1) : new Vector2Int(1, 1));

        // 계단 내려갈 때는 경사라서 "바닥 조건"을 완화해서 통과만 체크
        if (!IsPassableIgnoreFloor(downCell)) return false;

        if (!GridStateManager.i.TryGetStairDir(under, out var sdir)) return false;

        if (sdir == STAIR_DIR.RIGHT && _dir != DIRECTION.LEFT) return false;
        if (sdir == STAIR_DIR.LEFT && _dir != DIRECTION.RIGHT) return false;

        // 이동 처리
        MoveTo(downCell);
        return true;
    }

    private void MoveTo(Vector2Int next)
    {
        bool isExitMove = false;
        if (GridStateManager.i.TryGetState(next, out var state) && state == MAP_STATE.EXIT)
        {
            isExitMove = true;
        }

        cellPos = next;
        _waitingReachExit = isExitMove;
        _isMoving = true;

        if (_check8Dir != null)
        {
            _check8Dir.Update8Direction(cellPos);
            _check8Dir.DumpArea();
        }
    }

}
