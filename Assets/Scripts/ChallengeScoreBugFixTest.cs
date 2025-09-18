using UnityEngine;
using System.Collections.Generic;

public class ChallengeScoreBugFixTest : MonoBehaviour
{
    [Header("测试配置")]
    public bool runTestOnStart = true;
    public bool enableDetailedLogs = true;
    
    private ChallengeManager challengeManager;
    
    void Start()
    {
        if (runTestOnStart)
        {
            RunAllTests();
        }
    }
    
    public void RunAllTests()
    {
        challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager，无法进行测试");
            return;
        }
        
        Debug.Log("=== 开始挑战模式计分bug修复测试 ===");
        
        // 测试1: 休止符期间不演奏应该加分
        TestRestNoteWithoutPlaying();
        
        // 测试2: 休止符期间演奏应该扣分
        TestRestNoteWithPlaying();
        
        // 测试3: 普通音符期间不演奏应该不加分
        TestNormalNoteWithoutPlaying();
        
        // 测试4: 普通音符期间演奏正确音符应该加分
        TestNormalNoteWithCorrectPlaying();
        
        // 测试5: 普通音符期间演奏错误音符应该不加分
        TestNormalNoteWithWrongPlaying();
        
        Debug.Log("=== 挑战模式计分bug修复测试完成 ===");
    }
    
    private void TestRestNoteWithoutPlaying()
    {
        Debug.Log("\n--- 测试1: 休止符期间不演奏应该加分 ---");
        
        // 创建测试数据：一个休止符
        var timedNote = new ChallengeManager.TimedNote("rest", 0f, 2f);
        var playerPerformance = new List<ChallengeManager.PlayerNote>();
        
        // 模拟没有演奏记录
        float correctTime = TestCalculateCorrectTimeForNote(timedNote, playerPerformance);
        
        Debug.Log($"期望结果: {timedNote.duration}s, 实际结果: {correctTime}s");
        
        if (Mathf.Approximately(correctTime, timedNote.duration))
        {
            Debug.Log("✓ 测试1通过: 休止符期间不演奏正确加分");
        }
        else
        {
            Debug.LogError("✗ 测试1失败: 休止符期间不演奏未正确加分");
        }
    }
    
    private void TestRestNoteWithPlaying()
    {
        Debug.Log("\n--- 测试2: 休止符期间演奏应该扣分 ---");
        
        // 创建测试数据：一个休止符
        var timedNote = new ChallengeManager.TimedNote("rest", 0f, 2f);
        var playerPerformance = new List<ChallengeManager.PlayerNote>
        {
            new ChallengeManager.PlayerNote("C4", 0.5f, 1f) // 在休止符期间演奏了1秒
        };
        
        float correctTime = TestCalculateCorrectTimeForNote(timedNote, playerPerformance);
        float expectedTime = timedNote.duration - 1f; // 应该扣除演奏的1秒
        
        Debug.Log($"期望结果: {expectedTime}s, 实际结果: {correctTime}s");
        
        if (Mathf.Approximately(correctTime, expectedTime))
        {
            Debug.Log("✓ 测试2通过: 休止符期间演奏正确扣分");
        }
        else
        {
            Debug.LogError("✗ 测试2失败: 休止符期间演奏未正确扣分");
        }
    }
    
    private void TestNormalNoteWithoutPlaying()
    {
        Debug.Log("\n--- 测试3: 普通音符期间不演奏应该不加分 ---");
        
        // 创建测试数据：一个普通音符
        var timedNote = new ChallengeManager.TimedNote("C4", 0f, 2f);
        var playerPerformance = new List<ChallengeManager.PlayerNote>();
        
        // 模拟没有演奏记录
        float correctTime = TestCalculateCorrectTimeForNote(timedNote, playerPerformance);
        
        Debug.Log($"期望结果: 0s, 实际结果: {correctTime}s");
        
        if (Mathf.Approximately(correctTime, 0f))
        {
            Debug.Log("✓ 测试3通过: 普通音符期间不演奏正确不加分");
        }
        else
        {
            Debug.LogError("✗ 测试3失败: 普通音符期间不演奏错误加分");
        }
    }
    
    private void TestNormalNoteWithCorrectPlaying()
    {
        Debug.Log("\n--- 测试4: 普通音符期间演奏正确音符应该加分 ---");
        
        // 创建测试数据：一个普通音符
        var timedNote = new ChallengeManager.TimedNote("C4", 0f, 2f);
        var playerPerformance = new List<ChallengeManager.PlayerNote>
        {
            new ChallengeManager.PlayerNote("C4", 0.5f, 1f) // 演奏了正确的音符1秒
        };
        
        float correctTime = TestCalculateCorrectTimeForNote(timedNote, playerPerformance);
        float expectedTime = 1f; // 应该得到1秒的正确时间
        
        Debug.Log($"期望结果: {expectedTime}s, 实际结果: {correctTime}s");
        
        if (Mathf.Approximately(correctTime, expectedTime))
        {
            Debug.Log("✓ 测试4通过: 普通音符期间演奏正确音符正确加分");
        }
        else
        {
            Debug.LogError("✗ 测试4失败: 普通音符期间演奏正确音符未正确加分");
        }
    }
    
    private void TestNormalNoteWithWrongPlaying()
    {
        Debug.Log("\n--- 测试5: 普通音符期间演奏错误音符应该不加分 ---");
        
        // 创建测试数据：一个普通音符
        var timedNote = new ChallengeManager.TimedNote("C4", 0f, 2f);
        var playerPerformance = new List<ChallengeManager.PlayerNote>
        {
            new ChallengeManager.PlayerNote("D4", 0.5f, 1f) // 演奏了错误的音符1秒
        };
        
        float correctTime = TestCalculateCorrectTimeForNote(timedNote, playerPerformance);
        
        Debug.Log($"期望结果: 0s, 实际结果: {correctTime}s");
        
        if (Mathf.Approximately(correctTime, 0f))
        {
            Debug.Log("✓ 测试5通过: 普通音符期间演奏错误音符正确不加分");
        }
        else
        {
            Debug.LogError("✗ 测试5失败: 普通音符期间演奏错误音符错误加分");
        }
    }
    
    private float TestCalculateCorrectTimeForNote(ChallengeManager.TimedNote timedNote, List<ChallengeManager.PlayerNote> playerPerformance)
    {
        // 使用反射调用私有方法进行测试
        var method = typeof(ChallengeManager).GetMethod("CalculateCorrectTimeForNote", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method == null)
        {
            Debug.LogError("无法找到CalculateCorrectTimeForNote方法");
            return 0f;
        }
        
        // 临时设置playerPerformance
        var performanceField = typeof(ChallengeManager).GetField("playerPerformance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (performanceField == null)
        {
            Debug.LogError("无法找到playerPerformance字段");
            return 0f;
        }
        
        var originalPerformance = performanceField.GetValue(challengeManager);
        performanceField.SetValue(challengeManager, playerPerformance);
        
        try
        {
            var result = method.Invoke(challengeManager, new object[] { timedNote });
            return (float)result;
        }
        finally
        {
            // 恢复原始数据
            performanceField.SetValue(challengeManager, originalPerformance);
        }
    }
}