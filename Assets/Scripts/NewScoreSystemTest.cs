using UnityEngine;
using System.Collections;

public class NewScoreSystemTest : MonoBehaviour
{
    [Header("测试设置")]
    public ChallengeManager challengeManager;
    public bool autoStartTest = true;
    public float testDuration = 10f; // 测试持续时间
    
    [Header("测试结果")]
    public float totalMusicDuration;
    public float correctPlayTime;
    public float finalScore;
    
    void Start()
    {
        if (autoStartTest)
        {
            StartCoroutine(TestNewScoreSystem());
        }
    }
    
    IEnumerator TestNewScoreSystem()
    {
        Debug.Log("=== 新积分系统测试开始 ===");
        
        // 等待ChallengeManager初始化
        yield return new WaitForSeconds(1f);
        
        if (challengeManager == null)
        {
            challengeManager = FindObjectOfType<ChallengeManager>();
        }
        
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager，测试失败");
            yield break;
        }
        
        // 开始挑战
        Debug.Log("开始挑战测试...");
        challengeManager.StartChallenge();
        
        // 等待挑战开始
        yield return new WaitForSeconds(0.5f);
        
        // 获取总时长
        totalMusicDuration = GetTotalMusicDuration();
        Debug.Log($"乐谱总时长: {totalMusicDuration:F2}秒");
        
        // 模拟演奏过程
        float testTime = 0f;
        while (testTime < testDuration && !IsChallengeCompleted())
        {
            // 获取当前积分信息
            correctPlayTime = GetCorrectPlayTime();
            finalScore = GetCurrentScore();
            
            Debug.Log($"测试进行中 - 时间: {testTime:F1}s, 正确时长: {correctPlayTime:F2}s, 当前得分: {finalScore:F1}%");
            
            yield return new WaitForSeconds(1f);
            testTime += 1f;
        }
        
        // 最终结果
        correctPlayTime = GetCorrectPlayTime();
        finalScore = GetCurrentScore();
        
        Debug.Log("=== 新积分系统测试结果 ===");
        Debug.Log($"乐谱总时长: {totalMusicDuration:F2}秒");
        Debug.Log($"正确演奏时长: {correctPlayTime:F2}秒");
        Debug.Log($"最终得分: {finalScore:F1}%");
        Debug.Log($"得分计算公式: {correctPlayTime:F2} / {totalMusicDuration:F2} * 100 = {finalScore:F1}%");
        
        // 验证计算是否正确
        float expectedScore = totalMusicDuration > 0 ? (correctPlayTime / totalMusicDuration) * 100f : 0f;
        expectedScore = Mathf.Clamp(expectedScore, 0f, 100f);
        
        if (Mathf.Abs(finalScore - expectedScore) < 0.1f)
        {
            Debug.Log("✓ 积分计算正确！");
        }
        else
        {
            Debug.LogWarning($"✗ 积分计算可能有误。期望: {expectedScore:F1}%, 实际: {finalScore:F1}%");
        }
    }
    
    // 通过反射获取私有变量
    private float GetTotalMusicDuration()
    {
        if (challengeManager == null) return 0f;
        
        var field = typeof(ChallengeManager).GetField("totalMusicDuration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (float)field.GetValue(challengeManager);
        }
        return 0f;
    }
    
    private float GetCorrectPlayTime()
    {
        if (challengeManager == null) return 0f;
        
        var field = typeof(ChallengeManager).GetField("correctPlayTime", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (float)field.GetValue(challengeManager);
        }
        return 0f;
    }
    
    private float GetCurrentScore()
    {
        if (challengeManager == null) return 0f;
        
        var method = typeof(ChallengeManager).GetMethod("CalculateNewScore", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method != null)
        {
            return (float)method.Invoke(challengeManager, null);
        }
        return 0f;
    }
    
    private bool IsChallengeCompleted()
    {
        if (challengeManager == null) return true;
        
        var field = typeof(ChallengeManager).GetField("challengeCompleted", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (bool)field.GetValue(challengeManager);
        }
        return true;
    }
    
    // 手动触发测试
    [ContextMenu("开始测试")]
    public void StartTest()
    {
        StartCoroutine(TestNewScoreSystem());
    }
    
    // 显示当前状态
    void OnGUI()
    {
        if (challengeManager == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("=== 新积分系统实时监控 ===");
        GUILayout.Label($"乐谱总时长: {GetTotalMusicDuration():F2}秒");
        GUILayout.Label($"正确演奏时长: {GetCorrectPlayTime():F2}秒");
        GUILayout.Label($"当前得分: {GetCurrentScore():F1}%");
        
        if (GUILayout.Button("开始测试"))
        {
            StartTest();
        }
        
        GUILayout.EndArea();
    }
}