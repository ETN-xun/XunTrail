using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SampleSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject challengeUI;
    public Text progressText;
    public Text upcomingNotesText;
    public Text scoreText;
    public Slider progressSlider;
    
    void Start()
    {
        Debug.Log("SampleSceneManager: SampleScene已加载");
        
        // 查找并设置UI引用
        FindUIElements();
        
        // 检查是否有来自ChallengeDataManager的选中乐谱
        if (ChallengeDataManager.Instance != null)
        {
            MusicSheet selectedSheet = ChallengeDataManager.Instance.GetSelectedMusicSheet();
            if (selectedSheet != null)
            {
                Debug.Log($"SampleSceneManager: 检测到选中的乐谱 - {selectedSheet.name}，启动挑战模式");
                StartChallengeMode(selectedSheet);
            }
            else
            {
                Debug.Log("SampleSceneManager: 没有选中的乐谱，进入自由模式");
                SetupFreeMode();
            }
        }
        else
        {
            Debug.Log("SampleSceneManager: ChallengeDataManager不存在，进入自由模式");
            SetupFreeMode();
        }
    }
    
    private void FindUIElements()
    {
        // 查找ChallengeUI对象（包括非激活的）
        if (challengeUI == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "ChallengeUI" && obj.scene.name != null)
                {
                    challengeUI = obj;
                    Debug.Log("SampleSceneManager: 自动找到ChallengeUI对象");
                    break;
                }
            }
        }
        
        // 查找UI文本组件
        Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
        foreach (Text text in allTexts)
        {
            if (text.gameObject.scene.name != null) // 确保是场景中的对象
            {
                switch (text.name)
                {
                    case "ProgressText":
                        if (progressText == null)
                        {
                            progressText = text;
                            Debug.Log("SampleSceneManager: 找到ProgressText");
                        }
                        break;
                    case "UpcomingNotesText":
                        if (upcomingNotesText == null)
                        {
                            upcomingNotesText = text;
                            Debug.Log("SampleSceneManager: 找到UpcomingNotesText");
                        }
                        break;
                    case "ScoreText":
                        if (scoreText == null)
                        {
                            scoreText = text;
                            Debug.Log("SampleSceneManager: 找到ScoreText");
                        }
                        break;
                }
            }
        }
        
        // 查找进度条
        if (progressSlider == null)
        {
            progressSlider = FindObjectOfType<Slider>();
            if (progressSlider != null)
            {
                Debug.Log("SampleSceneManager: 找到ProgressSlider");
            }
        }
    }
    
    private void SetupFreeMode()
    {
        // 在自由模式下显示基本UI
        if (challengeUI != null)
        {
            challengeUI.SetActive(true);
        }
        
        if (progressText != null)
        {
            progressText.text = "自由演奏模式";
        }
        
        if (upcomingNotesText != null)
        {
            upcomingNotesText.text = "请自由演奏";
        }
        
        if (scoreText != null)
        {
            scoreText.text = "得分: --";
        }
        
        if (progressSlider != null)
        {
            progressSlider.value = 0;
        }
    }
    
    private void StartChallengeMode(MusicSheet musicSheet)
    {
        // 启动挑战模式
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.StartChallenge(musicSheet);
            Debug.Log("SampleSceneManager: 挑战模式已启动");
        }
        else
        {
            Debug.LogWarning("SampleSceneManager: 未找到ChallengeManager实例，无法启动挑战模式");
        }
    }
    
    // 返回挑战选择界面
    public void BackToChallengeScene()
    {
        Debug.Log("SampleSceneManager: 返回挑战选择界面");
        
        // 清除选中的乐谱
        if (ChallengeDataManager.Instance != null)
        {
            ChallengeDataManager.Instance.SetSelectedMusicSheet(null);
        }
        
        SceneManager.LoadScene("ChallengeScene");
    }
    
    // 返回主菜单
    public void BackToMainMenu()
    {
        Debug.Log("SampleSceneManager: 返回主菜单");
        
        // 清除选中的乐谱
        if (ChallengeDataManager.Instance != null)
        {
            ChallengeDataManager.Instance.SetSelectedMusicSheet(null);
        }
        
        SceneManager.LoadScene("TitleScene");
    }
    
    // 新增方法：更新得分显示
    public void UpdateScoreDisplay(float score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"得分: {score:F1}%";
            Debug.Log($"SampleSceneManager: 更新得分显示为 {score:F1}%");
        }
        else
        {
            Debug.LogWarning("SampleSceneManager: scoreText为null，无法更新得分显示");
        }
    }
    
    // 新增方法：获取ScoreText组件的引用
    public Text GetScoreText()
    {
        return scoreText;
    }
}