using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryPoolComponent : MonoBehaviour
{
    public static InventoryPoolComponent i;

    [SerializeField] private ItemRegistrySO itemRegistry;

    private Transform poolRoot;
    //[SerializeField] GameObject basic;
    //[SerializeField] GameObject stair;
    //[SerializeField] GameObject energy;
    //[SerializeField] GameObject pickaxe;

    private readonly Dictionary<ITEM_TYPE, Transform> _poolParents = new();

    //private Transform BasicPool => transform.GetChild(0);
    //private Transform StairPool => transform.GetChild(1);
    //private Transform EtcPool => transform.GetChild(2);

    private void Awake()
    {
        i = this;
        poolRoot = transform;
    }

    private Transform GetPool(ITEM_TYPE item)
    {
        if (_poolParents.TryGetValue(item, out Transform t) && t != null) return t;

        GameObject pool = new GameObject($"{item}");
        pool.transform.SetParent(transform, false);

        t = pool.transform;
        _poolParents[item] = t;
        return t;
    }

    public void InitPool(ITEM_TYPE type, int val = 1)
    {
        if (itemRegistry == null) return;

        GameObject prefab = itemRegistry.GetWorldPrefab(type);
        Transform parent = GetPool(type);

        for (int i = 0; i < val; i++)
        {
            GameObject obj = Instantiate(prefab, parent, false);
            obj.SetActive(false);
        }
    }

    public GameObject UseItem(ITEM_TYPE type, Vector3 worldPos, Transform level)
    {
        Transform pool = GetPool(type);

        if (pool.childCount == 0)
        {
            Debug.LogWarning("NO BLOCK");
            return null;
        }

        GameObject obj = pool.GetChild(0).gameObject;

        obj.transform.SetParent(level, true);
        obj.transform.position = worldPos;
        obj.SetActive(true);

        return obj;
    }

    public void RetreiveItem(ITEM_TYPE type, GameObject obj)
    {
        if (obj == null) return;

        Transform pool = GetPool(type);

        obj.SetActive(false);
        obj.transform.SetParent(pool, false);
    }

    //private Transform GetPool(ITEM_TYPE type)
    //{
    //    return type switch
    //    {
    //        ITEM_TYPE.BASIC => BasicPool,
    //        ITEM_TYPE.STAIR => StairPool,
    //        _ => EtcPool,
    //    };
    //}
        //=> (type == ITEM_TYPE.STAIR) ? StairPool : BasicPool;

    //private GameObject GetPrefab(ITEM_TYPE type)
    //{
    //    return type switch
    //    {
    //        ITEM_TYPE.BASIC => basic,
    //        ITEM_TYPE.STAIR => stair,
    //        ITEM_TYPE.ENERGY => energy,
    //        _ => pickaxe,
    //    };
    //}
        //=> (type == ITEM_TYPE.STAIR) ? stair : basic;
}
