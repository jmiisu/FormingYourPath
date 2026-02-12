using System;
using UnityEngine;

public enum CARRIED_MODE 
{ 
    NONE, 
    PLACEMENT, 
    MINING 
}
public class CarriedItemComponent : MonoBehaviour
{
    public event Action OnChanged;

    public ITEM_TYPE Carried { get; private set; } = ITEM_TYPE.NONE;
    public STAIR_DIR StairDir { get; private set; } = STAIR_DIR.RIGHT;

    public CARRIED_MODE Mode
    {
        get
        {
            if (Carried == ITEM_TYPE.PICKAXE) return CARRIED_MODE.MINING;
            if (Carried == ITEM_TYPE.BASIC || Carried == ITEM_TYPE.STAIR) return CARRIED_MODE.PLACEMENT;
            return CARRIED_MODE.NONE;
        }
    }

    public void SetCarried(ITEM_TYPE item)
    {
        if (Carried == item) return;
        Carried = item;
        OnChanged?.Invoke();
    }

    public void ToggleStairDir()
    {
        StairDir = (StairDir == STAIR_DIR.RIGHT) ? STAIR_DIR.LEFT : STAIR_DIR.RIGHT;
        OnChanged?.Invoke();
    }
}
