using UnityEngine;
using UnityEngine.UI;

public class NoteDisplayTest : MonoBehaviour
{
    [Header("测试音符显示修改")]
    public Text testResultText;
    
    private ChallengeManager challengeManager;
    
    void Start()
    {
        challengeManager = FindObjectOfType<ChallengeManager>();
        
        if (testResultText != null)
        {
            testResultText.text = "按 N 键测试音符显示修改";
        }
        
        Debug.Log("音符显示测试脚本已启动");
        Debug.Log("修改内容：音符序列现在显示【当前】音符 + 接下来的两个音符");
        Debug.Log("之前：只显示接下来的三个音符");
    }
    
    void Update()
    {
        // N键测试音符显示
        if (Input.GetKeyDown(KeyCode.N))
        {
            TestNoteDisplay();
        }
        
        // 显示当前挑战状态
        if (challengeManager != null && challengeManager.IsInChallenge())
        {
            if (testResultText != null)
            {
                testResultText.text = "挑战进行中 - 观察音符序列显示\n现在应该显示：【当前】音符 + 接下来2个音符";
            }
        }
    }
    
    void TestNoteDisplay()
    {
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager！");
            if (testResultText != null)
            {
                testResultText.text = "错误：未找到ChallengeManager";
            }
            return;
        }
        
        Debug.Log("=== 音符显示测试 ===");
        Debug.Log("开始挑战以测试音符显示修改...");
        
        if (testResultText != null)
        {
            testResultText.text = "测试开始！\n启动挑战模式...\n观察音符序列是否显示【当前】音符";
        }
        
        // 启动挑战
        challengeManager.StartChallenge();
        
        Debug.Log("挑战已启动，请观察UI中的音符序列显示");
        Debug.Log("预期效果：应该看到【当前】标记的音符 + 接下来的两个音符");
    }
}