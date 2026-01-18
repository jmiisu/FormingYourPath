using UnityEngine;

public class PlacementController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject mouseIndicator;
    [SerializeField] private InteractController interactController;
    //[SerializeField] private Grid gridMap;
    [SerializeField] private Transform player;
    [SerializeField] private LevelManager _level;

    [Header("Inventory")]
    [SerializeField] private InventoryController inventory;

    [Header("Prefabs")]
    [SerializeField] private GameObject basicBlockPrefab;
    [SerializeField] private GameObject stairBlockPrefab;

    [Header("Placement")]
    [SerializeField] private MAP_STATE placedState = MAP_STATE.BASIC;
    [SerializeField] private STAIR_DIR stairDir = STAIR_DIR.RIGHT;

    private SpriteRenderer indicatorSR;
    private SpriteRenderer blockCarried;

    private void Awake()
    {
        //_level = FindAnyObjectByType<LevelManager>();
        if (mouseIndicator != null)
        {
            indicatorSR = mouseIndicator.GetComponentInChildren<SpriteRenderer>(true);
            mouseIndicator.SetActive(true);  // �׻� �ѵΰ� �����θ� ���� ǥ��
        }

        blockCarried = player.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        Debug.Log(blockCarried);
    }

    public void SetCarriedVisible(bool visible)
    {
        if (blockCarried == null) return;
        blockCarried.enabled = visible;
    }

    public void SetSelectedBlock(MAP_STATE state)
    {
        if (state != MAP_STATE.BASIC && state != MAP_STATE.STAIR) return;
        placedState = state;
        ShowBlock(placedState);
    }

    public void ShowBlock(MAP_STATE state /*int amount*/)
    {
        if (inventory != null && inventory.GetCount(state) <= 0)
        {
            SetCarriedVisible(false);
            return;
        }

        if (!blockCarried.enabled) SetCarriedVisible(true);

        if (state == MAP_STATE.BASIC)
        {
            blockCarried.sprite = basicBlockPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
        }
        else if (state == MAP_STATE.STAIR)
        {
            blockCarried.sprite = stairBlockPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
            blockCarried.flipX = (stairDir == STAIR_DIR.LEFT);
        }
    }

    private GameObject GetSelectedPrefab()
    {
        return placedState == MAP_STATE.STAIR ? stairBlockPrefab : basicBlockPrefab;
    }

    private Vector2Int WorldToMapCell(Vector3 world)
    {
        float ts = _level.TileSize;
        Vector3 ws = _level.WorldStart;

        int x = Mathf.RoundToInt((world.x - ws.x) / ts);
        int y = Mathf.RoundToInt((ws.y - world.y) / ts); // y ���� ���� (�Ʒ��� ������ +)
        return new Vector2Int(x, y);
    }

    private void Update()
    {
        if (mouseIndicator == null || interactController == null || player == null)
            return;

        if (GridStateManager.i == null) return;

        // 1) ���콺 ��ġ(�׸��� ������ ���� ��ǥ)
        Vector3 worldPos = interactController.GetSelectedMapPosition();

        // 2) �� ��ǥ ���

        Vector2Int mouseCell = WorldToMapCell(worldPos);
        Vector2Int playerCell = WorldToMapCell(player.position);

        Vector3 placePos = new Vector3(
            _level.WorldStart.x + _level.TileSize * mouseCell.x,
            _level.WorldStart.y - _level.TileSize * mouseCell.y,
            0
        );

        mouseIndicator.transform.position = placePos;

        int dx = mouseCell.x - playerCell.x;
        int dy = mouseCell.y - playerCell.y;

        // 3) �÷��̾� �ֺ� 6ĭ�� ���
        // (��1, -1/0/1), �ڱ� �ڸ�(0,0)�� ����
        bool nearRule = (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) <= 1);

        bool inside = GridStateManager.i.IsInside(mouseCell);
        bool hasState = GridStateManager.i.TryGetState(mouseCell, out var state);
        bool isEmpty = hasState && (state == MAP_STATE.EMPTY);

        bool canPlace = nearRule && inside && isEmpty;

        bool isPlacedBlock = hasState && GridStateManager.i.IsThereBlockYouPlaced(mouseCell);
        bool canRemove = nearRule && inside && isPlacedBlock;

        // 4) �������� ����/�Ұ� ǥ��
        if (indicatorSR != null)
        {
            if (canPlace) indicatorSR.color = Color.green;
            else if (canRemove) indicatorSR.color = Color.yellow;
            else indicatorSR.color = Color.red;
        }

        if (placedState == MAP_STATE.STAIR && Input.GetKeyDown(KeyCode.R))
        {
            stairDir = (stairDir == STAIR_DIR.RIGHT) ? STAIR_DIR.LEFT : STAIR_DIR.RIGHT;
            ShowBlock(MAP_STATE.STAIR);
        }

        // 5) ��Ŭ�� �� ��ġ
        if (canPlace && Input.GetMouseButtonDown(0))
        {
            if (inventory != null && !inventory.TryConsume(placedState))
            {
                // 0���϶� ����
                SetCarriedVisible(false);
                return;
            }

            // Ǯ���� ������ ��ġ
            GameObject placed = InventoryPoolComponent.i.UseBlock(placedState, placePos, _level.transform);
            //GameObject blockPlaced = GetSelectedPrefab();

            // ��� ���� ����
            if (placedState == MAP_STATE.STAIR)
            {
                var stairComp = placed.GetComponent<StairComponent>();
                if (stairComp != null) stairComp.SetDir(stairDir);
            }

            // �׸��� ��� + �����
            if (GridStateManager.i.RegisterPlacedBlock(mouseCell, placedState, placed, out var prevObj, out var prevState))
            {
                if (prevObj != null && (prevState == MAP_STATE.BASIC || prevState == MAP_STATE.STAIR))
                {
                    InventoryPoolComponent.i.RetreiveBlock(prevState, prevObj);
                }
            }

            Check8DirectionComponent check = player.GetComponent<Check8DirectionComponent>();
            if (check != null)
            {
                check.Update8Direction(playerCell);
                check.DumpArea();
            }
        }

        if (canRemove && Input.GetMouseButtonDown(1)) // ��Ŭ�� �� ���� (��, ���� �������� ������ ����)
        {
            if (GridStateManager.i.TryRemovePlacedBlock(mouseCell, out var removedObj, out var removedState))
            {
                //Debug.Log("REMOVED!");
                if (removedObj != null)
                {
                    InventoryPoolComponent.i.RetreiveBlock(state, removedObj);                    
                }

                inventory?.TryRetrieve(removedState);

                var check = player.GetComponent<Check8DirectionComponent>();

                if (check != null)
                {
                    check.Update8Direction(playerCell);
                    check.DumpArea();
                }
            }
        }
    }
}
