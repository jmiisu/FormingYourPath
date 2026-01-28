using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FYP/Items/Item Registry")]
public class ItemRegistrySO : ScriptableObject
{
    public List<ItemDefinitionSO> items = new();

    private Dictionary<ITEM_TYPE, ItemDefinitionSO> _map;

    private void BuildMap()
    {
        if (_map != null) return;
        
        _map = new Dictionary<ITEM_TYPE, ItemDefinitionSO>();
        foreach (ItemDefinitionSO it in items)
        {
            if (it != null) _map[it.type] = it;
        }
    }

    public ItemDefinitionSO GetItem(ITEM_TYPE type)
    {
        BuildMap();
        _map.TryGetValue(type, out ItemDefinitionSO def);
        return def;
    }

    public Sprite GetIcon(ITEM_TYPE type) => GetItem(type).icon;

    public GameObject GetWorldPrefab(ITEM_TYPE type) => GetItem(type).worldPrefab;
 
    public bool TryGetMapState(ITEM_TYPE type, out MAP_STATE mapState)
    {
        ItemDefinitionSO def = GetItem(type);
        mapState = MAP_STATE.EMPTY;
        
        if (def == null) return false;
        if (!def.placeable) return false;

        mapState = def.placeMapState;
        return true;
    }
}
