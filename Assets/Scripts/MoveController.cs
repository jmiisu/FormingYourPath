using System;
using UnityEngine;

public enum DIRECTION
{
    LEFT, RIGHT, NONE
};

public class MoveController : MonoBehaviour
{
    [SerializeField] float speed = 1f;

    public Grid gridMap;
    Vector3Int cellPos = Vector3Int.zero;
    bool _isMoving = false;

    DIRECTION _dir = DIRECTION.LEFT;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //gridMap = GameObject.Find("Grid").GetComponent<Grid>();
        Vector3 pos = gridMap.CellToWorld(cellPos) + new Vector3(0.5f, -3.6f);
        //Debug.Log(pos);
        transform.position = pos;
    }

    // Update is called once per frame
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

    void UpdatePosition()
    {
        if (!_isMoving) return;

        Vector3 destPos = gridMap.CellToWorld(cellPos) + new Vector3(0.5f, -3.6f);
        Vector3 moveDir = destPos - transform.position;


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
        if (!_isMoving)
        {
            switch (_dir)
            {
                case DIRECTION.LEFT:
                    cellPos += Vector3Int.left;
                    
                    _isMoving = true;
                    break;
                case DIRECTION.RIGHT:
                    cellPos += Vector3Int.right;
                    _isMoving = true;
                    break;
            }
        }
    }
}
