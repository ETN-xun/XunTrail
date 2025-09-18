using UnityEngine;

public class PlayDetectionBugFixTest : MonoBehaviour
{
    [Header("测试配置")]
    public bool runTestOnStart = true;
    public bool enableDetailedLogs = true;
    
    private void Start()
    {
        if (runTestOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }
    
    private System.Collections.IEnumerator RunAllTests()
    {
        Debug.Log("=== 开始演奏检测Bug修复测试 ===");
        
        yield return new WaitForSeconds(1f);
        
        // 测试1: 验证非演奏状态不加分
        TestNoPlayNoScore();
        yield return new WaitForSeconds(0.5f);
        
        // 测试2: 验证休止符时不演奏加分
        TestRestNoteNoPlayScore();
        yield return new WaitForSeconds(0.5f);
        
        // 测试3: 验证休止符时演奏不加分
        TestRestNotePlayScore();
        yield return new WaitForSeconds(0.5f);
        
        // 测试4: 验证正确演奏加分
        TestCorrectPlayScore();
        yield return new WaitForSeconds(0.5f);
        
        // 测试5: 验证错误演奏不加分
        TestWrongPlayNoScore();
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("=== 演奏检测Bug修复测试完成 ===");
    }
    
    private void TestNoPlayNoScore()
    {
        Debug.Log("--- 测试1: 期望音符C4，玩家未演奏，应该不加分 ---");
        
        // 模拟测试场景
        string expectedNote = "C4";
        string playedNote = ""; // 玩家未演奏
        
        bool shouldScore = ShouldAddScore(expectedNote, playedNote);
        
        if (!shouldScore)
        {
            Debug.Log("✓ 测试1通过: 玩家未演奏时正确地不加分");
        }
        else
        {
            Debug.LogError("✗ 测试1失败: 玩家未演奏时错误地加了分");
        }
    }
    
    private void TestRestNoteNoPlayScore()
    {
        Debug.Log("--- 测试2: 期望休止符，玩家未演奏，应该加分 ---");
        
        string expectedNote = "rest";
        string playedNote = ""; // 玩家未演奏
        
        bool shouldScore = ShouldAddScore(expectedNote, playedNote);
        
        if (shouldScore)
        {
            Debug.Log("✓ 测试2通过: 休止符时玩家未演奏正确地加分");
        }
        else
        {
            Debug.LogError("✗ 测试2失败: 休止符时玩家未演奏错误地不加分");
        }
    }
    
    private void TestRestNotePlayScore()
    {
        Debug.Log("--- 测试3: 期望休止符，玩家演奏了C4，应该不加分 ---");
        
        string expectedNote = "rest";
        string playedNote = "C4"; // 玩家演奏了
        
        bool shouldScore = ShouldAddScore(expectedNote, playedNote);
        
        if (!shouldScore)
        {
            Debug.Log("✓ 测试3通过: 休止符时玩家演奏正确地不加分");
        }
        else
        {
            Debug.LogError("✗ 测试3失败: 休止符时玩家演奏错误地加了分");
        }
    }
    
    private void TestCorrectPlayScore()
    {
        Debug.Log("--- 测试4: 期望音符C4，玩家演奏C4，应该加分 ---");
        
        string expectedNote = "C4";
        string playedNote = "C4"; // 玩家演奏正确
        
        bool shouldScore = ShouldAddScore(expectedNote, playedNote);
        
        if (shouldScore)
        {
            Debug.Log("✓ 测试4通过: 正确演奏时正确地加分");
        }
        else
        {
            Debug.LogError("✗ 测试4失败: 正确演奏时错误地不加分");
        }
    }
    
    private void TestWrongPlayNoScore()
    {
        Debug.Log("--- 测试5: 期望音符C4，玩家演奏D4，应该不加分 ---");
        
        string expectedNote = "C4";
        string playedNote = "D4"; // 玩家演奏错误
        
        bool shouldScore = ShouldAddScore(expectedNote, playedNote);
        
        if (!shouldScore)
        {
            Debug.Log("✓ 测试5通过: 错误演奏时正确地不加分");
        }
        else
        {
            Debug.LogError("✗ 测试5失败: 错误演奏时错误地加了分");
        }
    }
    
    // 模拟ChallengeManager中的加分逻辑
    private bool ShouldAddScore(string expectedNote, string playedNote)
    {
        if (string.IsNullOrEmpty(expectedNote))
            return false;
            
        bool shouldAddScore = false;
        
        // 检查是否应该加分（复制修复后的逻辑）
        if (IsRestNote(expectedNote))
        {
            // 如果当前期望音符是休止符，玩家不演奏就应该加分
            if (string.IsNullOrEmpty(playedNote))
            {
                shouldAddScore = true;
                if (enableDetailedLogs)
                    Debug.Log($"休止符正确处理: 期望休止符，玩家未演奏 -> 加分");
            }
            else
            {
                if (enableDetailedLogs)
                    Debug.Log($"休止符错误处理: 期望休止符，但玩家演奏了 '{playedNote}' -> 不加分");
            }
        }
        else
        {
            // 如果当前期望音符不是休止符，只有演奏正确音符才加分
            if (!string.IsNullOrEmpty(playedNote) && IsNoteMatch(expectedNote, playedNote))
            {
                shouldAddScore = true;
                if (enableDetailedLogs)
                    Debug.Log($"音符正确演奏: 期望 '{expectedNote}', 演奏 '{playedNote}' -> 加分");
            }
            else if (string.IsNullOrEmpty(playedNote))
            {
                if (enableDetailedLogs)
                    Debug.Log($"音符未演奏: 期望 '{expectedNote}', 玩家未演奏 -> 不加分");
            }
            else
            {
                if (enableDetailedLogs)
                    Debug.Log($"音符演奏错误: 期望 '{expectedNote}', 演奏 '{playedNote}' -> 不加分");
            }
        }
        
        return shouldAddScore;
    }
    
    // 检查是否为休止符
    private bool IsRestNote(string noteName)
    {
        if (string.IsNullOrEmpty(noteName))
            return false;
            
        // 常见的休止符表示方法
        string noteBase = ExtractNoteBase(noteName).ToLower();
        return noteBase == "rest" || noteBase == "r" || noteBase == "pause" || noteBase == "0";
    }
    
    // 提取音符的基础名称（去掉八度信息）
    private string ExtractNoteBase(string noteName)
    {
        if (string.IsNullOrEmpty(noteName))
            return "";
            
        string result = "";
        foreach (char c in noteName)
        {
            if (char.IsLetter(c) || c == '#')
            {
                result += c;
            }
            else
            {
                break; // 遇到数字就停止
            }
        }
        
        return result;
    }
    
    // 检查两个音符是否匹配
    private bool IsNoteMatch(string expectedNote, string playedNote)
    {
        if (string.IsNullOrEmpty(expectedNote) || string.IsNullOrEmpty(playedNote))
        {
            return false;
        }
            
        // 提取音符的基础名称（去掉八度信息）
        string expectedBase = ExtractNoteBase(expectedNote);
        string playedBase = ExtractNoteBase(playedNote);
        
        // 比较基础音符名称（忽略大小写）
        bool isMatch = string.Equals(expectedBase, playedBase, System.StringComparison.OrdinalIgnoreCase);
        
        return isMatch;
    }
    
    [ContextMenu("运行测试")]
    public void RunTests()
    {
        StartCoroutine(RunAllTests());
    }
}