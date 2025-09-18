using UnityEngine;

public class QuickScoreTest : MonoBehaviour
{
    void Start()
    {
        TestNewScoreCalculation();
    }
    
    void TestNewScoreCalculation()
    {
        Debug.Log("=== 新积分系统测试 ===");
        
        // 测试用例1：完美演奏
        float perfectScore = CalculateTestScore(10f, 10f);
        Debug.Log($"完美演奏 (10/10秒): {perfectScore:F2}分");
        
        // 测试用例2：一半正确
        float halfScore = CalculateTestScore(5f, 10f);
        Debug.Log($"一半正确 (5/10秒): {halfScore:F2}分");
        
        // 测试用例3：四分之一正确
        float quarterScore = CalculateTestScore(2.5f, 10f);
        Debug.Log($"四分之一正确 (2.5/10秒): {quarterScore:F2}分");
        
        // 测试用例4：零分
        float zeroScore = CalculateTestScore(0f, 10f);
        Debug.Log($"零分 (0/10秒): {zeroScore:F2}分");
        
        Debug.Log("=== 测试完成 ===");
    }
    
    // 模拟新的积分计算逻辑
    private float CalculateTestScore(float correctDuration, float totalDuration)
    {
        if (totalDuration <= 0f) return 0f;
        
        float ratio = correctDuration / totalDuration;
        return Mathf.Clamp01(ratio) * 100f;
    }
}