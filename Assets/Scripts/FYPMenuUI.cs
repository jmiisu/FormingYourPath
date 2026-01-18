using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FYPMenuUI : MonoBehaviour
{
    public static FYPMenuUI i;
    [Header("Views")]
    [SerializeField] private GameObject _mainMenuView;
    [SerializeField] private GameObject _stageView;

    [Header("Main Menu Button")]
    [SerializeField] private Button _selectStageButton;
    [SerializeField] private Button _continueButton;

    [Header("Back to Menu Button")]
    [SerializeField] private Button _backButton;
    private void Awake()
    {
        i = this;

        if (_selectStageButton)
        {
            _selectStageButton.onClick.AddListener(OnSelectStageButtonPressed);
        }

        if (_continueButton)
        {
            _continueButton.onClick.AddListener(OnContinueButtonPressed);
        }

        if (_backButton)
        {
            _backButton.onClick.AddListener(OnBackFromStageView);
        }
    }

    public void ShowView(GameObject from, GameObject target)
    {
        if (!target) return;

        from.SetActive(false);
        target.SetActive(true);
    }

    public void OnSelectStageButtonPressed()
    {
        ShowView(_mainMenuView, _stageView);
    }

    public void OnBackFromStageView()
    {
        OnBackButtonPressed(_stageView);
    }

    public void OnBackButtonPressed(GameObject from)
    {
        ShowView(from, _mainMenuView);
    }

    public void OnContinueButtonPressed()
    {
        SceneManager.LoadScene("GameLevel");
    }
}
