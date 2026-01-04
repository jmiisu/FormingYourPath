using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlacementController placement;

    [Header("Inventory Buttons")]
    [SerializeField] private Button[] itemUI;

    [Header("Block Count UI")]
    [SerializeField] private TMP_Text[] countText;

    [Header("Block Count")]
    [SerializeField] private int basicCount = 3;
    [SerializeField] private int stairCount = 1;

    private MAP_STATE selected = MAP_STATE.BASIC;

    private void Awake()
    {
        if (itemUI[0] != null) itemUI[0].onClick.AddListener(SelectBasic);
        if (itemUI[1] != null) itemUI[1].onClick.AddListener(SelectStair);

        RefreshTextUI();
    }

    private void Start()
    {
        ValidateSelection();
    }

    public int GetCount(MAP_STATE state)
    {
        return state == MAP_STATE.BASIC ? basicCount : stairCount;
    }

    public bool TryConsume(MAP_STATE state)
    {
        if (state == MAP_STATE.BASIC)
        {
            if (basicCount <= 0) return false;
            basicCount--;
        }
        else if (state == MAP_STATE.STAIR)
        {
            if (stairCount <= 0) return false;
            stairCount--;
        }
        else return false;

        RefreshTextUI();
        ValidateSelection();
        return true;
    }

    public bool TryRetrieve(MAP_STATE state)
    {
        if (state == MAP_STATE.BASIC)
        {
            basicCount++;
        }
        else if (state == MAP_STATE.STAIR)
        {
            stairCount++;
        }
        else return false;

        RefreshTextUI();
        ValidateSelection();
        return true;
    }

    private void ValidateSelection()
    {
        if (GetCount(selected) >= 0)
        {
            placement?.SetSelectedBlock(selected);
            return;
        }

        if (basicCount >= 0)
        {
            selected = MAP_STATE.BASIC;
            placement?.SetSelectedBlock(selected);
        }
        else if (stairCount >= 0)
        {
            selected = MAP_STATE.STAIR;
            placement?.SetSelectedBlock(selected);
        }
        else
        {

        }
    }

    public void SelectBasic()
    {
        if (basicCount < 0) return;
        selected = MAP_STATE.BASIC;

        placement?.SetSelectedBlock(MAP_STATE.BASIC);
        Debug.Log("Selected: BASIC");
    }

    public void SelectStair()
    {
        if (stairCount < 0) return;
        selected = MAP_STATE.STAIR;

        placement?.SetSelectedBlock(MAP_STATE.STAIR);
        Debug.Log("Selected: STAIR");
    }

    private void RefreshTextUI()
    {
        if (countText[0] != null) countText[0].text = basicCount.ToString();
        if (countText[1] != null) countText[1].text = stairCount.ToString();

        // 0개일 때 버튼 비활성화
        if (itemUI[0] != null) itemUI[0].interactable = (basicCount > 0);
        if (itemUI[1] != null) itemUI[1].interactable = (stairCount > 0);

        
    }

    private void RefreshHighlight()
    {
        
    }
}
