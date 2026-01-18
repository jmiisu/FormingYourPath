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
    bool _waitingReachExit = false; // �ⱸ �������� ���

    public Vector3 curPos { get; private set; } // ���� ��ġ 3 * 3 �ʿ� �ʿ�
    DIRECTION _dir = DIRECTION.LEFT;

    private LevelManager _level;        // LevelManager ����
    private Check8DirectionComponent _check8Dir;

    // LevelManager�� ���� ���� �˷��� �� ȣ��
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
        // �ּ� ����: LevelManager�� SetStartCell�� ����� ��������,
        // Ȥ�� ȣ�� Ÿ�̹��� ���� ���� ����� ������ ��Ƶ�
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

    // LevelManager�� WorldStart/TileSize �������� ������� ��ȯ (�ϵ� ������ ����)
    private Vector2 CellToWorld(Vector2Int cell)
    {
        if (_level == null)
        {
            Debug.LogError("LevelManager�� �����ϴ�!");
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

        // ���� GridStateManager ���� ����: LevelManager ��Ģ ���
        curPos = destPos;

        float dist = moveDir.magnitude;
        if (dist < speed * Time.deltaTime)
        {
            transform.position = destPos;
            _isMoving = false;
            _isJumping = false;

            // �ⱸ ���� ���� Ŭ���� ó��
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

        // �̵� ���� ���δ� GridStateManager���� ����
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

        // �̵� ���� ���δ� GridStateManager���� ����
        if (GridStateManager.i == null) return false;

        if (!GridStateManager.i.TryGetState(stairCell, out var stairState)) return false;
        if (stairState != MAP_STATE.STAIR) return false;

        if (!GridStateManager.i.IsWalkable(jumpCell)) return false;

        // ��� ó��
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

        // ���� Ÿ�ϸ� ����
        return s != MAP_STATE.STAGE_BLOCK && s != MAP_STATE.BASIC;
    }

    private bool TryStairDownFromUnderfoot()
    {
        if (_dir == DIRECTION.NONE) return false;
        if (GridStateManager.i == null) return false;

        // �߹��� STAIR�ΰ�
        Vector2Int under = cellPos + new Vector2Int(0, 1);
        if (!GridStateManager.i.TryGetState(under, out var underState)) return false;
        if (underState != MAP_STATE.STAIR) return false;

        // ������ ������: �밢�� �Ʒ� (y+1)
        Vector2Int downCell = cellPos + (_dir == DIRECTION.LEFT ? new Vector2Int(-1, 1) : new Vector2Int(1, 1));

        // ��� ������ ���� ���� "�ٴ� ����"�� ��ȭ�ؼ� ����� üũ
        if (!IsPassableIgnoreFloor(downCell)) return false;

        if (!GridStateManager.i.TryGetStairDir(under, out var sdir)) return false;

        if (sdir == STAIR_DIR.RIGHT && _dir != DIRECTION.LEFT) return false;
        if (sdir == STAIR_DIR.LEFT && _dir != DIRECTION.RIGHT) return false;

        // �̵� ó��
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
