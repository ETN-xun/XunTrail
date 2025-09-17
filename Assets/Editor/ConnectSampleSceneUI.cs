using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ConnectSampleSceneUI : EditorWindow
{
    [MenuItem("Tools/Connect SampleScene UI")]
    public static void ConnectUI()
    {
        Debug.Log("开始连接SampleScene UI...");
        
        // 查找ChallengeManager
        ChallengeManager challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager == null)
        {
            Debug.LogError("ChallengeManager not found in scene!");
            return;
        }
        Debug.Log("找到ChallengeManager");

        // 查找Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in scene!");
            return;
        }
        Debug.Log("找到Canvas");

        // 查找ChallengeUI（包括非激活的）
        Transform challengeUITransform = null;
        foreach (Transform child in canvas.transform)
        {
            if (child.name == "ChallengeUI")
            {
                challengeUITransform = child;
                break;
            }
        }
        
        if (challengeUITransform != null)
        {
            GameObject challengeUI = challengeUITransform.gameObject;
            Debug.Log("找到ChallengeUI");
            
            // 启用ChallengeUI
            challengeUI.SetActive(true);
            Debug.Log("ChallengeUI已启用");
            
            // 连接UI元素
            challengeManager.challengeUI = challengeUI;
            Debug.Log("ChallengeUI已连接到ChallengeManager");
            
            // 查找并连接ProgressText
            foreach (Transform child in challengeUI.transform)
            {
                if (child.name == "ProgressText")
                {
                    Text progressText = child.GetComponent<Text>();
                    if (progressText != null)
                    {
                        challengeManager.progressText = progressText;
                        Debug.Log("ProgressText已连接");
                    }
                }
                else if (child.name == "UpcomingNotesText")
                {
                    Text upcomingNotesText = child.GetComponent<Text>();
                    if (upcomingNotesText != null)
                    {
                        challengeManager.upcomingNotesText = upcomingNotesText;
                        Debug.Log("UpcomingNotesText已连接");
                    }
                }
                else if (child.name == "ScoreText")
                {
                    Text scoreText = child.GetComponent<Text>();
                    if (scoreText != null)
                    {
                        challengeManager.scoreText = scoreText;
                        Debug.Log("ScoreText已连接");
                    }
                }
                else if (child.name == "CountdownText")
                {
                    Text countdownText = child.GetComponent<Text>();
                    if (countdownText != null)
                    {
                        challengeManager.countdownText = countdownText;
                        Debug.Log("CountdownText已连接");
                    }
                }
            }
            
            // 标记场景为已修改
            EditorUtility.SetDirty(challengeManager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
            Debug.Log("所有UI元素连接完成！");
        }
        else
        {
            Debug.LogError("ChallengeUI not found under Canvas!");
        }
    }
}