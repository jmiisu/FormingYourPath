using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FYPStaminaUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static FYPStaminaUI i;
    [Header("References")]
    [SerializeField] private Slider fillImage;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private GameObject tooltipRoot;

    private StaminaComponent _stamina;

    public void Bind(StaminaComponent stamina)
    {
        if (stamina != null) stamina.OnStaminaChanged -= HandleChanged;
        
        _stamina = stamina;

        if (stamina != null)
        {
            _stamina.OnStaminaChanged += HandleChanged;
            HandleChanged(_stamina.Current, _stamina.Max);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipRoot != null) tooltipRoot.SetActive(true);
        //throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipRoot != null) tooltipRoot.SetActive(false);
        //throw new System.NotImplementedException();
    }

    private void Awake()
    {
        i = this;
        if (tooltipRoot != null) tooltipRoot.SetActive(false);
    }

    private void HandleChanged(int cur, int max)
    {
        if (fillImage != null)
        {
            fillImage.maxValue = max;
            fillImage.value = max - cur;
        }

        if (tooltipText != null) tooltipText.text = $"{cur}/{max}";
    }
}
