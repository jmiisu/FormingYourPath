using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public enum DIRECTION
{
    LEFT, RIGHT, NONE
};

public class MoveController : MonoBehaviour
{
    [SerializeField] float speed = 1f;

    public Grid gridMap; // (현재 씬에 남아있어도 됨: 최소 수정)
    Vector3Int cellPos = Vector3Int.zero;
    bool _isMoving = false;

    public Vector3 curPos { get; private set; } // 현재 위치 3 * 3 맵에 필요

    DIRECTION _dir = DIRECTION.LEFT;

    private LevelManager _level; // LevelManager 참조

    // LevelManager가 스폰 셀을 알려줄 때 호출
    public void SetStartCell(Vector3Int startCell, LevelManager level)
    {
        _level = level;
        cellPos = startCell;

        Vector3 pos = CellToWorld(cellPos);
        transform.position = pos;
        curPos = pos;

        _isMoving = false;
    }

    void Start()
    {
        // 최소 수정: LevelManager가 SetStartCell로 덮어쓰는 구조지만,
        // 혹시 호출 타이밍이 꼬일 때를 대비해 참조만 잡아둠
        if (_level == null)
            _level = FindAnyObjectByType<LevelManager>();

        // 기존 Start 위치 세팅은 "임시"로만 남겨둠 (SetStartCell이 오면 덮어씀)
        if (_level != null)
        {
            Vector3 pos = CellToWorld(cellPos);
            transform.position = pos;
            curPos = pos;
        }
    }

    void Update()
    {
        GetDirInput();
        UpdatePosition();
        UpdateIsMoving();
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
    private Vector3 CellToWorld(Vector3Int cell)
    {
        if (_level == null)
            return transform.position; // 안전장치

        return new Vector3(
            _level.WorldStart.x + (_level.TileSize * cell.x),
            _level.WorldStart.y - (_level.TileSize * cell.y),
            transform.position.z
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
        }
        else
        {
            transform.position += moveDir.normalized * speed * Time.deltaTime;
            _isMoving = true;
        }

        GetComponentInChildren<Animator>().SetBool("isWalking", _isMoving);
    }

    private void UpdateIsMoving()
    {
        if (_isMoving) return;
        if (_level == null) return;

        Vector3Int next = cellPos;

        switch (_dir)
        {
            case DIRECTION.LEFT:
                next += Vector3Int.left;
                break;
            case DIRECTION.RIGHT:
                next += Vector3Int.right;
                break;
            default:
                return;
        }

        // 이동 가능 여부는 LevelManager에게 질의
        if (!_level.IsWalkable(new Vector2Int(next.x, next.y)))
            return;

        cellPos = next;
        _isMoving = true;
    }
}
