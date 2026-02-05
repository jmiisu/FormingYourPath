using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private ITEM_TYPE itemType = ITEM_TYPE.NONE;
    public ITEM_TYPE ItemType => itemType;

    private Button btn;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    private TMP_Text countText;


    private void Awake()
    {
        btn = GetComponent<Button>();
        countText = GetComponentInChildren<TMP_Text>(true);
        //Debug.Log(countText);
    }

    public void Bind(Action<ITEM_TYPE> onClick)
    {
        if (btn == null) btn = GetComponent<Button>();

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => 
        {
            FYPSoundManager.i?.PlaySFX(E_SFX.BUTTON_CLICK);
            onClick?.Invoke(itemType);
        });
    }
    
    public void SetCount(int count)
    {
        bool hasAny = count > 0;
        countText.text = count.ToString();

        btn.interactable = hasAny;
        iconImage.enabled = hasAny && itemType != ITEM_TYPE.NONE;

        if (backgroundImage != null)
        {
            Color c = backgroundImage.color;
            backgroundImage.color = hasAny
                ? new Color(c.r, c.g, c.b, 1f)
                : new Color(c.r, c.g, c.b, 0.3f);
        }
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage == null) return;

        backgroundImage.color = selected
            ? new Color(1f, 1f, 1f, 0.7f)
            : new Color(1f, 1f, 1f, 1f);
    }

    public void SetItem(ITEM_TYPE newType, Sprite icon)
    {
        itemType = newType;

        iconImage.sprite = icon;
        iconImage.enabled = (newType != ITEM_TYPE.NONE);
    }
}
