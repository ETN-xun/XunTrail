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
        // 加载自由模式场景（SampleScene）
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
            SceneManager.LoadScene("ChallengeScene");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载挑战模式场景失败: {e.Message}");
        }
    }
}