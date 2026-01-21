using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class StageManager : MonoBehaviour
{
    [Header("Stage Settings")] // 스테이지 관련
    [SerializeField] private int totalStageCount = 20;
    [SerializeField] private int currentStageIndex = 0;

    [SerializeField] private StageClearComponent stageClearUI;

    private const string KEY_SELECTED_STAGE = "SelectedStage";
    private const string KEY_UNLOCKED_STAGE = "UnlockedStage";

    private int _unlockedMaxStage = 5;

    private LevelManager _level;

    private void Awake()
    {
        _level = FindAnyObjectByType<LevelManager>();
    }

    void Start()
    {
        // 저장된 해금 정보 불러오기
        _unlockedMaxStage = PlayerPrefs.GetInt(KEY_UNLOCKED_STAGE, 0);
        currentStageIndex = PlayerPrefs.GetInt(KEY_SELECTED_STAGE, 0);

        // LevelManager 클리어 이벤트 구독
        if (_level != null)
        {
            _level.OnStageCleared += HandleStageCleared;
        }

        if (stageClearUI != null)
        {
            stageClearUI.Bind(this);
        }

        LoadStage(currentStageIndex);
    }

    // 스테이지 클리어 시 호출됨
    private void HandleStageCleared()
    {
        Debug.Log($"Stage {currentStageIndex} cleared");

        // 다음 스테이지 해금
        if (currentStageIndex >= _unlockedMaxStage)
        {
            _unlockedMaxStage = currentStageIndex + 1;
            PlayerPrefs.SetInt("UnlockedStage", _unlockedMaxStage);
        }

        // 클리어 팝업 띄우기
        if (stageClearUI != null)
        {
            stageClearUI.Show();
        }
        else
        {
            LoadNextStage();
        }
    }

    // 다음 스테이지 로드
    public void LoadNextStage()
    {
        int nextIndex = currentStageIndex + 1;

        if (nextIndex >= totalStageCount)
        {
            Debug.Log("모든 스테이지 클리어!");
            // 엔딩 / 월드 맵 복귀
            return;
        }

        PlayerPrefs.SetInt(KEY_SELECTED_STAGE, nextIndex);
        LoadStage(nextIndex);
    }

    public void LoadStage(int stageIndex)
    {
        if (stageIndex > _unlockedMaxStage)
        {
            Debug.Log("아직 해금되지 않은 스테이지");
            return;
        }

        currentStageIndex = stageIndex;

        Debug.Log($"Load Stage {currentStageIndex}");

        if (_level != null)
        {
            _level.LoadStage(currentStageIndex);
        }
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
