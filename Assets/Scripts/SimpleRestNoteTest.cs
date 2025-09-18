using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class SimpleRestNoteTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 开始休止符修复验证测试 ===");
        TestRestNoteIdentification();
        TestTimedNoteSequenceGeneration();
    }

    void TestRestNoteIdentification()
    {
        Debug.Log("--- 测试1: 休止符识别 ---");
        
        ChallengeManager challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager");
            return;
        }

        // 使用反射调用私有的IsRestNote方法
        MethodInfo isRestNoteMethod = typeof(ChallengeManager).GetMethod("IsRestNote", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (isRestNoteMethod == null)
        {
            Debug.LogError("无法找到IsRestNote方法");
            return;
        }

        // 测试各种休止符格式
        string[] restFormats = { "rest", "r", "pause", "0", "R", "REST", "PAUSE" };
        
        foreach (string format in restFormats)
        {
            bool isRest = (bool)isRestNoteMethod.Invoke(challengeManager, new object[] { format });
            Debug.Log($"格式 '{format}' 识别为休止符: {isRest}");
        }
        
        // 测试非休止符
        string[] nonRestFormats = { "C4", "D4", "E4", "note", "music" };
        
        foreach (string format in nonRestFormats)
        {
            bool isRest = (bool)isRestNoteMethod.Invoke(challengeManager, new object[] { format });
            Debug.Log($"格式 '{format}' 识别为休止符: {isRest} (应该为false)");
        }
    }

    void TestTimedNoteSequenceGeneration()
    {
        Debug.Log("--- 测试2: 时间序列生成 ---");
        
        // 获取IsRestNote方法的反射引用
        MethodInfo isRestNoteMethod = typeof(ChallengeManager).GetMethod("IsRestNote", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        // 创建包含休止符的测试乐谱
        MusicSheet testSheet = new MusicSheet();
        testSheet.bpm = 120f; // 设置BPM
        testSheet.notes = new List<Note>
        {
            new Note("C4", 1.0f),
            new Note("R", 1.0f),
            new Note("D4", 1.0f),
            new Note("rest", 1.0f),
            new Note("E4", 1.0f)
        };

        ChallengeManager challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager");
            return;
        }

        // 使用反射调用私有方法
        var method = typeof(ChallengeManager).GetMethod("GenerateTimedNoteSequenceFromSheet", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method != null)
        {
            method.Invoke(challengeManager, new object[] { testSheet });
            
            // 获取生成的时间序列
            var field = typeof(ChallengeManager).GetField("timedNoteSequence", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                var sequence = field.GetValue(challengeManager) as List<ChallengeManager.TimedNote>;
                
                if (sequence != null)
                {
                    Debug.Log($"生成的时间序列包含 {sequence.Count} 个音符");
                    
                    for (int i = 0; i < sequence.Count; i++)
                    {
                        var note = sequence[i];
                        bool isRest = (bool)isRestNoteMethod.Invoke(challengeManager, new object[] { note.noteName });
                        Debug.Log($"时间 {note.startTime:F2}s: {note.noteName} (休止符: {isRest})");
                    }
                    
                    // 验证休止符是否被正确包含
                    int restCount = 0;
                    foreach (var note in sequence)
                    {
                        if ((bool)isRestNoteMethod.Invoke(challengeManager, new object[] { note.noteName }))
                        {
                            restCount++;
                        }
                    }
                    
                    Debug.Log($"✓ 时间序列中包含 {restCount} 个休止符 (预期: 2)");
                    
                    if (restCount == 2)
                    {
                        Debug.Log("✓ 休止符修复验证成功！");
                    }
                    else
                    {
                        Debug.LogError("✗ 休止符数量不正确");
                    }
                }
                else
                {
                    Debug.LogError("无法获取时间序列");
                }
            }
            else
            {
                Debug.LogError("无法找到timedNoteSequence字段");
            }
        }
        else
        {
            Debug.LogError("无法找到GenerateTimedNoteSequenceFromSheet方法");
        }
    }
}