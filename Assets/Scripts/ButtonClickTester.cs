using UnityEngine;
using UnityEngine.UI;

public class ButtonClickTester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("ButtonClickTester: 开始测试按钮点击功能");
        
        // 等待一帧后执行测试
        Invoke("TestButtonClick", 1f);
    }
    
    void TestButtonClick()
    {
        Debug.Log("ButtonClickTester: 查找TitleUIManager");
        
        // 查找TitleUIManager
        TitleUIManager titleUIManager = FindObjectOfType<TitleUIManager>();
        if (titleUIManager != null)
        {
            Debug.Log("ButtonClickTester: 找到TitleUIManager，调用OnChallengeButtonClicked");
            
            // 直接调用OnChallengeButtonClicked方法
            titleUIManager.SendMessage("OnChallengeButtonClicked", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogError("ButtonClickTester: 未找到TitleUIManager");
        }
    }
}