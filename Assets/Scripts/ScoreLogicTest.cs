using UnityEngine;

public class ScoreLogicTest : MonoBehaviour
{
    [Header("积分系统验证")]
    public bool runValidation = true;
    
    void Start()
    {
        if (runValidation)
        {
            ValidateScoreSystem();
        }
    }
    
    void ValidateScoreSystem()
    {
        Debug.Log("=== 积分系统逻辑验证开始 ===");
        
        // 测试用例1：完美演奏
        TestScoreCalculation(10f, 10f, 100f, "完美演奏");
        
        // 测试用例2：一半正确
        TestScoreCalculation(10f, 5f, 50f, "一半正确");
        
        // 测试用例3：无演奏
        TestScoreCalculation(10f, 0f, 0f, "无演奏");
        
        // 测试用例4：超时演奏（不应该超过100%）
        TestScoreCalculation(10f, 12f, 100f, "超时演奏");
        
        // 测试用例5：零时长乐谱
        TestScoreCalculation(0f, 5f, 0f, "零时长乐谱");
        
        Debug.Log("=== 积分系统逻辑验证完成 ===");
    }
    
    void TestScoreCalculation(float totalDuration, float correctTime, float expectedScore, string testName)
    {
        float actualScore = CalculateScore(totalDuration, correctTime);
        bool passed = Mathf.Abs(actualScore - expectedScore) < 0.1f;
        
        string result = passed ? "✓ 通过" : "✗ 失败";
        Debug.Log($"{result} {testName}: 总时长={totalDuration}s, 正确时长={correctTime}s, 期望得分={expectedScore}%, 实际得分={actualScore:F1}%");
        
        if (!passed)
        {
            Debug.LogWarning($"测试失败详情: 期望 {expectedScore}%, 实际 {actualScore:F1}%");
        }
    }
    
    // 模拟新积分系统的计算逻辑
    float CalculateScore(float totalDuration, float correctTime)
    {
        if (totalDuration <= 0f)
            return 0f;
            
        float scorePercentage = (correctTime / totalDuration) * 100f;
        return Mathf.Clamp(scorePercentage, 0f, 100f);
    }
    
    [ContextMenu("运行验证")]
    public void RunValidation()
    {
        ValidateScoreSystem();
    }
}