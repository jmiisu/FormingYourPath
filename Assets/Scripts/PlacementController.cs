using Unity.VisualScripting;
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
    [SerializeField] private ItemRegistrySO itemRegistry;

    //[Header("Prefabs")]
    //[SerializeField] private GameObject basicBlockPrefab;
    //[SerializeField] private GameObject stairBlockPrefab;

    [Header("Placement")]
    [SerializeField] private STAIR_DIR stairDir = STAIR_DIR.RIGHT;

    private SpriteRenderer indicatorSR;
    private SpriteRenderer blockCarried;

    private ITEM_TYPE selectedItem = ITEM_TYPE.NONE;
    
    private void Awake()
    {
        //_level = FindAnyObjectByType<LevelManager>();
        indicatorSR = mouseIndicator.GetComponentInChildren<SpriteRenderer>(true);
        mouseIndicator.SetActive(true);  // 항상 켜두고 색으로만 상태 표시

        blockCarried = player.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        //Debug.Log(blockCarried);
        SetCarriedVisible(false);

        if (inventory == null) inventory = FindAnyObjectByType<InventoryController>();
    }
    
    public void SetSelectedItem(ITEM_TYPE item)
    {
        selectedItem = item;
        UpdateCarriedPreview();
    }

    private void UpdateCarriedPreview()
    {
        if (selectedItem == ITEM_TYPE.NONE)
        {
            SetCarriedVisible(false);
            return;
        }
        if (inventory.GetCount(selectedItem) <= 0)
        {
            SetCarriedVisible(false);
            return;
        }

        //if (!blockCarried.enabled) SetCarriedVisible(true);

        Sprite icon = itemRegistry.GetItem(selectedItem)?.icon;

        if (icon == null)
        {
            SetCarriedVisible(false);
            return;
        }

        blockCarried.sprite = icon;
        SetCarriedVisible(true);

        if (selectedItem == ITEM_TYPE.STAIR)
        {
            blockCarried.flipX = (stairDir == STAIR_DIR.LEFT);
        }
        //if (item == ITEM_TYPE.BASIC)
        //{
        //    //blockCarried.sprite = basicBlockPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
        //}
        //else if (item == ITEM_TYPE.STAIR)
        //{
        //    //blockCarried.sprite = stairBlockPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
        //    blockCarried.flipX = (stairDir == STAIR_DIR.LEFT);
        //}
    }

    public void SetCarriedVisible(bool visible)
    {
        if (blockCarried == null) return;
        blockCarried.enabled = visible;
    }

    //public void SetSelectedBlock(MAP_STATE state)
    //{
    //    if (state != MAP_STATE.BASIC && state != MAP_STATE.STAIR) return;
    //    placedState = state;
    //    ShowBlock(placedState);
    //}

    private void TryPlace(Vector2Int cell, Vector3 worldPos)
    {
        if (selectedItem == ITEM_TYPE.NONE) return;

        if (!itemRegistry.TryGetMapState(selectedItem, out MAP_STATE placedState)) return;
        
        if (!inventory.TryConsume(selectedItem)) return;

        // 설치할 맵 상태로 변환
        //MAP_STATE placedState = ItemDatabase.ToMapState(selectedItem);

        // 풀에서 꺼내서 배치
        GameObject placed = InventoryPoolComponent.i.UseItem(selectedItem, worldPos, _level.transform);
        if (placed == null) return;

        // 계단 방향 적용
        if (placedState == MAP_STATE.STAIR)
        {
            StairComponent stairComp = placed.GetComponent<StairComponent>();
            if (stairComp != null) stairComp.SetDir(stairDir);
        }

        // 그리드 등록 + 덮어쓰기
        GridStateManager.i.RegisterPlacedBlock(cell, placedState, placed, out _, out _);
        
        //if (GridStateManager.i.RegisterPlacedBlock(cell, placedState, placed, out var prevObj, out var prevState))
        //{
        //    if (prevObj != null && (prevState == MAP_STATE.BASIC || prevState == MAP_STATE.STAIR))
        //    {
        //        ITEM_TYPE prevItem = (prevState == MAP_STATE.STAIR) ? ITEM_TYPE.STAIR : ITEM_TYPE.BASIC;
        //        InventoryPoolComponent.i.RetreiveItem(prevItem, prevObj);
        //    }
        //}

        Check8DirectionComponent check = player.GetComponent<Check8DirectionComponent>();
        if (check != null)
        {
            check.Update8Direction(cell);
            check.DumpArea();
        }
    }

    private Vector2Int WorldToMapCell(Vector3 world)
    {
        float ts = _level.TileSize;
        Vector3 ws = _level.WorldStart;

        int x = Mathf.RoundToInt((world.x - ws.x) / ts);
        int y = Mathf.RoundToInt((ws.y - world.y) / ts); // y 반전 포함 (아래로 갈수록 +)
        return new Vector2Int(x, y);
    }

    private void Update()
    {
        if (GridStateManager.i == null) return;

        // 1) 마우스 위치(그리드 스냅된 월드 좌표)
        Vector3 worldPos = interactController.GetSelectedMapPosition();

        // 2) 셀 좌표 계산
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

        // 3) 플레이어 주변 6칸만 허용
        // (±1, -1/0/1), 자기 자리(0,0)는 제외
        bool nearRule = (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) <= 1);

        bool inside = GridStateManager.i.IsInside(mouseCell);
        bool hasState = GridStateManager.i.TryGetState(mouseCell, out var state);
        bool isEmpty = hasState && (state == MAP_STATE.EMPTY);

        bool canPlace = nearRule && inside && isEmpty;

        bool isPlacedBlock = hasState && GridStateManager.i.IsThereBlockYouPlaced(mouseCell);
        bool canRemove = nearRule && inside && isPlacedBlock;

        // 4) 색상으로 가능/불가 표시
        if (indicatorSR != null)
        {
            if (canPlace) indicatorSR.color = Color.green;
            else if (canRemove) indicatorSR.color = Color.yellow;
            else indicatorSR.color = Color.red;
        }

        if (selectedItem == ITEM_TYPE.STAIR && Input.GetKeyDown(KeyCode.R))
        {
            stairDir = (stairDir == STAIR_DIR.RIGHT) ? STAIR_DIR.LEFT : STAIR_DIR.RIGHT;
            UpdateCarriedPreview();
        }

        // 5) 좌클릭 시 설치
        if (canPlace && Input.GetMouseButtonDown(0))
        {
            TryPlace(mouseCell, placePos);
        }

        if (canRemove && Input.GetMouseButtonDown(1)) // 우클릭 시 제거 (단, 기존 스테이지 블록은 제외)
        {
            if (GridStateManager.i.TryRemovePlacedBlock(mouseCell, out var removedObj, out var removedState))
            {
                //Debug.Log("REMOVED!");
                if (removedObj != null)
                {
                    ITEM_TYPE removedItem = (removedState == MAP_STATE.STAIR) ? ITEM_TYPE.STAIR : ITEM_TYPE.BASIC;
                    InventoryPoolComponent.i.RetreiveItem(removedItem, removedObj);                    
                }

                ITEM_TYPE retrieveItem = (removedState == MAP_STATE.STAIR) ? ITEM_TYPE.STAIR: ITEM_TYPE.BASIC;
                inventory?.TryRetrieve(retrieveItem);

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


//public void ShowBlock(MAP_STATE state /*int amount*/)
//{
//    if (inventory != null && inventory.GetCount(state) <= 0)
//    {
//        SetCarriedVisible(false);
//        return;
//    }

//    if (!blockCarried.enabled) SetCarriedVisible(true);

//    if (state == MAP_STATE.BASIC)
//    {
//        blockCarried.sprite = basicBlockPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
//    }
//    else if (state == MAP_STATE.STAIR)
//    {
//        blockCarried.sprite = stairBlockPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
//        blockCarried.flipX = (stairDir == STAIR_DIR.LEFT);
//    }
//}

//private GameObject GetSelectedPrefab()
//{
//    return placedState == MAP_STATE.STAIR ? stairBlockPrefab : basicBlockPrefab;
//}