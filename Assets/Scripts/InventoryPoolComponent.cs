using UnityEngine;

public class InventoryPoolComponent : MonoBehaviour
{
    public static InventoryPoolComponent i;

    [SerializeField] GameObject basic;
    [SerializeField] GameObject stair;

    private Transform BasicPool => transform.GetChild(0);
    private Transform StairPool => transform.GetChild(1);

    private void Awake()
    {
        i = this;
    }

    public void InitPool(MAP_STATE state, int val = 1)
    {
        Transform parent = GetPool(state);
        GameObject prefab = GetPrefab(state);

        for (int i = 0; i < val; i++)
        {
            GameObject obj = Instantiate(prefab, parent, false);
            obj.SetActive(false);
        }
    }

    public GameObject UseBlock(MAP_STATE state, Vector3 worldPos, Transform level)
    {
        Transform pool = GetPool(state);

        if (pool.childCount == 0)
        {
            Debug.LogWarning("NO BLOCK");
            return null;
        }

        GameObject block = pool.GetChild(0).gameObject;

        block.transform.SetParent(level, true);
        block.transform.position = worldPos;
        block.SetActive(true);

        return block;
    }

    public void RetreiveBlock(MAP_STATE state, GameObject block)
    {
        if (block == null) return;

        Transform pool = GetPool(state);

        block.SetActive(false);
        block.transform.SetParent(pool, false);
    }

    private Transform GetPool(MAP_STATE state)
        => (state == MAP_STATE.STAIR) ? StairPool : BasicPool;

    private GameObject GetPrefab(MAP_STATE state)
        => (state == MAP_STATE.STAIR) ? stair : basic;
}
