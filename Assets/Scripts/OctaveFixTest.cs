using UnityEngine;

public class OctaveFixTest : MonoBehaviour
{
    void Start()
    {
        RunOctaveTests();
    }

    [ContextMenu("Run Octave Tests")]
    void RunOctaveTests()
    {
        Debug.Log("=== 八度显示修复测试 ===");
        
        // 测试C4在1=C调号下
        TestNote("C4", 261.63f, 0, "中音1");
        
        // 测试F4在1=F调号下
        TestNote("F4", 349.23f, 5, "中音1");
        
        // 测试边界情况
        Debug.Log("\n=== 边界测试 ===");
        
        // C3应该显示为低音
        TestNote("C3", 130.81f, 0, "低音1");
        
        // C5应该显示为高音
        TestNote("C5", 523.25f, 0, "高音1");
        
        // F3应该显示为低音
        TestNote("F3", 174.61f, 5, "低音1");
        
        // F5应该显示为高音
        TestNote("F5", 698.46f, 5, "高音1");
        
        // 测试其他调号
        Debug.Log("\n=== 其他调号测试 ===");
        
        // G4在1=G调号下
        TestNote("G4", 392.00f, 7, "中音1");
        
        // D4在1=D调号下
        TestNote("D4", 293.66f, 2, "中音1");
    }
    
    void TestNote(string noteName, float frequency, int key, string expected)
    {
        string result = ChallengeManager.FrequencyToSolfege(frequency, key);
        bool passed = result == expected;
        
        string keyName = GetKeyName(key);
        Debug.Log($"{noteName} 在 1={keyName} 调号下:");
        Debug.Log($"  频率: {frequency:F2} Hz");
        Debug.Log($"  显示结果: {result}");
        Debug.Log($"  期望结果: {expected}");
        Debug.Log($"  测试通过: {passed} {(passed ? "✅" : "❌")}");
        Debug.Log("");
    }
    
    string GetKeyName(int key)
    {
        return key switch
        {
            -4 => "A♭",
            -3 => "A",
            -2 => "B♭",
            -1 => "B",
            0 => "C",
            1 => "D♭",
            2 => "D",
            3 => "E♭",
            4 => "E",
            5 => "F",
            6 => "F♯",
            7 => "G",
            _ => "C"
        };
    }
}