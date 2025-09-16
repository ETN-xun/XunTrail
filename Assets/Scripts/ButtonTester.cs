using UnityEngine;
using UnityEngine.UI;

public class ButtonTester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== ButtonTester Start() ===");
        
        // 查找TitleUIManager
        TitleUIManager titleUIManager = FindObjectOfType<TitleUIManager>();
        if (titleUIManager != null)
        {
            Debug.Log("找到TitleUIManager");
            
            // 查找按钮
            Button challengeButton = GameObject.Find("ChallengeButton")?.GetComponent<Button>();
            Button quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();
            
            if (challengeButton != null)
            {
                Debug.Log("找到ChallengeButton，添加测试点击事件");
                challengeButton.onClick.AddListener(() => {
                    Debug.Log("ChallengeButton被点击！");
                    titleUIManager.OnChallengeButtonClicked();
                });
            }
            else
            {
                Debug.LogError("未找到ChallengeButton");
            }
            
            if (quitButton != null)
            {
                Debug.Log("找到QuitButton");
            }
            else
            {
                Debug.LogError("未找到QuitButton");
            }
        }
        else
        {
            Debug.LogError("未找到TitleUIManager");
        }
    }
    
    void Update()
    {
        // 按空格键测试挑战按钮
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("空格键被按下，测试挑战按钮");
            TitleUIManager titleUIManager = FindObjectOfType<TitleUIManager>();
            if (titleUIManager != null)
            {
                titleUIManager.OnChallengeButtonClicked();
            }
        }
    }
}