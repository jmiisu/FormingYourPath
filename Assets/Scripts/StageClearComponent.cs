using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스테이지 클리어 UI 컨트롤러
/// - 패널 Show/Hide
/// - 버튼 클릭 시 StageManager에 위임
/// </summary>
public class StageClearComponent : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private GameObject panel;     // STAGE CLEAR 전체 패널
    [SerializeField] private Button menuButton;   // Go To Menu 버튼
    [SerializeField] private Button nextButton;  // Next Stage 버튼

    private StageManager _stageManager;

    private void Awake()
    {
        // 시작할 때는 안 보이게
        if (panel) panel.SetActive(false);

        // 버튼 이벤트 연결
        if (menuButton) menuButton.onClick.AddListener(OnGoToMenu);
        if (nextButton) nextButton.onClick.AddListener(OnNextStage);
    }

    /// <summary>
    /// StageManager를 주입해서 "다음/메뉴" 처리를 위임
    /// </summary>
    public void Bind(StageManager stageManager)
    {
        _stageManager = stageManager;
    }

    public void Show()
    {
        if (panel) panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnGoToMenu()
    {
        Hide();
        _stageManager?.GoToMenu();
    }

    private void OnNextStage()
    {
        Hide();
        _stageManager?.LoadNextStage();
    }
}
