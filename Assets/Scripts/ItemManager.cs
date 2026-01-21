using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemManager : MonoBehaviour
{
    public static ItemManager i;

    [Header("Item Prefabs")]
    [SerializeField] private GameObject basicItem;
    [SerializeField] private GameObject stairItem;
    [SerializeField] private GameObject energyItem;

    // 현재 스테이지 아이템 데이터
    private Dictionary<Vector2Int, ITEM_STATE> _itemByCell = new();
    private Dictionary<Vector2Int, GameObject> _spawnedObjByCell = new();

    // 아이템 효과를 다른 시스템에 넘기기 위한 이벤트
    public event Action<ITEM_STATE> OnItemPicked;

    public Vector3 WorldStart { get; private set; }
    public float TileSize { get; private set; }

    private void Awake()
    {
        i = this;  
    }

    // LevelManager에서 WorldStart, TileSize 반환 받기
    public void LoadStageItems(int stageIdx, Vector3 worldStart, float tileSize, Transform parent)
    {
        WorldStart = worldStart;
        TileSize = tileSize;

        ClearStageItems();
        BuildItemsFromText(stageIdx, parent);
    }

    private void BuildItemsFromText(int stageIdx, Transform parent)
    {
        string[] itemData = ReadItemText(stageIdx);
        if (itemData.Length == 0) return;

        int width = itemData[0].Length;
        int height = itemData.Length;


        _itemByCell = new Dictionary<Vector2Int, ITEM_STATE>(width * height);
        for (int y = 0; y < height; y++)
        {
            char[] row = itemData[y].ToCharArray();
            for (int x = 0; x < width; x++)
            {
                PlaceItem(row[x].ToString(), x, y, parent);
            }
        }
    }

    public bool TryPickup(Vector2Int playerCell)
    {
        if (!_itemByCell.TryGetValue(playerCell, out var item)) return false;

        _itemByCell.Remove(playerCell);

        if (_spawnedObjByCell.TryGetValue(playerCell, out var obj) && obj != null)
        {
            Destroy(obj);
        }
        _spawnedObjByCell.Remove(playerCell);

        OnItemPicked?.Invoke(item);
        Debug.Log($"[ItemManager] Picked {item} at {playerCell}");
        return true;
    }
    
    private void PlaceItem(string itemType, int x, int y, Transform parent)
    {
        int itemIndex = int.Parse(itemType);
        ITEM_STATE item = (ITEM_STATE)itemIndex;

        Vector2Int cell = new Vector2Int(x, y);
        _itemByCell[cell] = item;

        SpawnItem(item, x, y, parent);
    }

    private void SpawnItem(ITEM_STATE item, int x, int y, Transform parent)
    {
        GameObject prefab = GetPrefab(item);
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, parent, false);

        obj.transform.position = new Vector3(
            WorldStart.x + TileSize * x,
            WorldStart.y - TileSize * y,
            0f
        );

        _spawnedObjByCell[new Vector2Int(x, y)] = obj;
    }


    private GameObject GetPrefab(ITEM_STATE item)
    {
        return item switch
        {
            ITEM_STATE.BASIC => basicItem,
            ITEM_STATE.STAIR => stairItem,
            ITEM_STATE.ENERGY => energyItem,
            _ => null
        };
    }

    private string[] ReadItemText(int stageIdx)
    {
        string itemName = $"ItemText/Tutorial_{stageIdx}";
        string[] lines = ReadTextFile.ReadText(itemName);

        if (lines.Length == 0)
        {
            Debug.LogWarning($"[ItemManager] 아이템 텍스트 없음: {itemName}");
        }

        return lines;
    }
    private void ClearStageItems()
    {
        _itemByCell.Clear();

        foreach (KeyValuePair<Vector2Int, GameObject> kv in _spawnedObjByCell)
        {
            if (kv.Value != null) Destroy(kv.Value);
        }
        _spawnedObjByCell.Clear();
    }
}
