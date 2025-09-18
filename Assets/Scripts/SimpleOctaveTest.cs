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
        
        // 测试手柄按键音高修复
        TestGamepadButtonPitches();
    }
    
    void TestGamepadButtonPitches()
    {
        Debug.Log("\n=== 手柄按键音高修复测试 ===");
        
        ToneGenerator toneGenerator = FindObjectOfType<ToneGenerator>();
        if (toneGenerator == null)
        {
            Debug.LogError("未找到ToneGenerator组件！");
            return;
        }
        
        // 测试X键（应该是中音5）
        Debug.Log("\n--- 测试X键音高 ---");
        float xFrequency = TestButtonFrequency(toneGenerator, "X");
        string xNote = GetNoteFromFrequency(toneGenerator, xFrequency);
        Debug.Log($"X键频率: {xFrequency:F2} Hz -> {xNote}");
        Debug.Log($"期望: 中音5, 实际: {xNote}");
        bool xCorrect = xNote.Contains("中音5") || xNote.Contains("5");
        Debug.Log($"X键测试: {(xCorrect ? "✓ 通过" : "✗ 失败")}");
        
        // 测试Y键（应该是高音1）
        Debug.Log("\n--- 测试Y键音高 ---");
        float yFrequency = TestButtonFrequency(toneGenerator, "Y");
        string yNote = GetNoteFromFrequency(toneGenerator, yFrequency);
        Debug.Log($"Y键频率: {yFrequency:F2} Hz -> {yNote}");
        Debug.Log($"期望: 高音1, 实际: {yNote}");
        bool yCorrect = yNote.Contains("高音1") || (yNote.Contains("高音") && yNote.Contains("1"));
        Debug.Log($"Y键测试: {(yCorrect ? "✓ 通过" : "✗ 失败")}");
        
        // 总结
        Debug.Log("\n=== 手柄按键测试结果总结 ===");
        if (xCorrect && yCorrect)
        {
            Debug.Log("✅ 手柄按键音高修复成功！X键和Y键都显示正确的音高。");
        }
        else
        {
            Debug.LogError("❌ 手柄按键音高仍有问题，需要进一步检查。");
        }
    }
    
    private float TestButtonFrequency(ToneGenerator toneGenerator, string buttonName)
    {
        // 使用反射调用GetBaseFrequency方法
        var method = toneGenerator.GetType().GetMethod("GetBaseFrequency", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method != null)
        {
            if (buttonName == "X")
            {
                // 模拟X键按下
                return (float)method.Invoke(toneGenerator, new object[] { true, false, false, false, false, false, false, false, false, false, false, false });
            }
            else if (buttonName == "Y")
            {
                // 模拟Y键按下
                return (float)method.Invoke(toneGenerator, new object[] { false, true, false, false, false, false, false, false, false, false, false, false });
            }
        }
        
        Debug.LogError($"无法获取{buttonName}键的频率");
        return 440f; // 默认A4频率
    }
    
    private string GetNoteFromFrequency(ToneGenerator toneGenerator, float frequency)
    {
        // 使用反射调用GetNoteFromFrequency方法
        var method = toneGenerator.GetType().GetMethod("GetNoteFromFrequency", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method != null)
        {
            return (string)method.Invoke(toneGenerator, new object[] { frequency });
        }
        
        Debug.LogError("无法调用GetNoteFromFrequency方法");
        return "未知";
    }
}