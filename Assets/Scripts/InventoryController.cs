using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlacementController placement;
    [SerializeField] private ItemRegistrySO itemRegistry;

    [Header("Slots")]
    [SerializeField] private InventorySlotUI[] slots;


    private Dictionary<ITEM_STATE, int> itemCount;
    [SerializeField] private int basicCount = 4;
    [SerializeField] private int stairCount = 2;
    [SerializeField] private int energyCount = 0;
    [SerializeField] private int pickaxeCount = 0;

    private ITEM_TYPE selected = ITEM_TYPE.BASIC;

    private void Awake()
    {
        foreach (InventorySlotUI slot in slots)
        {
            if (slot == null) continue;
            slot.Bind(Select);

            if (slot.ItemType == ITEM_TYPE.NONE) continue;

            Sprite icon = itemRegistry.GetIcon(slot.ItemType);
            slot.SetItem(slot.ItemType, icon);
        }
    }

    private void Start()
    {
        if (ItemManager.i != null)
        {
            ItemManager.i.OnItemPicked -= HandleItemPicked;
            ItemManager.i.OnItemPicked += HandleItemPicked;
        }
        InventoryPoolComponent.i.InitPool(ITEM_TYPE.BASIC, basicCount);
        InventoryPoolComponent.i.InitPool(ITEM_TYPE.STAIR, stairCount);

        RefreshUI();
        Select(selected);
    }
    private void OnEnable()
    {
        if (ItemManager.i != null) ItemManager.i.OnItemPicked += HandleItemPicked;
    }

    private void OnDisable()
    {
        if (ItemManager.i != null) ItemManager.i.OnItemPicked -= HandleItemPicked;
    }

    public int GetEnergyCount() => energyCount; // 이동 횟수 증가 연결용
    
    private void HandleItemPicked(ITEM_STATE picked)
    {
        ITEM_TYPE item = ItemDatabase.ToItemType(picked);
        switch (item)
        {
            case ITEM_TYPE.BASIC:
                basicCount++;
                InventoryPoolComponent.i?.InitPool(ITEM_TYPE.BASIC, 1);
                break;
            case ITEM_TYPE.STAIR:
                stairCount++;
                InventoryPoolComponent.i?.InitPool(ITEM_TYPE.STAIR, 1);
                break;
            case ITEM_TYPE.ENERGY:
                energyCount++;
                InventoryPoolComponent.i?.InitPool(ITEM_TYPE.ENERGY, 1);
                break;
            case ITEM_TYPE.PICKAXE:
                pickaxeCount++;
                InventoryPoolComponent.i?.InitPool(ITEM_TYPE.PICKAXE, 1);
                break;
        }

        EnsureSlotHasItem(item);

        ValidateSelection();
        RefreshUI();
    }

    public void AddItem(ITEM_TYPE item, int amount)
    {
        if (amount <= 0) return;

        switch (item)
        {
            case ITEM_TYPE.BASIC:
                basicCount += amount;
                InventoryPoolComponent.i?.InitPool(ITEM_TYPE.BASIC, amount);
                break;
            case ITEM_TYPE.STAIR:
                stairCount += amount;
                InventoryPoolComponent.i?.InitPool(ITEM_TYPE.STAIR, amount);
                break;
            default:
                break;
        }

        EnsureSlotHasItem(item);
        ValidateSelection(); 
        RefreshUI();
    }

    public bool TryConsume(ITEM_TYPE item)
    {
        switch (item)
        {
            case ITEM_TYPE.BASIC:
                if (basicCount <= 0) return false;
                basicCount--;
                break;
            case ITEM_TYPE.STAIR:
                if (stairCount <= 0) return false;
                stairCount--;
                break;
            default:
                return false;
        }

        ValidateSelection();
        RefreshUI();
        return true;
    }

    public bool TryRetrieve(ITEM_TYPE item)
    {
        switch (item)
        {
            case ITEM_TYPE.BASIC: basicCount++; break;
            case ITEM_TYPE.STAIR: stairCount++; break;
            default: return false;
        }

        ValidateSelection();
        RefreshUI();
        return true;
    }

    public int GetCount(ITEM_TYPE item)
    {
        return item switch
        {
            ITEM_TYPE.BASIC => basicCount,
            ITEM_TYPE.STAIR => stairCount,
            ITEM_TYPE.ENERGY => energyCount,
            ITEM_TYPE.PICKAXE => pickaxeCount,
            _ => 0
        };
    }

    private void EnsureSlotHasItem(ITEM_TYPE type)
    {
        if (itemRegistry == null) return;

        InventorySlotUI slot = FindSlot(type);
        if (slot == null) return;

        if (slot.ItemType == type) return;

        Sprite icon = itemRegistry.GetIcon(type);
        slot.SetItem(type, icon);

        slot.Bind(Select);
    }

    private void Select(ITEM_TYPE item)
    {
        if (GetCount(item) <= 0) return;

        selected = item;
        placement?.SetSelectedItem(selected);
        RefreshUI();
    }

    private void ValidateSelection()
    {
        if (GetCount(selected) >= 0)
        {
            placement?.SetSelectedItem(selected);
            return;
        }

        if (basicCount >= 0)
        {
            selected = ITEM_TYPE.BASIC;
            placement?.SetSelectedItem(selected);
        }
        else if (stairCount >= 0)
        {
            selected = ITEM_TYPE.STAIR;
            placement?.SetSelectedItem(selected);
        }
        else
        {
            //placement?.SetCarriedVisible(false);
        }
    }

    private void RefreshUI()
    {
        if (slots == null) return;

        foreach (InventorySlotUI slot in slots)
        {
            if (slot == null) continue;

            int cnt = GetCount(slot.ItemType);
            slot.SetCount(cnt);
            slot.SetSelected(slot.ItemType == selected);
        }
    }

    private InventorySlotUI FindSlot(ITEM_TYPE type)
    {
        foreach (InventorySlotUI slot in slots)
        {
            if (slot != null && slot.ItemType == type) return slot;
        }

        foreach (InventorySlotUI slot in slots)
        {
            if (slot != null && slot.ItemType == ITEM_TYPE.NONE) return slot;
        }

        return null;
    }
    
    public bool TryConsumeEnergy()
    {
        if (energyCount <= 0) return false;
        energyCount--;
        RefreshUI();
        return true;
    }
}
//private void Start()
//{
//    if (ItemManager.i != null)
//    {
//        ItemManager.i.OnItemPicked += HandleItemPicked;
//    }

//    InventoryPoolComponent.i.InitPool(ITEM_TYPE.BASIC, basicCount);
//    InventoryPoolComponent.i.InitPool(ITEM_TYPE.STAIR, stairCount);

//    ValidateSelection();
//    RefreshUI();
//}
//private void OnDestroy()
//{
//    if (ItemManager.i != null) ItemManager.i.OnItemPicked -= HandleItemPicked;
//}







//public void SelectBasic()
//{
//    if (basicCount <= 0) return;
//    selected = MAP_STATE.BASIC;

//    placement?.SetSelectedBlock(MAP_STATE.BASIC);

//    RefreshUI();
//}

//public void SelectStair()
//{
//    if (stairCount <= 0) return;
//    selected = MAP_STATE.STAIR;

//    placement?.SetSelectedBlock(MAP_STATE.STAIR);

//    RefreshUI();
//}

//private void RefreshTextUI()
//{
//    if (countText[0] != null) countText[0].text = basicCount.ToString();
//    if (countText[1] != null) countText[1].text = stairCount.ToString();

//    if (countText[2] != null) countText[2].text = energyCount.ToString();

//    // 0개일 때 버튼 비활성화
//    if (itemUI[0] != null) itemUI[0].interactable = (basicCount > 0);
//    if (itemUI[1] != null) itemUI[1].interactable = (stairCount > 0);
//    if (itemUI[2] != null) itemUI[2].interactable = (energyCount > 0);
//    RefreshHighlight();
//}

//private void RefreshHighlight()
//{
//    // 버튼이 없으면 종료
//    if (itemUI == null || itemUI.Length < 2) return;

//    // 각 버튼의 Image를 하이라이트 대상으로 사용
//    Image basicImg = itemUI[0] != null ? itemUI[0].GetComponent<Image>() : null;
//    Image stairImg = itemUI[1] != null ? itemUI[1].GetComponent<Image>() : null;

//    // 색이 아니라 다른 방식(Outline/Animator) 쓰고 싶으면 여기만 바꾸면 됨
//    // 기본 색: 흰색, 선택 색: 약간 밝게
//    Color normal = Color.green;
//    Color selectedC = new Color(1f, 1f, 1f, 0.65f); // "하이라이트 느낌"만 주는 값(원하면 바꿔도 됨)

//    if (basicImg != null) basicImg.color = (selected == MAP_STATE.BASIC) ? selectedC : normal;
//    if (stairImg != null) stairImg.color = (selected == MAP_STATE.STAIR) ? selectedC : normal;
//}