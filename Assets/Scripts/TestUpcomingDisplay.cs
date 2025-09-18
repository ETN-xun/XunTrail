using UnityEngine;
using UnityEngine.SceneManagement;

public class TestUpcomingDisplay : MonoBehaviour
{
    void Start()
    {
        // 延迟2秒后自动触发挑战模式测试
        Invoke("StartChallengeTest", 2f);
    }
    
    void Update()
    {
        // 按T键手动触发测试
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartChallengeTest();
        }
        
        // 按ESC键退出挑战
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var challengeManager = FindObjectOfType<ChallengeManager>();
            if (challengeManager != null && challengeManager.IsInChallenge())
            {
                challengeManager.ExitChallenge();
            }
        }
    }
    
    void StartChallengeTest()
    {
        Debug.Log("TestUpcomingDisplay: 开始测试UpcomingNotesText显示");
        
        var challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager != null)
        {
            Debug.Log("TestUpcomingDisplay: 找到ChallengeManager，启动挑战模式");
            challengeManager.StartChallenge();
            
            // 激活相关UI元素
            ActivateUIElements();
        }
        else
        {
            Debug.LogError("TestUpcomingDisplay: 未找到ChallengeManager");
        }
    }
    
    void ActivateUIElements()
    {
        // 确保UpcomingNotesText等UI元素被激活
        GameObject upcomingNotesText = GameObject.Find("UpcomingNotesText");
        if (upcomingNotesText != null)
        {
            upcomingNotesText.SetActive(true);
            Debug.Log("TestUpcomingDisplay: UpcomingNotesText已激活");
        }
        else
        {
            Debug.LogWarning("TestUpcomingDisplay: 未找到UpcomingNotesText对象");
        }
        
        // 激活其他相关UI
        string[] uiElements = {"ProgressText", "ScoreText", "ProgressSlider", "NoteDisplayText", "OctaveDisplayText", "KeyDisplayText"};
        foreach (string elementName in uiElements)
        {
            GameObject element = GameObject.Find(elementName);
            if (element != null)
            {
                element.SetActive(true);
                Debug.Log($"TestUpcomingDisplay: {elementName}已激活");
            }
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("UpcomingNotesText 显示测试", GUI.skin.box);
        
        if (GUILayout.Button("开始测试 (T)"))
        {
            StartChallengeTest();
        }
        
        var challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager != null)
        {
            bool isInChallenge = challengeManager.IsInChallenge();
            GUILayout.Label($"挑战状态: {(isInChallenge ? "进行中" : "未开始")}");
            
            if (isInChallenge && GUILayout.Button("退出挑战 (ESC)"))
            {
                challengeManager.ExitChallenge();
            }
        }
        
        GUILayout.EndArea();
    }
}