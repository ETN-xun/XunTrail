using UnityEngine;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Button challengeButton;
    public Button quitButton;
    
    void Start()
    {
        // 配置按钮点击事件
        if (challengeButton != null)
        {
            challengeButton.onClick.AddListener(OnChallengeButtonClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
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