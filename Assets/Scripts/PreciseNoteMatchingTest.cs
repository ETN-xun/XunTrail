using UnityEngine;
using System.Reflection;

public class PreciseNoteMatchingTest : MonoBehaviour
{
    [Header("测试设置")]
    public bool runTestOnStart = true;
    
    private void Start()
    {
        if (runTestOnStart)
        {
            TestPreciseNoteMatching();
        }
    }
    
    [ContextMenu("运行精确音符匹配测试")]
    public void TestPreciseNoteMatching()
    {
        Debug.Log("=== 开始精确音符匹配测试 ===");
        
        // 获取ChallengeManager实例
        ChallengeManager challengeManager = ChallengeManager.Instance;
        if (challengeManager == null)
        {
            Debug.LogError("无法找到ChallengeManager实例");
            return;
        }
        
        // 使用反射获取私有的IsNoteMatch方法
        MethodInfo isNoteMatchMethod = typeof(ChallengeManager).GetMethod("IsNoteMatch", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (isNoteMatchMethod == null)
        {
            Debug.LogError("无法找到IsNoteMatch方法");
            return;
        }
        
        // 测试用例：应该匹配的情况
        TestNoteMatch(isNoteMatchMethod, challengeManager, "A4", "A4", true, "相同音符应该匹配");
        TestNoteMatch(isNoteMatchMethod, challengeManager, "C3", "C3", true, "相同音符应该匹配");
        TestNoteMatch(isNoteMatchMethod, challengeManager, "G5", "G5", true, "相同音符应该匹配");
        
        // 测试用例：不应该匹配的情况（不同八度）
        TestNoteMatch(isNoteMatchMethod, challengeManager, "A4", "A3", false, "不同八度不应该匹配");
        TestNoteMatch(isNoteMatchMethod, challengeManager, "A4", "A5", false, "不同八度不应该匹配");
        TestNoteMatch(isNoteMatchMethod, challengeManager, "C4", "C3", false, "不同八度不应该匹配");
        TestNoteMatch(isNoteMatchMethod, challengeManager, "G4", "G5", false, "不同八度不应该匹配");
        
        // 测试用例：不应该匹配的情况（不同音符）
        TestNoteMatch(isNoteMatchMethod, challengeManager, "A4", "B4", false, "不同音符不应该匹配");
        TestNoteMatch(isNoteMatchMethod, challengeManager, "C4", "D4", false, "不同音符不应该匹配");
        TestNoteMatch(isNoteMatchMethod, challengeManager, "F3", "G3", false, "不同音符不应该匹配");
        
        // 测试用例：大小写不敏感
        TestNoteMatch(isNoteMatchMethod, challengeManager, "a4", "A4", true, "大小写不敏感应该匹配");
        TestNoteMatch(isNoteMatchMethod, challengeManager, "C4", "c4", true, "大小写不敏感应该匹配");
        
        // 测试用例：空字符串
        TestNoteMatch(isNoteMatchMethod, challengeManager, "", "A4", false, "空字符串不应该匹配");
        TestNoteMatch(isNoteMatchMethod, challengeManager, "A4", "", false, "空字符串不应该匹配");
        TestNoteMatch(isNoteMatchMethod, challengeManager, "", "", false, "两个空字符串不应该匹配");
        
        Debug.Log("=== 精确音符匹配测试完成 ===");
    }
    
    private void TestNoteMatch(MethodInfo method, ChallengeManager instance, 
        string expectedNote, string playedNote, bool expectedResult, string description)
    {
        try
        {
            // 调用IsNoteMatch方法
            object result = method.Invoke(instance, new object[] { expectedNote, playedNote });
            bool actualResult = (bool)result;
            
            // 检查结果
            if (actualResult == expectedResult)
            {
                Debug.Log($"✓ PASS: {description} - 期望: '{expectedNote}', 演奏: '{playedNote}' -> {actualResult}");
            }
            else
            {
                Debug.LogError($"✗ FAIL: {description} - 期望: '{expectedNote}', 演奏: '{playedNote}' -> 预期: {expectedResult}, 实际: {actualResult}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"✗ ERROR: {description} - 测试执行失败: {e.Message}");
        }
    }
    
    [ContextMenu("测试ToneGenerator音符生成")]
    public void TestToneGeneratorNoteGeneration()
    {
        Debug.Log("=== 开始ToneGenerator音符生成测试 ===");
        
        ToneGenerator toneGenerator = FindObjectOfType<ToneGenerator>();
        if (toneGenerator == null)
        {
            Debug.LogError("无法找到ToneGenerator实例");
            return;
        }
        
        // 使用反射获取私有的GetNoteFromFrequency方法
        MethodInfo getNoteFromFrequencyMethod = typeof(ToneGenerator).GetMethod("GetNoteFromFrequency", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (getNoteFromFrequencyMethod == null)
        {
            Debug.LogError("无法找到GetNoteFromFrequency方法");
            return;
        }
        
        // 测试一些标准频率
        TestFrequencyToNote(getNoteFromFrequencyMethod, toneGenerator, 440f, "A4", "标准A4频率");
        TestFrequencyToNote(getNoteFromFrequencyMethod, toneGenerator, 261.63f, "C4", "中央C频率");
        TestFrequencyToNote(getNoteFromFrequencyMethod, toneGenerator, 220f, "A3", "A3频率");
        TestFrequencyToNote(getNoteFromFrequencyMethod, toneGenerator, 880f, "A5", "A5频率");
        TestFrequencyToNote(getNoteFromFrequencyMethod, toneGenerator, 329.63f, "E4", "E4频率");
        
        Debug.Log("=== ToneGenerator音符生成测试完成 ===");
    }
    
    private void TestFrequencyToNote(MethodInfo method, ToneGenerator instance, 
        float frequency, string expectedNote, string description)
    {
        try
        {
            // 调用GetNoteFromFrequency方法
            object result = method.Invoke(instance, new object[] { frequency });
            string actualNote = (string)result;
            
            // 检查结果
            if (actualNote == expectedNote)
            {
                Debug.Log($"✓ PASS: {description} - 频率: {frequency}Hz -> {actualNote}");
            }
            else
            {
                Debug.LogWarning($"? INFO: {description} - 频率: {frequency}Hz -> 预期: {expectedNote}, 实际: {actualNote}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"✗ ERROR: {description} - 测试执行失败: {e.Message}");
        }
    }
}