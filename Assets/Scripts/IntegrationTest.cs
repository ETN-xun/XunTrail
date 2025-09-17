using UnityEngine;
using System.Collections.Generic;

public class IntegrationTest : MonoBehaviour
{
    [Header("测试设置")]
    public ChallengeManager challengeManager;
    public bool runTestOnStart = true;
    public bool testTimedNoteGeneration = true;
    public bool testRealtimeDetection = true;
    public bool testScoring = true;
    
    private void Start()
    {
        if (runTestOnStart)
        {
            StartCoroutine(RunIntegrationTests());
        }
    }
    
    private System.Collections.IEnumerator RunIntegrationTests()
    {
        Debug.Log("=== 开始挑战模式集成测试 ===");
        
        // 等待一秒确保所有组件初始化完成
        yield return new WaitForSeconds(1f);
        
        if (challengeManager == null)
        {
            challengeManager = FindObjectOfType<ChallengeManager>();
            if (challengeManager == null)
            {
                Debug.LogError("未找到ChallengeManager组件！");
                yield break;
            }
        }
        
        // 测试1: 带时间音符序列生成
        if (testTimedNoteGeneration)
        {
            yield return TestTimedNoteGeneration();
        }
        
        // 测试2: 实时音符检测
        if (testRealtimeDetection)
        {
            yield return TestRealtimeDetection();
        }
        
        // 测试3: 评分系统
        if (testScoring)
        {
            yield return TestScoringSystem();
        }
        
        Debug.Log("=== 集成测试完成 ===");
    }
    
    private System.Collections.IEnumerator TestTimedNoteGeneration()
    {
        Debug.Log("--- 测试1: 带时间音符序列生成 ---");
        
        bool testPassed = false;
        string errorMessage = "";
        
        try
        {
            // 测试生成随机带时间音符序列
            var timedNotes = challengeManager.GetType()
                .GetMethod("GenerateTimedNoteSequence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(challengeManager, new object[] { 5, 120f });
            
            if (timedNotes != null)
            {
                testPassed = true;
            }
        }
        catch (System.Exception e)
        {
            errorMessage = e.Message;
        }
        
        if (testPassed)
        {
            Debug.Log("✓ 成功生成带时间音符序列");
        }
        else
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Debug.LogError($"✗ 测试带时间音符序列生成时出错: {errorMessage}");
            }
            else
            {
                Debug.LogWarning("✗ 生成带时间音符序列失败");
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    private System.Collections.IEnumerator TestRealtimeDetection()
    {
        Debug.Log("--- 测试2: 实时音符检测 ---");
        
        bool testPassed = false;
        string errorMessage = "";
        
        try
        {
            // 模拟音符检测
            challengeManager.OnNoteDetected("C4");
            challengeManager.OnNoteDetected("D4");
            challengeManager.OnNoteDetected("E4");
            testPassed = true;
        }
        catch (System.Exception e)
        {
            errorMessage = e.Message;
        }
        
        yield return new WaitForSeconds(0.3f);
        
        if (testPassed)
        {
            Debug.Log("✓ 实时音符检测测试完成");
        }
        else
        {
            Debug.LogError($"✗ 测试实时音符检测时出错: {errorMessage}");
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    private System.Collections.IEnumerator TestScoringSystem()
    {
        Debug.Log("--- 测试3: 评分系统 ---");
        
        bool testPassed = false;
        string errorMessage = "";
        object similarity = null;
        
        try
        {
            // 测试评分计算
            similarity = challengeManager.GetType()
                .GetMethod("CalculatePerformanceSimilarity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(challengeManager, null);
            
            if (similarity != null)
            {
                testPassed = true;
            }
        }
        catch (System.Exception e)
        {
            errorMessage = e.Message;
        }
        
        if (testPassed)
        {
            Debug.Log($"✓ 评分系统测试完成，当前相似度: {similarity}%");
        }
        else
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Debug.LogError($"✗ 测试评分系统时出错: {errorMessage}");
            }
            else
            {
                Debug.LogWarning("✗ 评分系统测试失败");
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    [ContextMenu("运行完整挑战测试")]
    public void RunFullChallengeTest()
    {
        StartCoroutine(RunFullChallengeTestCoroutine());
    }
    
    private System.Collections.IEnumerator RunFullChallengeTestCoroutine()
    {
        Debug.Log("=== 开始完整挑战测试 ===");
        
        if (challengeManager == null)
        {
            Debug.LogError("ChallengeManager未设置！");
            yield break;
        }
        
        // 启动挑战
        challengeManager.StartChallenge();
        Debug.Log("挑战已启动");
        
        // 等待倒计时结束
        yield return new WaitForSeconds(4f);
        
        // 模拟演奏一些音符
        for (int i = 0; i < 10; i++)
        {
            string[] testNotes = { "C4", "D4", "E4", "F4", "G4", "A4", "B4" };
            string randomNote = testNotes[Random.Range(0, testNotes.Length)];
            
            challengeManager.OnNoteDetected(randomNote);
            Debug.Log($"模拟演奏音符: {randomNote}");
            
            yield return new WaitForSeconds(0.5f);
        }
        
        // 等待挑战自然结束或手动结束
        yield return new WaitForSeconds(2f);
        challengeManager.ExitChallenge();
        
        Debug.Log("=== 完整挑战测试结束 ===");
    }
}