using UnityEngine;

public class SimpleSeparatorTest : MonoBehaviour
{
void Start()
    {
        Debug.Log("SimpleSeparatorTest: Start method called!");
        Debug.Log("SimpleSeparatorTest: About to run separator test...");
        
        // 立即运行测试
        RunSeparatorTest();
        
        Debug.Log("SimpleSeparatorTest: Test completed!");
    }
    
void RunSeparatorTest()
    {
        Debug.Log("SimpleSeparatorTest: 开始测试分隔符修改");
        
        // 模拟GetUpcomingNotesByTime方法的逻辑
        string testResult = SimulateGetUpcomingNotesByTime();
        
        Debug.Log($"SimpleSeparatorTest: 测试结果 - {testResult}");
        Debug.Log("SimpleSeparatorTest: 如果看到音符之间有换行而不是 | 分隔，说明修改成功");
        
        // 将结果显示到UI上
        var textComponent = GameObject.Find("SeparatorTestText")?.GetComponent<UnityEngine.UI.Text>();
        if (textComponent != null)
        {
            textComponent.text = testResult;
            Debug.Log("SimpleSeparatorTest: 已将测试结果显示到UI上");
        }
        else
        {
            Debug.LogError("SimpleSeparatorTest: 找不到SeparatorTestText UI元素");
        }
    }
    
    string SimulateGetUpcomingNotesByTime()
    {
        // 模拟一些音符数据
        string[] noteNames = {"C", "D", "E", "F", "G"};
        string[] solfegeNames = {"do", "re", "mi", "fa", "sol"};
        
        string result = "";
        for (int i = 0; i < noteNames.Length; i++)
        {
            result += $"{noteNames[i]}({solfegeNames[i]})";
            
            // 这里使用修改后的分隔符：换行符而不是" | "
            if (i < noteNames.Length - 1) 
                result += "\n";  // 修改后的分隔符
        }
        
        return result;
    }
    
    void Update()
    {
        // 按T键重新运行测试
        if (Input.GetKeyDown(KeyCode.T))
        {
            RunSeparatorTest();
        }
    }
}