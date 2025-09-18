using UnityEngine;

public class OctaveFixVerification : MonoBehaviour
{
    void Start()
    {
        VerifyOctaveFix();
    }
    
    void VerifyOctaveFix()
    {
        Debug.Log("=== 八度显示修复验证 ===");
        
        // 测试1=F调号（key=5）
        int fKey = 5;
        
        // 标准音符频率
        float c4 = 261.63f;
        float f4 = c4 * Mathf.Pow(2f, 5f/12f); // F4 ≈ 349.23 Hz
        float f5 = f4 * 2f; // F5 ≈ 698.46 Hz
        float f3 = f4 / 2f; // F3 ≈ 174.61 Hz
        
        // 测试关键频率
        string f3Result = ChallengeManager.FrequencyToSolfege(f3, fKey);
        string f4Result = ChallengeManager.FrequencyToSolfege(f4, fKey);
        string f5Result = ChallengeManager.FrequencyToSolfege(f5, fKey);
        
        Debug.Log($"F3 ({f3:F2} Hz) -> {f3Result} (期望: 低音1)");
        Debug.Log($"F4 ({f4:F2} Hz) -> {f4Result} (期望: 中音1)");
        Debug.Log($"F5 ({f5:F2} Hz) -> {f5Result} (期望: 高音1)");
        
        // 验证结果
        bool f3Correct = f3Result == "低音1";
        bool f4Correct = f4Result == "中音1";
        bool f5Correct = f5Result == "高音1";
        
        Debug.Log($"\n=== 验证结果 ===");
        Debug.Log($"F3显示正确: {f3Correct}");
        Debug.Log($"F4显示正确: {f4Correct}");
        Debug.Log($"F5显示正确: {f5Correct}");
        Debug.Log($"修复成功: {f3Correct && f4Correct && f5Correct}");
        
        if (f4Correct)
        {
            Debug.Log("✅ 问题已修复！F4在1=F调号下正确显示为'中音1'");
        }
        else
        {
            Debug.LogError("❌ 问题仍未解决，F4显示不正确");
        }
        
        // 测试其他调号
        Debug.Log("\n=== 测试其他调号 ===");
        TestOtherKeys();
    }
    
    void TestOtherKeys()
    {
        // 测试C调（key=0）
        float c4 = 261.63f;
        string c4InCKey = ChallengeManager.FrequencyToSolfege(c4, 0);
        Debug.Log($"C4在1=C调号下: {c4InCKey} (期望: 中音1)");
        
        // 测试G调（key=7）
        float g4 = c4 * Mathf.Pow(2f, 7f/12f);
        string g4InGKey = ChallengeManager.FrequencyToSolfege(g4, 7);
        Debug.Log($"G4在1=G调号下: {g4InGKey} (期望: 中音1)");
    }
}