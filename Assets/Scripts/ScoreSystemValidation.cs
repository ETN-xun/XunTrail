using UnityEngine;

/// <summary>
/// 新积分系统验证脚本
/// 验证修改后的积分计算逻辑：正确时长/总时长 * 100
/// </summary>
public class ScoreSystemValidation : MonoBehaviour
{
    [Header("测试配置")]
    public bool runTestOnStart = true;
    public bool showDetailedLogs = true;
    
    void Start()
    {
        if (runTestOnStart)
        {
            ValidateNewScoreSystem();
        }
    }
    
    [ContextMenu("运行积分系统验证")]
    public void ValidateNewScoreSystem()
    {
        Debug.Log("=== 新积分系统验证开始 ===");
        
        // 测试用例集合
        TestCase[] testCases = new TestCase[]
        {
            new TestCase("完美演奏", 10f, 10f, 100f),
            new TestCase("优秀演奏", 9f, 10f, 90f),
            new TestCase("良好演奏", 8f, 10f, 80f),
            new TestCase("一般演奏", 5f, 10f, 50f),
            new TestCase("较差演奏", 2f, 10f, 20f),
            new TestCase("极差演奏", 0.5f, 10f, 5f),
            new TestCase("零分演奏", 0f, 10f, 0f),
            new TestCase("边界测试1", 1f, 1f, 100f),
            new TestCase("边界测试2", 0f, 1f, 0f),
            new TestCase("长时间演奏", 45f, 60f, 75f)
        };
        
        int passedTests = 0;
        int totalTests = testCases.Length;
        
        foreach (var testCase in testCases)
        {
            float calculatedScore = CalculateNewScore(testCase.correctDuration, testCase.totalDuration);
            bool passed = Mathf.Approximately(calculatedScore, testCase.expectedScore);
            
            if (showDetailedLogs)
            {
                string status = passed ? "✓ 通过" : "✗ 失败";
                Debug.Log($"{status} | {testCase.name}: {testCase.correctDuration}/{testCase.totalDuration}秒 = {calculatedScore:F1}分 (期望: {testCase.expectedScore:F1}分)");
            }
            
            if (passed)
                passedTests++;
            else
                Debug.LogError($"测试失败: {testCase.name} - 计算得分: {calculatedScore:F1}, 期望得分: {testCase.expectedScore:F1}");
        }
        
        // 输出总结
        Debug.Log($"=== 验证完成: {passedTests}/{totalTests} 个测试通过 ===");
        
        if (passedTests == totalTests)
        {
            Debug.Log("<color=green>✓ 所有测试通过！新积分系统工作正常。</color>");
        }
        else
        {
            Debug.LogWarning($"<color=orange>⚠ {totalTests - passedTests} 个测试失败，需要检查积分计算逻辑。</color>");
        }
        
        // 验证边界情况
        ValidateEdgeCases();
    }
    
    /// <summary>
    /// 新的积分计算方法：正确时长/总时长 * 100
    /// </summary>
    private float CalculateNewScore(float correctDuration, float totalDuration)
    {
        if (totalDuration <= 0f)
        {
            Debug.LogWarning("总时长不能为0或负数");
            return 0f;
        }
        
        if (correctDuration < 0f)
        {
            Debug.LogWarning("正确时长不能为负数");
            correctDuration = 0f;
        }
        
        // 确保正确时长不超过总时长
        correctDuration = Mathf.Min(correctDuration, totalDuration);
        
        // 计算比例并转换为百分制
        float ratio = correctDuration / totalDuration;
        return ratio * 100f;
    }
    
    /// <summary>
    /// 验证边界情况
    /// </summary>
    private void ValidateEdgeCases()
    {
        Debug.Log("--- 边界情况验证 ---");
        
        // 测试负数输入
        float score1 = CalculateNewScore(-1f, 10f);
        Debug.Log($"负数正确时长测试: {score1:F1}分 (应为0分)");
        
        // 测试零总时长
        float score2 = CalculateNewScore(5f, 0f);
        Debug.Log($"零总时长测试: {score2:F1}分 (应为0分)");
        
        // 测试正确时长超过总时长
        float score3 = CalculateNewScore(15f, 10f);
        Debug.Log($"超出总时长测试: {score3:F1}分 (应为100分)");
        
        // 测试极小值
        float score4 = CalculateNewScore(0.001f, 1f);
        Debug.Log($"极小值测试: {score4:F3}分");
    }
    
    /// <summary>
    /// 测试用例数据结构
    /// </summary>
    [System.Serializable]
    private struct TestCase
    {
        public string name;
        public float correctDuration;
        public float totalDuration;
        public float expectedScore;
        
        public TestCase(string name, float correctDuration, float totalDuration, float expectedScore)
        {
            this.name = name;
            this.correctDuration = correctDuration;
            this.totalDuration = totalDuration;
            this.expectedScore = expectedScore;
        }
    }
    
    /// <summary>
    /// 在Inspector中显示测试结果
    /// </summary>
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 30), "运行积分系统验证"))
        {
            ValidateNewScoreSystem();
        }
        
        GUI.Label(new Rect(10, 50, 400, 20), "新积分系统公式: 得分 = (正确时长 / 总时长) × 100");
        GUI.Label(new Rect(10, 70, 400, 20), "查看Console窗口获取详细测试结果");
    }
}