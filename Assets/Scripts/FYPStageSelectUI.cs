using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FYPStageSelectUI : MonoBehaviour
{
    private Transform tutorialRoot;

    [SerializeField] private string gameSceneName = "GameLevel";
    private const string KEY_SELECTED_STAGE = "SelectedStage";
    private const string KEY_UNLOCKED_STAGE = "UnlockedStage";

    private void Awake()
    {
        tutorialRoot = transform.GetChild(0);
        BindTutorialButtons();
    }

    private void OnEnable()
    {
        // 화면 열릴 때마다 버튼 잠금/해금 상태 갱신
        RefreshLockState();
    }

    private void BindTutorialButtons()
    {
        for (int i = 0; i < tutorialRoot.childCount; i++)
        {
            Transform child = tutorialRoot.GetChild(i);
            Button btn = child.GetComponent<Button>();
            if (btn == null)
            {
                Debug.Log("NO BUTTON");
                continue;
            }

            int stageIndex = ParseStageIndex(child.name);
            int idxCopy = stageIndex;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                // 잠김이면 클릭해도 아무 일 없게(안전장치)
                int unlockedMax = PlayerPrefs.GetInt(KEY_UNLOCKED_STAGE, 0);
                if (idxCopy > unlockedMax) return;

                // 선택 스테이지 저장
                PlayerPrefs.SetInt(KEY_SELECTED_STAGE, stageIndex);

                // 게임 씬으로 이동
                SceneManager.LoadScene(gameSceneName);
            });

        }
    }

    private void RefreshLockState()
    {
        int unlockedMaxStage = PlayerPrefs.GetInt(KEY_UNLOCKED_STAGE, 0);

        for (int i = 0; i < tutorialRoot.childCount; i++)
        {
            Transform child = tutorialRoot.GetChild(i);

            Button btn = child.GetComponent<Button>();
            if (btn == null) continue;

            int stageIndex = ParseStageIndex(child.name);

            bool unlocked = (stageIndex <= unlockedMaxStage);

            // 클릭 가능/불가능
            btn.interactable = unlocked;

            // 시각적으로 어둡게(버튼 Image 색/알파 조절)
            // 버튼 오브젝트에 Image가 있으면 그걸 바꾸고, 없으면 자식 Image를 찾음
            Image img = child.GetComponent<Image>();

            // 밝은 상태: 원래 색 / 잠김: 어둡고 반투명
            Color c = img.color;
            img.color = unlocked
                ? new Color(c.r, c.g, c.b, 1f)
                : new Color(0.35f, 0.35f, 0.35f, 0.6f);
            
        }
    }

    private int ParseStageIndex(string objName)
    {
        int idx = 0;
        int underscore = objName.LastIndexOf('_');
        if (underscore >= 0 && underscore + 1 < objName.Length)
        {
            string number = objName.Substring(underscore + 1);
            int.TryParse(number, out idx);
        }
        return idx;
    }
}
