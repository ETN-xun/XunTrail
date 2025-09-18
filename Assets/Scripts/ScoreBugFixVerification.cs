using UnityEngine;

/// <summary>
/// 挑战模式计分bug修复验证脚本
/// 用于验证修复后的计分逻辑是否正确处理休止符和普通音符
/// </summary>
public class ScoreBugFixVerification : MonoBehaviour
{
    [Header("验证结果")]
    [SerializeField] private bool restNoteLogicFixed = false;
    [SerializeField] private bool normalNoteLogicFixed = false;
    
    void Start()
    {
        VerifyScoreFix();
    }
    
    /// <summary>
    /// 验证计分修复是否正确
    /// </summary>
    public void VerifyScoreFix()
    {
        Debug.Log("=== 开始验证挑战模式计分bug修复 ===");
        
        // 验证休止符逻辑
        restNoteLogicFixed = VerifyRestNoteLogic();
        
        // 验证普通音符逻辑  
        normalNoteLogicFixed = VerifyNormalNoteLogic();
        
        if (restNoteLogicFixed && normalNoteLogicFixed)
        {
            Debug.Log("✓ 所有计分bug修复验证通过！");
        }
        else
        {
            Debug.LogWarning("✗ 部分计分bug修复验证失败，请检查实现");
        }
        
        Debug.Log("=== 挑战模式计分bug修复验证完成 ===");
    }
    
    /// <summary>
    /// 验证休止符逻辑是否正确
    /// </summary>
    private bool VerifyRestNoteLogic()
    {
        Debug.Log("\n--- 验证休止符逻辑 ---");
        
        var challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager");
            return false;
        }
        
        // 检查IsRestNote方法是否存在并正确识别休止符
        var isRestMethod = typeof(ChallengeManager).GetMethod("IsRestNote", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (isRestMethod == null)
        {
            Debug.LogError("未找到IsRestNote方法");
            return false;
        }
        
        // 测试休止符识别
        bool restTest1 = (bool)isRestMethod.Invoke(challengeManager, new object[] { "rest" });
        bool restTest2 = (bool)isRestMethod.Invoke(challengeManager, new object[] { "R" });
        bool restTest3 = (bool)isRestMethod.Invoke(challengeManager, new object[] { "0" });
        bool restTest4 = (bool)isRestMethod.Invoke(challengeManager, new object[] { "C4" });
        
        if (restTest1 && restTest2 && restTest3 && !restTest4)
        {
            Debug.Log("✓ 休止符识别逻辑正确");
            return true;
        }
        else
        {
            Debug.LogError("✗ 休止符识别逻辑错误");
            return false;
        }
    }
    
    /// <summary>
    /// 验证普通音符逻辑是否正确
    /// </summary>
    private bool VerifyNormalNoteLogic()
    {
        Debug.Log("\n--- 验证普通音符逻辑 ---");
        
        var challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager");
            return false;
        }
        
        // 检查CalculateCorrectTimeForNote方法是否存在
        var calculateMethod = typeof(ChallengeManager).GetMethod("CalculateCorrectTimeForNote", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (calculateMethod == null)
        {
            Debug.LogError("未找到CalculateCorrectTimeForNote方法");
            return false;
        }
        
        Debug.Log("✓ CalculateCorrectTimeForNote方法存在");
        
        // 检查IsNoteMatch方法是否存在
        var matchMethod = typeof(ChallengeManager).GetMethod("IsNoteMatch", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (matchMethod == null)
        {
            Debug.LogError("未找到IsNoteMatch方法");
            return false;
        }
        
        Debug.Log("✓ IsNoteMatch方法存在");
        
        // 测试音符匹配逻辑
        bool matchTest1 = (bool)matchMethod.Invoke(challengeManager, new object[] { "C4", "C4" });
        bool matchTest2 = (bool)matchMethod.Invoke(challengeManager, new object[] { "C4", "D4" });
        bool matchTest3 = (bool)matchMethod.Invoke(challengeManager, new object[] { "C4", "c4" });
        
        if (matchTest1 && !matchTest2 && matchTest3)
        {
            Debug.Log("✓ 音符匹配逻辑正确");
            return true;
        }
        else
        {
            Debug.LogError("✗ 音符匹配逻辑错误");
            return false;
        }
    }
    
    /// <summary>
    /// 获取验证结果摘要
    /// </summary>
    public string GetVerificationSummary()
    {
        return $"休止符逻辑: {(restNoteLogicFixed ? "✓ 已修复" : "✗ 未修复")}\n" +
               $"普通音符逻辑: {(normalNoteLogicFixed ? "✓ 已修复" : "✗ 未修复")}";
    }
}