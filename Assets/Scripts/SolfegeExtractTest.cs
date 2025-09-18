using UnityEngine;

public class SolfegeExtractTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 测试ExtractSolfegeNumber方法 ===");
        
        string[] testInputs = { "低音1", "中音2", "高音3", "中音4♯", "低音7" };
        string[] expectedOutputs = { "1", "2", "3", "4♯", "7" };
        
        for (int i = 0; i < testInputs.Length; i++)
        {
            string result = ChallengeManager.ExtractSolfegeNumber(testInputs[i]);
            string expected = expectedOutputs[i];
            string status = result == expected ? "✓" : "✗";
            
            Debug.Log($"{status} 输入: \"{testInputs[i]}\" -> 结果: \"{result}\" (期望: \"{expected}\")");
        }
        
        Debug.Log("=== ExtractSolfegeNumber测试完成 ===");
        
        // 测试音高显示修复
        TestOctaveDisplay();
    }
    
    void TestOctaveDisplay()
    {
        Debug.Log("=== 开始测试音高显示修复 ===");
        
        // 测试1=F调号（key=5）下的音高显示
        int testKey = 5; // F调
        
        // 获取F调主音频率（F4）
        float f4Frequency = GetTonicFrequencyPublic(testKey);
        float f3Frequency = f4Frequency / 2f; // F3
        float f5Frequency = f4Frequency * 2f; // F5
        
        // 测试不同八度的显示
        string f3Result = ChallengeManager.FrequencyToSolfege(f3Frequency, testKey);
        string f4Result = ChallengeManager.FrequencyToSolfege(f4Frequency, testKey);
        string f5Result = ChallengeManager.FrequencyToSolfege(f5Frequency, testKey);
        
        Debug.Log($"F3频率 {f3Frequency:F2}Hz -> {f3Result} (期望: 低音1)");
        Debug.Log($"F4频率 {f4Frequency:F2}Hz -> {f4Result} (期望: 中音1)");
        Debug.Log($"F5频率 {f5Frequency:F2}Hz -> {f5Result} (期望: 高音1)");
        
        // 验证结果
        bool f3Correct = f3Result == "低音1";
        bool f4Correct = f4Result == "中音1";
        bool f5Correct = f5Result == "高音1";
        
        Debug.Log($"F3测试: {(f3Correct ? "通过" : "失败")}");
        Debug.Log($"F4测试: {(f4Correct ? "通过" : "失败")}");
        Debug.Log($"F5测试: {(f5Correct ? "通过" : "失败")}");
        
        if (f3Correct && f4Correct && f5Correct)
        {
            Debug.Log("✓ 音高显示修复测试全部通过！");
        }
        else
        {
            Debug.LogError("✗ 音高显示修复测试失败！");
        }
        
        Debug.Log("=== 音高显示修复测试完成 ===");
    }
    
    // 公开版本的GetTonicFrequency方法用于测试
    private float GetTonicFrequencyPublic(int keyValue)
    {
        int tonicSemitone = keyValue switch
        {
            -4 => 8,  // A♭
            -3 => 9,  // A
            -2 => 10, // B♭
            -1 => 11, // B
            0 => 0,   // C
            1 => 1,   // D♭
            2 => 2,   // D
            3 => 3,   // E♭
            4 => 4,   // E
            5 => 5,   // F
            6 => 6,   // F♯
            7 => 7,   // G
            _ => 0    // 默认C
        };
        
        float c4Frequency = 261.63f;
        return c4Frequency * Mathf.Pow(2f, tonicSemitone / 12f);
    }
}