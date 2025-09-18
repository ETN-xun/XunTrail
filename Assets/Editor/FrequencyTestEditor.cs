using UnityEngine;
using UnityEditor;

public class FrequencyTestEditor : EditorWindow
{
    [MenuItem("Tools/Test Frequency To Solfege")]
    public static void TestFrequencyToSolfege()
    {
        Debug.Log("=== 测试FrequencyToSolfege方法 ===");
        
        // 测试F调（1个升号）
        int keyValue = 5; // F调
        
        // 测试一些基本频率
        float[] testFrequencies = {
            349.23f,  // F4 (主音)
            392.00f,  // G4 
            440.00f,  // A4
            466.16f,  // B♭4
            523.25f   // C5
        };
        
        string[] expectedResults = {
            "1",      // F4 -> 1 (主音)
            "2",      // G4 -> 2
            "3",      // A4 -> 3
            "4",      // B♭4 -> 4
            "5"       // C5 -> 5
        };
        
        for (int i = 0; i < testFrequencies.Length; i++)
        {
            string result = ChallengeManager.FrequencyToSolfege(testFrequencies[i], keyValue);
            string expected = expectedResults[i];
            string status = result == expected ? "✓" : "✗";
            
            Debug.Log($"{status} 频率: {testFrequencies[i]:F2}Hz -> 结果: \"{result}\" (期望: \"{expected}\")");
        }
        
        Debug.Log("=== 测试完成 ===");
        
        // 测试ExtractSolfegeNumber方法
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
    }
}