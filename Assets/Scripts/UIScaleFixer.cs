using UnityEngine;
using UnityEngine.UI;

public class UIScaleFixer : MonoBehaviour
{
    void Start()
    {
        FixUIScales();
    }
    
    private void FixUIScales()
    {
        // 修复ChallengeUI的scale
        GameObject challengeUI = GameObject.Find("ChallengeUI");
        if (challengeUI == null)
        {
            // 查找非激活的ChallengeUI
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
            Debug.Log("UIScaleFixer: 找到ChallengeUI，修复scale");
            challengeUI.transform.localScale = Vector3.one;
            
            // 调整子元素位置到合适的屏幕位置
            AdjustChildPositions(challengeUI);
        }
        
        // 修复ProgressSlider的scale
        GameObject progressSlider = GameObject.Find("ProgressSlider");
        if (progressSlider != null)
        {
            Debug.Log("UIScaleFixer: 找到ProgressSlider，修复scale");
            progressSlider.transform.localScale = Vector3.one;
            
            // 调整ProgressSlider位置
            RectTransform sliderRect = progressSlider.GetComponent<RectTransform>();
            if (sliderRect != null)
            {
                sliderRect.anchoredPosition = new Vector2(0, 200); // 放在屏幕上方
            }
        }
    }
    
    private void AdjustChildPositions(GameObject challengeUI)
    {
        // 参考现有UI的位置：
        // 音名UI: anchoredPosition(0, -217) - 屏幕下方
        // 调号UI: anchoredPosition(0, 0) - 屏幕中心
        
        Transform[] children = challengeUI.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == challengeUI.transform) continue;
            
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                switch (child.name)
                {
                    case "ProgressText":
                        rectTransform.anchoredPosition = new Vector2(0, 150); // 屏幕上方
                        break;
                    case "UpcomingNotesText":
                        rectTransform.anchoredPosition = new Vector2(0, 100); // 屏幕上方偏下
                        break;
                    case "ScoreText":
                        rectTransform.anchoredPosition = new Vector2(300, 150); // 屏幕右上角
                        break;
                    case "CountdownText":
                        rectTransform.anchoredPosition = new Vector2(0, 50); // 屏幕中心偏上
                        break;
                    case "ExitChallengeButton":
                        rectTransform.anchoredPosition = new Vector2(300, -200); // 屏幕右下角
                        break;
                }
                
                Debug.Log($"UIScaleFixer: 调整{child.name}位置到{rectTransform.anchoredPosition}");
            }
        }
    }
}