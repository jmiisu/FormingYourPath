using UnityEngine;

public static class ItemDatabase
{
    public static MAP_STATE ToMapState(ITEM_TYPE item)
    {
        return item switch
        {
            ITEM_TYPE.BASIC => MAP_STATE.BASIC,
            ITEM_TYPE.STAIR => MAP_STATE.STAIR,
            _ => MAP_STATE.EMPTY
        };
    }

    public static ITEM_TYPE ToItemType(ITEM_STATE itemState)
    {
        return itemState switch
        {
            ITEM_STATE.BASIC => ITEM_TYPE.BASIC,
            ITEM_STATE.STAIR => ITEM_TYPE.STAIR,
            ITEM_STATE.ENERGY => ITEM_TYPE.ENERGY,
            ITEM_STATE.PICKAXE => ITEM_TYPE.PICKAXE,
            _ => ITEM_TYPE.NONE
        };
    }
}
