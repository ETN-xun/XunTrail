using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        
        // 检查游戏模式
        if (GameModeManager.Instance != null)
        {
            Debug.Log($"SampleSceneManager: 当前游戏模式 - {GameModeManager.Instance.currentMode}");
            
            if (GameModeManager.Instance.IsTutorialMode())
            {
                Debug.Log("SampleSceneManager: 检测到教程模式");
                SetupTutorialMode();
            }
            else if (GameModeManager.Instance.IsChallengeMode())
            {
                Debug.Log("SampleSceneManager: 检测到挑战模式");
                if (ChallengeDataManager.Instance != null)
                {
                    MusicSheet selectedSheet = ChallengeDataManager.Instance.GetSelectedMusicSheet();
                    if (selectedSheet != null)
                    {
                        Debug.Log($"SampleSceneManager: 启动挑战模式，乐谱: {selectedSheet.name}");
                        StartChallengeMode(selectedSheet);
                    }
                    else
                    {
                        Debug.LogWarning("SampleSceneManager: 挑战模式但没有选中乐谱，回退到自由模式");
                        GameModeManager.Instance.SetFreeMode();
                        SetupFreeMode();
                    }
                }
                else
                {
                    Debug.LogWarning("SampleSceneManager: 挑战模式但ChallengeDataManager不存在，回退到自由模式");
                    GameModeManager.Instance.SetFreeMode();
                    SetupFreeMode();
                }
            }
            else if (GameModeManager.Instance.IsFreeMode())
            {
                Debug.Log("SampleSceneManager: 检测到自由模式");
                SetupFreeMode();
            }
            else
            {
                Debug.Log("SampleSceneManager: 未知模式，默认进入自由模式");
                GameModeManager.Instance.SetFreeMode();
                SetupFreeMode();
            }
        }
        else
        {
            Debug.Log("SampleSceneManager: GameModeManager不存在，进入自由模式");
            SetupFreeMode();
        }
    }
    
    void Update()
    {
        // 检测ESC键按下，直接退出游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }
    
    private void RestartToTitle()
    {
        Debug.Log("SampleSceneManager: 检测到ESC键，重启游戏并回到标题画面");
        
        // 强制停止任何正在进行的挑战
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.ForceStopChallenge();
            Debug.Log("SampleSceneManager: 已强制停止挑战");
        }
        
        // 停止所有音频
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.Stop();
        }
        
        // 重置时间缩放
        Time.timeScale = 1f;
        
        // 清除选中的乐谱
        if (ChallengeDataManager.Instance != null)
        {
            ChallengeDataManager.Instance.SetSelectedMusicSheet(null);
        }
        
        // 重置游戏模式为自由模式
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetFreeMode();
            Debug.Log("SampleSceneManager: 已重置为自由模式");
        }
        
        // 加载标题场景
        SceneManager.LoadScene("TitleScene");
    }
    
    private void QuitGame()
    {
        Debug.Log("SampleSceneManager: 检测到ESC键，退出游戏");
        
        // 停止所有音频
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.Stop();
        }
        
        // 重置时间缩放
        Time.timeScale = 1f;
        
        // 退出应用程序
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
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
        Debug.Log("SampleSceneManager: 设置自由模式");
        
        // 停止任何正在进行的挑战
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.ForceStopChallenge();
            Debug.Log("SampleSceneManager: 已强制停止挑战模式");
        }
        
        // 注意：不要在这里重新设置游戏模式，因为模式应该已经在TitleManager中设置好了
        
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
        
        Debug.Log("SampleSceneManager: 自由模式设置完成");
    }
    
private void SetupTutorialMode()
    {
        Debug.Log("SampleSceneManager: 设置教程模式");
        
        // 停止任何正在进行的挑战
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.ForceStopChallenge();
            Debug.Log("SampleSceneManager: 已强制停止挑战模式");
        }
        
        // 注意：不要在这里重新设置游戏模式，因为模式应该已经在TitleManager中设置好了
        
        // 在教程模式下显示基本UI
        if (challengeUI != null)
        {
            challengeUI.SetActive(true);
        }
        
        if (progressText != null)
        {
            progressText.text = "新手教程模式";
        }
        
        if (upcomingNotesText != null)
        {
            upcomingNotesText.text = "欢迎来到新手教程！";
        }
        
        if (scoreText != null)
        {
            scoreText.text = "教程进度: 0%";
        }
        
        if (progressSlider != null)
        {
            progressSlider.value = 0;
        }
        
        // 启动教程管理器
        TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
        if (tutorialManager != null)
        {
            tutorialManager.StartTutorial();
            Debug.Log("SampleSceneManager: 已启动教程管理器");
        }
        else
        {
            Debug.LogError("SampleSceneManager: 未找到TutorialManager组件");
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
        
        // 强制停止任何正在进行的挑战
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.ForceStopChallenge();
            Debug.Log("SampleSceneManager: 已强制停止挑战");
        }
        
        // 清除选中的乐谱
        if (ChallengeDataManager.Instance != null)
        {
            ChallengeDataManager.Instance.SetSelectedMusicSheet(null);
        }
        
        // 重置游戏模式为自由模式（等待重新选择挑战）
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetFreeMode();
            Debug.Log("SampleSceneManager: 已重置为自由模式，等待重新选择挑战");
        }
        
        SceneManager.LoadScene("ChallengeScene");
    }
    
    // 返回主菜单
    public void BackToMainMenu()
    {
        Debug.Log("SampleSceneManager: 返回主菜单");
        
        // 强制停止任何正在进行的挑战
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.ForceStopChallenge();
            Debug.Log("SampleSceneManager: 已强制停止挑战");
        }
        
        // 清除选中的乐谱
        if (ChallengeDataManager.Instance != null)
        {
            ChallengeDataManager.Instance.SetSelectedMusicSheet(null);
        }
        
        // 重置游戏模式为自由模式
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetFreeMode();
            Debug.Log("SampleSceneManager: 已重置为自由模式");
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