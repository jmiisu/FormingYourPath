using UnityEngine;

[CreateAssetMenu(menuName = "FYP/Items/Item Definition")]
public class ItemDefinitionSO : ScriptableObject
{
    public ITEM_TYPE type;
    public Sprite icon;
    public GameObject worldPrefab;
    public bool placeable = false;
    public MAP_STATE placeMapState;
}