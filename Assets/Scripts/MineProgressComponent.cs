using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MineProgressComponent : MonoBehaviour
{
    [SerializeField] private MineComponent mine;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text remainTime;
    [SerializeField] private Image progressBar;
    //private float maxTime;

    private void OnEnable()
    {
        if (mine != null) mine.OnMineProgress += HandleMineProgress;
        SetVisible(false);
    }

    private void OnDisable()
    {
        if (mine != null) mine.OnMineProgress -= HandleMineProgress;
    }

    private void HandleMineProgress(float progress01)
    {
        Debug.Log($"panel:{panel}, bar:{progressBar}, text:{remainTime}, mine:{mine}");
        progress01 = Mathf.Clamp01(progress01);
        //bool visible = progress01 > 0f;

        SetVisible(progress01 > 0f);

        progressBar.fillAmount = progress01;

        float maxTime = (mine != null) ? mine.RequiredHoldTime : 3f;
        float remain = (1f - progress01) * maxTime;
        remainTime.text = ((int)remain).ToString();

        //transform.GetChild(0).gameObject.SetActive(progress01 > 0f);
    }

    private void SetVisible(bool visible)
    {
        if (panel != null && panel.activeSelf != visible) panel.SetActive(visible);
    }
}
