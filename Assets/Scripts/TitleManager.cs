using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // 按钮将通过代码动态查找
    
void Start()
    {
        // 动态查找按钮并绑定事件
        GameObject freeModeButtonObj = GameObject.Find("FreeModeButton");
        GameObject challengeButtonObj = GameObject.Find("ChallengeButton");
        GameObject quitButtonObj = GameObject.Find("QuitButton");
        GameObject keyChangeButtonObj = GameObject.Find("KeyChangeButton");
        GameObject guideButtonObj = GameObject.Find("GuideButton");
        
        if (freeModeButtonObj != null)
        {
            Button freeModeButton = freeModeButtonObj.GetComponent<Button>();
            if (freeModeButton != null)
                freeModeButton.onClick.AddListener(OnFreeModeClicked);
        }
        
        if (challengeButtonObj != null)
        {
            Button challengeButton = challengeButtonObj.GetComponent<Button>();
            if (challengeButton != null)
                challengeButton.onClick.AddListener(OnChallengeModeClicked);
        }
        
        if (quitButtonObj != null)
        {
            Button quitButton = quitButtonObj.GetComponent<Button>();
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        if (keyChangeButtonObj != null)
        {
            Button keyChangeButton = keyChangeButtonObj.GetComponent<Button>();
            if (keyChangeButton != null)
                keyChangeButton.onClick.AddListener(OnKeyChangeClicked);
        }
        
        if (guideButtonObj != null)
        {
            Button guideButton = guideButtonObj.GetComponent<Button>();
            if (guideButton != null)
                guideButton.onClick.AddListener(OnGuideButtonClicked);
        }
    }
    
    void Update()
    {
        // 在标题画面按ESC键退出游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnQuitClicked();
        }
    }

public void OnFreeModeClicked()
    {
        Debug.Log("点击了自由模式按钮");
        
        // 清除挑战模式状态
        if (ChallengeDataManager.Instance != null)
        {
            ChallengeDataManager.Instance.SetSelectedMusicSheet(null);
            Debug.Log("TitleManager: 已清除挑战模式的选中乐谱");
        }
        
        // 设置游戏模式为自由模式
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetFreeMode();
            Debug.Log("TitleManager: 已设置为自由模式");
        }
        
        // 加载自由模式场景（SampleScene）
        SceneManager.LoadScene("SampleScene");
    }

    public void OnKeyChangeClicked()
    {
        Debug.Log("点击了键位切换按钮");
        // 加载自由模式场景（SampleScene）
        SceneManager.LoadScene("KeyChangeEight");
    }

    public void OnGuideButtonClicked()
    {
        Debug.Log("点击了新手教程按钮");
        
        // 清除挑战模式状态
        if (ChallengeDataManager.Instance != null)
        {
            ChallengeDataManager.Instance.SetSelectedMusicSheet(null);
            Debug.Log("TitleManager: 已清除挑战模式的选中乐谱");
        }
        
        // 设置游戏模式为教程模式
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetTutorialMode();
            Debug.Log("TitleManager: 已设置为教程模式");
        }
        
        // 加载SampleScene进入教程模式
        SceneManager.LoadScene("SampleScene");
    }

    
void OnQuitClicked()
    {
        // 退出应用程序
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
public void OnChallengeModeClicked()
    {
        try
        {
            Debug.Log("挑战模式按钮被点击");
            
            // 确保清除之前的状态
            if (ChallengeDataManager.Instance != null)
            {
                ChallengeDataManager.Instance.SetSelectedMusicSheet(null);
                Debug.Log("TitleManager: 已清除之前的挑战模式状态");
            }
            
            // 设置游戏模式为挑战模式（暂时设置，具体乐谱在ChallengeScene中选择）
            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.SetFreeMode(); // 先设为自由模式，等选择乐谱后再设为挑战模式
                Debug.Log("TitleManager: 准备进入挑战模式选择界面");
            }
            
            SceneManager.LoadScene("ChallengeScene");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载挑战模式场景失败: {e.Message}");
        }
    }
}