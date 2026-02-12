using Unity.VisualScripting;
using UnityEngine;

public class PlacementController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject mouseIndicator;

    [Header("TargetingSystem")]
    [SerializeField] private MonoBehaviour targetProviderBehaviour;
    private IGridTargetProvider targetProvider;

    [Header("CarriedItem")]
    [SerializeField] private CarriedItemComponent hand;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private LevelManager _level;
    //[SerializeField] private InteractController _interact;
    //[SerializeField] private Grid gridMap;

    [Header("Inventory")]
    [SerializeField] private InventoryController inventory;
    [SerializeField] private ItemRegistrySO itemRegistry;

    private SpriteRenderer indicatorSR;
    private SpriteRenderer blockCarried;

    private bool placementEnabled = true;

    //[Header("Prefabs")]
    //[SerializeField] private GameObject basicBlockPrefab;
    //[SerializeField] private GameObject stairBlockPrefab;

    //[Header("Placement")]
    //[SerializeField] private STAIR_DIR stairDir = STAIR_DIR.RIGHT;

    //private ITEM_TYPE selectedItem = ITEM_TYPE.NONE;

    private void Awake()
    {
        targetProvider = targetProviderBehaviour as IGridTargetProvider;

        indicatorSR = mouseIndicator.GetComponentInChildren<SpriteRenderer>(true);
        mouseIndicator.SetActive(true);  // 항상 켜두고 색으로만 상태 표시

        blockCarried = player.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        
        if (inventory == null) inventory = FindAnyObjectByType<InventoryController>();

        if (hand != null) hand.OnChanged += UpdateCarriedPreview;
        
        SetCarriedVisible(false);
        UpdateCarriedPreview();
        
        //_level = FindAnyObjectByType<LevelManager>();
        //Debug.Log(blockCarried);
    }

    private void OnDestroy()
    {
        if (hand != null) hand.OnChanged -= UpdateCarriedPreview;
    }
    public void SetPlacementEnabled(bool enabled)
    {
        placementEnabled = enabled;
    }

    /// <summary>
    /// 다른 코드/버튼에서 호출 가능
    /// hand로 위임만 함!
    /// </summary>

    public void SetSelectedItem(ITEM_TYPE item)
    {
        if (hand == null) return;
        hand.SetCarried(item);
    }

    //public void SetCarriedBlock(ITEM_TYPE item) => SetSelectedItem(item);

    public void SetSelectedBlock(MAP_STATE state)
    {
        switch (state)
        {
            case MAP_STATE.BASIC: 
                SetSelectedItem(ITEM_TYPE.BASIC); 
                break;
            case MAP_STATE.STAIR:
                SetSelectedItem(ITEM_TYPE.STAIR);
                break;
            default:
                SetSelectedItem(ITEM_TYPE.NONE);
                break;
        }
    }

    //public void ShowBlock(MAP_STATE state) => SetSelectedBlock(state);

    public void SetCarriedVisible(bool visible)
    {
        if (blockCarried == null) return;
        blockCarried.enabled = visible;
    }

    private void Update()
    {
        //if (GridStateManager.i == null) return;
        if (!placementEnabled) return;
        if (hand == null) return;

        if (hand.Mode != CARRIED_MODE.PLACEMENT) return;

        if (targetProvider == null) return;
        if (!targetProvider.TryGetTarget(out var info)) return;

        /*
         * // 1) 마우스 위치(그리드 스냅된 월드 좌표)
        Vector3 worldPos = interactController.GetSelectedMapPosition();

        // 2) 셀 좌표 계산
        Vector2Int mouseCell = WorldToMapCell(worldPos);
        Vector2Int playerCell = WorldToMapCell(player.position);

        Vector3 placePos = new Vector3(
            _level.WorldStart.x + _level.TileSize * mouseCell.x,
            _level.WorldStart.y - _level.TileSize * mouseCell.y,
            0
        );
         */

        mouseIndicator.transform.position = info.worldPos;

        /*
        int dx = mouseCell.x - playerCell.x;
        int dy = mouseCell.y - playerCell.y;

        // 3) 플레이어 주변 6칸만 허용
        // (±1, -1/0/1), 자기 자리(0,0)는 제외
        bool nearRule = (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) <= 1);

        bool inside = GridStateManager.i.IsInside(mouseCell);
        bool hasState = GridStateManager.i.TryGetState(mouseCell, out var state);
        bool isEmpty = hasState && (state == MAP_STATE.EMPTY);

        bool isPlacedBlock = hasState && GridStateManager.i.IsThereBlockYouPlaced(mouseCell); 
         */

        bool canPlace = info.nearRule && info.inside && info.isEmpty;
        bool canRemove = info.nearRule && info.inside && info.isPlacedBlock;

        // 색상으로 가능/불가 표시
        if (indicatorSR != null)
        {
            if (canPlace) indicatorSR.color = Color.green;
            else if (canRemove) indicatorSR.color = Color.yellow;
            else indicatorSR.color = Color.red;
        }

        if (hand.Carried == ITEM_TYPE.STAIR && Input.GetKeyDown(KeyCode.R))
        {
            hand.ToggleStairDir();
            // UpdateCarriedPreview 자동 호출
        }

        // 좌클릭 시 설치
        if (canPlace && Input.GetMouseButtonDown(0))
        {
            TryPlace(info.cell, info.worldPos, info.playerCell);
        }

        if (canRemove && Input.GetMouseButtonDown(1)) // 우클릭 시 제거 (단, 기존 스테이지 블록은 제외)
        {
            TryRemovePlaced(info.cell, info.playerCell);
        }
    }

    private void TryPlace(Vector2Int cell, Vector3 worldPos, Vector2Int playerCell)
    {
        ITEM_TYPE selectedItem = hand.Carried;
        if (selectedItem == ITEM_TYPE.NONE) return;

        if (!inventory.TryConsume(selectedItem)) return;
        if (!itemRegistry.TryGetMapState(selectedItem, out MAP_STATE placedState)) return;

        // 설치할 맵 상태로 변환
        //MAP_STATE placedState = ItemDatabase.ToMapState(selectedItem);

        // 풀에서 꺼내서 배치
        GameObject placed = InventoryPoolComponent.i.UseItem(selectedItem, worldPos, _level.transform);
        if (placed == null) return;

        FYPSoundManager.i?.PlaySFX(E_SFX.PLACE);

        // 계단 방향 적용
        if (placedState == MAP_STATE.STAIR)
        {
            StairComponent stairComp = placed.GetComponent<StairComponent>();
            if (stairComp != null) stairComp.SetDir(hand.StairDir);
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

        UpdatePlayerCheck8Dir(playerCell);
        UpdateCarriedPreview();
    }

    private void TryRemovePlaced(Vector2Int cell, Vector2Int playerCell)
    {
        if (GridStateManager.i.TryRemovePlacedBlock(cell, out var removedObj, out var removedState))
        {
            FYPSoundManager.i?.PlaySFX(E_SFX.REMOVE);
            //Debug.Log("REMOVED!");
            if (removedObj != null)
            {
                ITEM_TYPE removedItem = (removedState == MAP_STATE.STAIR) ? ITEM_TYPE.STAIR : ITEM_TYPE.BASIC;
                InventoryPoolComponent.i.RetreiveItem(removedItem, removedObj);
            }

            ITEM_TYPE retrieveItem = (removedState == MAP_STATE.STAIR) ? ITEM_TYPE.STAIR : ITEM_TYPE.BASIC;
            inventory?.TryRetrieve(retrieveItem);

            UpdatePlayerCheck8Dir(playerCell    );
            UpdateCarriedPreview();
        }
    }

    private void UpdateCarriedPreview()
    {
        SetCarriedVisible(false);
        if (hand == null || blockCarried == null) return;

        if (hand.Mode != CARRIED_MODE.PLACEMENT) return;

        ITEM_TYPE item = hand.Carried;
        if (inventory.GetCount(item) <= 0) return;
        
        Sprite icon = itemRegistry.GetItem(item)?.icon;
        if (icon == null) return;
        
        blockCarried.sprite = icon;
        blockCarried.flipX = (item == ITEM_TYPE.STAIR && hand.StairDir == STAIR_DIR.LEFT);
        SetCarriedVisible(true);

        /*
        if (selectedItem == ITEM_TYPE.STAIR)
        {
            blockCarried.flipX = (stairDir == STAIR_DIR.LEFT);
        }
        if (item == ITEM_TYPE.BASIC)
        {
            //blockCarried.sprite = basicBlockPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
        }
        else if (item == ITEM_TYPE.STAIR)
        {
            //blockCarried.sprite = stairBlockPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
            blockCarried.flipX = (stairDir == STAIR_DIR.LEFT);
        } 
         */
    }

    private void UpdatePlayerCheck8Dir(Vector2Int playerCell)
    {
        if (player == null) return;

        Check8DirectionComponent check = player.GetComponent<Check8DirectionComponent>();
        if (check == null) return;

        check.Update8Direction(playerCell);
        check.DumpArea();
    }
}

//private Vector2Int WorldToMapCell(Vector3 world)
//{
//    float ts = _level.TileSize;
//    Vector3 ws = _level.WorldStart;

//    int x = Mathf.RoundToInt((world.x - ws.x) / ts);
//    int y = Mathf.RoundToInt((ws.y - world.y) / ts); // y 반전 포함 (아래로 갈수록 +)
//    return new Vector2Int(x, y);
//}

//public void SetSelectedBlock(MAP_STATE state)
//{
//    if (state != MAP_STATE.BASIC && state != MAP_STATE.STAIR) return;
//    placedState = state;
//    ShowBlock(placedState);
//}

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