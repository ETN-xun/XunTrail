using UnityEngine;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Button challengeButton;
    public Button quitButton;
    
    void Start()
    {
        // 彻底禁用TitleUIManager组件，避免与TitleManager冲突
        Debug.Log("TitleUIManager: 组件已禁用，所有按钮处理由TitleManager负责");
        
        // 禁用此组件
        this.enabled = false;
    }
    
    public void OnChallengeButtonClicked()
    {
        Debug.Log("进入挑战模式");
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadChallengeScene();
        }
    }
    
    public void OnQuitButtonClicked()
    {
        Debug.Log("退出游戏");
        if (SceneController.Instance != null)
        {
            SceneController.Instance.QuitGame();
        }
    }
}