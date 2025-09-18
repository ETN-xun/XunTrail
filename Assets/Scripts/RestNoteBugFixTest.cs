using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 休止符Bug修复验证测试
/// 验证修复后的休止符功能是否正常工作
/// </summary>
public class RestNoteBugFixTest : MonoBehaviour
{
    [Header("测试控制")]
    public bool runTestOnStart = false;
    
    private ChallengeManager challengeManager;
    
    void Start()
    {
        challengeManager = FindObjectOfType<ChallengeManager>();
        
        if (runTestOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }
    
    void Update()
    {
        // 按T键运行测试
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(RunAllTests());
        }
    }
    
    private System.Collections.IEnumerator RunAllTests()
    {
        Debug.Log("=== 开始休止符Bug修复验证测试 ===");
        
        // 测试1: 验证休止符能正确添加到时间序列中
        yield return StartCoroutine(TestRestNoteInTimedSequence());
        
        // 测试2: 验证休止符评分逻辑
        yield return StartCoroutine(TestRestNoteScoring());
        
        // 测试3: 验证包含休止符的乐谱能正确解析
        yield return StartCoroutine(TestRestNoteSheetParsing());
        
        Debug.Log("=== 休止符Bug修复验证测试完成 ===");
    }
    
    private System.Collections.IEnumerator TestRestNoteInTimedSequence()
    {
        Debug.Log("\n--- 测试1: 验证休止符能正确添加到时间序列中 ---");
        
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager");
            yield break;
        }
        
        // 加载包含休止符的乐谱
        string restSheetPath = "Assets/乐谱/间隔空.txt";
        var musicSheetParser = FindObjectOfType<MusicSheetParser>();
        
        if (musicSheetParser == null)
        {
            Debug.LogError("未找到MusicSheetParser");
            yield break;
        }
        
        // 解析乐谱
        var musicSheet = musicSheetParser.ParseMusicSheet(restSheetPath);
        if (musicSheet == null)
        {
            Debug.LogError($"无法解析乐谱: {restSheetPath}");
            yield break;
        }
        
        Debug.Log($"乐谱解析成功，包含 {musicSheet.notes.Count} 个音符");
        
        // 检查乐谱中的休止符
        int restCount = 0;
        for (int i = 0; i < musicSheet.notes.Count; i++)
        {
            var note = musicSheet.notes[i];
            Debug.Log($"音符 {i}: {note.noteName}, isRest: {note.isRest}");
            if (note.isRest)
            {
                restCount++;
            }
        }
        
        Debug.Log($"乐谱中包含 {restCount} 个休止符");
        
        // 启动挑战模式测试时间序列生成
        challengeManager.StartChallenge(musicSheet);
        
        yield return new WaitForSeconds(1f);
        
        // 检查时间序列是否包含休止符
        var timedSequenceField = typeof(ChallengeManager).GetField("timedNoteSequence", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (timedSequenceField != null)
        {
            var timedSequence = timedSequenceField.GetValue(challengeManager) as List<ChallengeManager.TimedNote>;
            if (timedSequence != null)
            {
                Debug.Log($"时间序列包含 {timedSequence.Count} 个音符");
                
                int timedRestCount = 0;
                foreach (var timedNote in timedSequence)
                {
                    bool isRest = IsRestNote(timedNote.noteName);
                    Debug.Log($"时间音符: {timedNote.noteName} ({timedNote.startTime:F2}s - {timedNote.endTime:F2}s), 是否休止符: {isRest}");
                    if (isRest)
                    {
                        timedRestCount++;
                    }
                }
                
                Debug.Log($"时间序列中包含 {timedRestCount} 个休止符");
                
                if (timedRestCount > 0)
                {
                    Debug.Log("✓ 测试1通过: 休止符正确添加到时间序列中");
                }
                else
                {
                    Debug.LogError("✗ 测试1失败: 时间序列中没有休止符");
                }
            }
        }
        
        challengeManager.ExitChallenge();
    }
    
    private System.Collections.IEnumerator TestRestNoteScoring()
    {
        Debug.Log("\n--- 测试2: 验证休止符评分逻辑 ---");
        
        // 创建包含休止符的测试序列
        var testSequence = new List<ChallengeManager.TimedNote>
        {
            new ChallengeManager.TimedNote("C4", 0f, 1f),
            new ChallengeManager.TimedNote("R", 1f, 2f),  // 休止符
            new ChallengeManager.TimedNote("D4", 2f, 3f)
        };
        
        // 使用反射设置测试序列
        var timedSequenceField = typeof(ChallengeManager).GetField("timedNoteSequence", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (timedSequenceField != null)
        {
            timedSequenceField.SetValue(challengeManager, testSequence);
        }
        
        // 启动挑战模式
        challengeManager.StartChallenge();
        
        yield return new WaitForSeconds(0.5f);
        
        // 模拟演奏第一个音符
        challengeManager.OnNoteDetected("C4");
        
        yield return new WaitForSeconds(1f);
        
        // 在休止符期间不演奏（应该加分）
        Debug.Log("休止符期间，玩家不演奏...");
        
        yield return new WaitForSeconds(1f);
        
        // 演奏第三个音符
        challengeManager.OnNoteDetected("D4");
        
        yield return new WaitForSeconds(1f);
        
        challengeManager.ExitChallenge();
        
        Debug.Log("✓ 测试2完成: 休止符评分逻辑测试");
    }
    
    private System.Collections.IEnumerator TestRestNoteSheetParsing()
    {
        Debug.Log("\n--- 测试3: 验证包含休止符的乐谱能正确解析 ---");
        
        // 测试不同的休止符乐谱文件
        string[] testSheets = {
            "Assets/乐谱/纯空乐谱.txt",
            "Assets/乐谱/间隔空.txt"
        };
        
        var musicSheetParser = FindObjectOfType<MusicSheetParser>();
        
        foreach (string sheetPath in testSheets)
        {
            Debug.Log($"测试乐谱: {sheetPath}");
            
            var musicSheet = musicSheetParser.ParseMusicSheet(sheetPath);
            if (musicSheet != null)
            {
                Debug.Log($"乐谱解析成功，包含 {musicSheet.notes.Count} 个音符");
                
                for (int i = 0; i < musicSheet.notes.Count && i < 5; i++)
                {
                    var note = musicSheet.notes[i];
                    string noteType = note.isRest ? "休止符" : "音符";
                    Debug.Log($"  {i}: {note.noteName} ({noteType})");
                }
            }
            else
            {
                Debug.LogError($"无法解析乐谱: {sheetPath}");
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("✓ 测试3完成: 乐谱解析测试");
    }
    
    // 检查是否为休止符
    private bool IsRestNote(string noteName)
    {
        if (string.IsNullOrEmpty(noteName))
            return false;
            
        string noteBase = noteName.ToLower();
        return noteBase == "rest" || noteBase == "r" || noteBase == "pause" || noteBase == "0";
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label("休止符Bug修复验证测试");
        
        if (GUILayout.Button("运行测试 (T键)"))
        {
            StartCoroutine(RunAllTests());
        }
        
        GUILayout.Label("按T键运行测试");
        GUILayout.EndArea();
    }
}