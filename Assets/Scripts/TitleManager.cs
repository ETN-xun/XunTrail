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
        Debug.Log("TitleManager: 开始初始化按钮绑定");
        
        // 确保GameModeManager存在
        if (GameModeManager.Instance == null)
        {
            Debug.LogWarning("TitleManager: GameModeManager.Instance为null，正在创建新实例");
            GameObject gameModeManagerObj = new GameObject("GameModeManager");
            gameModeManagerObj.AddComponent<GameModeManager>();
        }
        
        // 初始化键位设置系统，从Appdata加载键位信息
        InitializeKeySettings();
        
        // 动态查找按钮并绑定事件
        GameObject freeModeButtonObj = GameObject.Find("FreeModeButton");
        GameObject challengeButtonObj = GameObject.Find("ChallengeButton");
        GameObject tutorialButtonObj = GameObject.Find("TutorialButton"); // 新手教程按钮
        GameObject quitButtonObj = GameObject.Find("QuitButton");
        GameObject keyChangeButtonObj = GameObject.Find("KeyChangeButton");
        
        Debug.Log($"TitleManager: 按钮查找结果 - FreeModeButton: {freeModeButtonObj != null}, ChallengeButton: {challengeButtonObj != null}, TutorialButton: {tutorialButtonObj != null}, QuitButton: {quitButtonObj != null}, KeyChangeButton: {keyChangeButtonObj != null}");
        
        if (freeModeButtonObj != null)
        {
            Button freeModeButton = freeModeButtonObj.GetComponent<Button>();
            if (freeModeButton != null)
            {
                freeModeButton.onClick.AddListener(OnFreeModeClicked);
                Debug.Log("TitleManager: FreeModeButton 事件绑定成功");
            }
            else
            {
                Debug.LogError("TitleManager: FreeModeButton 没有Button组件");
            }
        }
        else
        {
            Debug.LogError("TitleManager: 未找到 FreeModeButton");
        }
        
        if (challengeButtonObj != null)
        {
            Button challengeButton = challengeButtonObj.GetComponent<Button>();
            if (challengeButton != null)
            {
                challengeButton.onClick.AddListener(OnChallengeModeClicked);
                Debug.Log("TitleManager: ChallengeButton 事件绑定成功");
            }
            else
            {
                Debug.LogError("TitleManager: ChallengeButton 没有Button组件");
            }
        }
        else
        {
            Debug.LogError("TitleManager: 未找到 ChallengeButton");
        }
        
        if (tutorialButtonObj != null)
        {
            Button tutorialButton = tutorialButtonObj.GetComponent<Button>();
            if (tutorialButton != null)
            {
                tutorialButton.onClick.AddListener(OnTutorialModeClicked);
                Debug.Log("TitleManager: TutorialButton 事件绑定成功");
            }
            else
            {
                Debug.LogError("TitleManager: TutorialButton 没有Button组件");
            }
        }
        else
        {
            Debug.LogWarning("TitleManager: 未找到 TutorialButton，可能该按钮在当前场景中不存在");
        }
        

        if (quitButtonObj != null)
        {
            Button quitButton = quitButtonObj.GetComponent<Button>();
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
                Debug.Log("TitleManager: QuitButton 事件绑定成功");
            }
            else
            {
                Debug.LogError("TitleManager: QuitButton 没有Button组件");
            }
        }
        else
        {
            Debug.LogError("TitleManager: 未找到 QuitButton");
        }
        
        if (keyChangeButtonObj != null)
        {
            Button keyChangeButton = keyChangeButtonObj.GetComponent<Button>();
            if (keyChangeButton != null)
            {
                keyChangeButton.onClick.AddListener(OnKeyChangeClicked);
                Debug.Log("TitleManager: KeyChangeButton 事件绑定成功");
            }
            else
            {
                Debug.LogError("TitleManager: KeyChangeButton 没有Button组件");
            }
        }
        else
        {
            Debug.LogError("TitleManager: 未找到 KeyChangeButton");
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
        Debug.Log("点击了指南按钮");
        // 这里可以添加指南功能，比如显示游戏说明
        // 暂时什么都不做
    }

    public void OnTutorialModeClicked()
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
            
            // 不在这里设置游戏模式，等在ChallengeScene选择乐谱后再设置为挑战模式
            Debug.Log("TitleManager: 准备进入挑战模式选择界面");
            
            SceneManager.LoadScene("ChallengeScene");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载挑战模式场景失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 初始化键位设置系统
    /// 在进入游戏后，读取Appdata中的键位信息，如果没有则新建一个默认键位的文件
    /// </summary>
    private void InitializeKeySettings()
    {
        Debug.Log("TitleManager: 开始初始化键位设置系统");
        
        try
        {
            // 确保KeySettingsManager实例存在
            if (KeySettingsManager.Instance == null)
            {
                Debug.LogWarning("TitleManager: KeySettingsManager.Instance为null，正在创建新实例");
                GameObject keySettingsManagerObj = new GameObject("KeySettingsManager");
                keySettingsManagerObj.AddComponent<KeySettingsManager>();
            }
            
            // 加载键位设置，如果Appdata中没有文件，会自动创建默认设置
            KeySettingsManager.Instance.LoadKeySettings();
            
            Debug.Log("TitleManager: 键位设置系统初始化完成");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TitleManager: 初始化键位设置系统失败: {e.Message}");
        }
    }
}