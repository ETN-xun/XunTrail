using UnityEngine;

public class SimpleOctaveTest : MonoBehaviour
{
    [Header("测试设置")]
    public bool runTestOnStart = true;
    
    void Start()
    {
        if (runTestOnStart)
        {
            RunOctaveTest();
        }
    }
    
    [ContextMenu("运行八度测试")]
    public void RunOctaveTest()
    {
        Debug.Log("=== 简单八度测试 ===");
        
        // 测试F4在1=F调号下的显示
        float f4Frequency = 349.23f; // 标准F4频率
        int fKey = 5; // 1=F调号
        
        string result = ChallengeManager.FrequencyToSolfege(f4Frequency, fKey);
        
        Debug.Log($"F4频率: {f4Frequency} Hz");
        Debug.Log($"调号: 1=F (key={fKey})");
        Debug.Log($"显示结果: {result}");
        Debug.Log($"期望结果: 中音1");
        Debug.Log($"测试通过: {result == "中音1"}");
        
        if (result == "中音1")
        {
            Debug.Log("✅ 八度显示修复成功！");
        }
        else
        {
            Debug.LogError("❌ 八度显示仍有问题");
        }
        
        // 额外测试
        TestAdditionalCases();
    }
    
    void TestAdditionalCases()
    {
        Debug.Log("\n=== 额外测试用例 ===");
        
        // 测试F5
        float f5 = 698.46f;
        string f5Result = ChallengeManager.FrequencyToSolfege(f5, 5);
        Debug.Log($"F5 -> {f5Result} (期望: 高音1)");
        
        // 测试F3
        float f3 = 174.61f;
        string f3Result = ChallengeManager.FrequencyToSolfege(f3, 5);
        Debug.Log($"F3 -> {f3Result} (期望: 低音1)");
        
        // 测试C4在C调
        float c4 = 261.63f;
        string c4Result = ChallengeManager.FrequencyToSolfege(c4, 0);
        Debug.Log($"C4在C调 -> {c4Result} (期望: 中音1)");
    }
}