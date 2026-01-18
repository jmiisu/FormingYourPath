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
    [SerializeField] private int basicCount = 4;
    [SerializeField] private int stairCount = 2;

    private MAP_STATE selected = MAP_STATE.BASIC;

    private void Awake()
    {
        if (itemUI[0] != null) itemUI[0].onClick.AddListener(SelectBasic);
        if (itemUI[1] != null) itemUI[1].onClick.AddListener(SelectStair);

        RefreshTextUI();
    }

    private void Start()
    {
        InventoryPoolComponent.i.InitPool(MAP_STATE.BASIC, basicCount);
        InventoryPoolComponent.i.InitPool(MAP_STATE.STAIR, stairCount);
        ValidateSelection();
        RefreshTextUI();
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
            //placement?.SetCarriedVisible(false);
        }
    }

    public void SelectBasic()
    {
        if (basicCount <= 0) return;
        selected = MAP_STATE.BASIC;

        placement?.SetSelectedBlock(MAP_STATE.BASIC);

        RefreshHighlight();
    }

    public void SelectStair()
    {
        if (stairCount <= 0) return;
        selected = MAP_STATE.STAIR;

        placement?.SetSelectedBlock(MAP_STATE.STAIR);

        RefreshHighlight();
    }

    private void RefreshTextUI()
    {
        if (countText[0] != null) countText[0].text = basicCount.ToString();
        if (countText[1] != null) countText[1].text = stairCount.ToString();

        // 0개일 때 버튼 비활성화
        if (itemUI[0] != null) itemUI[0].interactable = (basicCount > 0);
        if (itemUI[1] != null) itemUI[1].interactable = (stairCount > 0);

        RefreshHighlight();
    }

    private void RefreshHighlight()
    {
        // 버튼이 없으면 종료
        if (itemUI == null || itemUI.Length < 2) return;

        // 각 버튼의 Image를 하이라이트 대상으로 사용
        Image basicImg = itemUI[0] != null ? itemUI[0].GetComponent<Image>() : null;
        Image stairImg = itemUI[1] != null ? itemUI[1].GetComponent<Image>() : null;

        // 색이 아니라 다른 방식(Outline/Animator) 쓰고 싶으면 여기만 바꾸면 됨
        // 기본 색: 흰색, 선택 색: 약간 밝게
        Color normal = Color.green;
        Color selectedC = new Color(1f, 1f, 1f, 0.65f); // "하이라이트 느낌"만 주는 값(원하면 바꿔도 됨)

        if (basicImg != null) basicImg.color = (selected == MAP_STATE.BASIC) ? selectedC : normal;
        if (stairImg != null) stairImg.color = (selected == MAP_STATE.STAIR) ? selectedC : normal;
    }
}
