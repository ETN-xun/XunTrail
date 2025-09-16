using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class FixChallengeUI
{
    [MenuItem("Tools/Fix Challenge UI Position")]
    public static void FixUI()
    {
        // 查找ChallengeUI
        GameObject challengeUI = GameObject.Find("ChallengeUI");
        if (challengeUI == null)
        {
            // 如果没找到，尝试在所有对象中查找（包括非激活的）
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "ChallengeUI" && obj.scene.name != null)
                {
                    challengeUI = obj;
                    break;
                }
            }
        }
        
        if (challengeUI != null)
        {
            // 查找Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // 将ChallengeUI设为Canvas的子对象
                challengeUI.transform.SetParent(canvas.transform, false);
                
                // 重置缩放
                challengeUI.transform.localScale = Vector3.one;
                
                // 激活ChallengeUI
                challengeUI.SetActive(true);
                
                // 调整UI元素位置（使用屏幕坐标）
                RectTransform challengeRect = challengeUI.GetComponent<RectTransform>();
                if (challengeRect == null)
                {
                    challengeRect = challengeUI.AddComponent<RectTransform>();
                }
                
                // 设置为全屏
                challengeRect.anchorMin = Vector2.zero;
                challengeRect.anchorMax = Vector2.one;
                challengeRect.offsetMin = Vector2.zero;
                challengeRect.offsetMax = Vector2.zero;
                
                // 调整子元素位置
                Transform progressText = challengeUI.transform.Find("ProgressText");
                if (progressText != null)
                {
                    RectTransform progressRect = progressText.GetComponent<RectTransform>();
                    if (progressRect == null) progressRect = progressText.gameObject.AddComponent<RectTransform>();
                    
                    // 左上角位置
                    progressRect.anchorMin = new Vector2(0, 1);
                    progressRect.anchorMax = new Vector2(0, 1);
                    progressRect.anchoredPosition = new Vector2(20, -20);
                }
                
                Transform upcomingText = challengeUI.transform.Find("UpcomingNotesText");
                if (upcomingText != null)
                {
                    RectTransform upcomingRect = upcomingText.GetComponent<RectTransform>();
                    if (upcomingRect == null) upcomingRect = upcomingText.gameObject.AddComponent<RectTransform>();
                    
                    // 左上角下方
                    upcomingRect.anchorMin = new Vector2(0, 1);
                    upcomingRect.anchorMax = new Vector2(0, 1);
                    upcomingRect.anchoredPosition = new Vector2(20, -60);
                }
                
                Transform scoreText = challengeUI.transform.Find("ScoreText");
                if (scoreText != null)
                {
                    RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
                    if (scoreRect == null) scoreRect = scoreText.gameObject.AddComponent<RectTransform>();
                    
                    // 右上角
                    scoreRect.anchorMin = new Vector2(1, 1);
                    scoreRect.anchorMax = new Vector2(1, 1);
                    scoreRect.anchoredPosition = new Vector2(-20, -20);
                }
                
                Transform countdownText = challengeUI.transform.Find("CountdownText");
                if (countdownText != null)
                {
                    RectTransform countdownRect = countdownText.GetComponent<RectTransform>();
                    if (countdownRect == null) countdownRect = countdownText.gameObject.AddComponent<RectTransform>();
                    
                    // 中央
                    countdownRect.anchorMin = new Vector2(0.5f, 0.5f);
                    countdownRect.anchorMax = new Vector2(0.5f, 0.5f);
                    countdownRect.anchoredPosition = Vector2.zero;
                }
                
                Debug.Log("ChallengeUI位置已修复");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            else
            {
                Debug.LogError("未找到Canvas");
            }
        }
        else
        {
            Debug.LogError("未找到ChallengeUI");
        }
    }
}