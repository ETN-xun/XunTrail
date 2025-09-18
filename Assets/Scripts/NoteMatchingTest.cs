using UnityEngine;

public class NoteMatchingTest : MonoBehaviour
{
    [Header("测试配置")]
    public bool runTestOnStart = true;
    
    private ToneGenerator toneGenerator;
    private ChallengeManager challengeManager;
    
    void Start()
    {
        if (runTestOnStart)
        {
            RunTests();
        }
    }
    
    public void RunTests()
    {
        Debug.Log("=== 开始音符匹配测试 ===");
        
        // 获取组件
        toneGenerator = FindObjectOfType<ToneGenerator>();
        challengeManager = FindObjectOfType<ChallengeManager>();
        
        if (toneGenerator == null)
        {
            Debug.LogError("未找到 ToneGenerator 组件");
            return;
        }
        
        if (challengeManager == null)
        {
            Debug.LogError("未找到 ChallengeManager 组件");
            return;
        }
        
        // 测试不同调号下的频率计算
        TestFrequencyCalculation();
        
        // 测试ConvertToSolfege方法
        TestConvertToSolfege();
        
        Debug.Log("=== 音符匹配测试完成 ===");
    }
    
    void TestFrequencyCalculation()
    {
        Debug.Log("--- 测试频率计算 ---");
        
        // 测试不同调号的主音频率计算
        string[] keyNames = { "C", "G", "D", "A", "E", "B", "F#" };
        
        for (int keyValue = 0; keyValue <= 6; keyValue++)
        {
            // 设置调号
            toneGenerator.key = keyValue;
            
            // 计算主音频率（使用反射或公共方法）
            float frequency = CalculateTonicFrequency(keyValue);
            Debug.Log($"{keyNames[keyValue]}调主音频率: {frequency:F2} Hz");
        }
    }
    
    // 辅助方法：计算主音频率
    private float CalculateTonicFrequency(int keyValue)
    {
        int tonicSemitone = keyValue switch
        {
            0 => 0,  // C
            1 => 7,  // G
            2 => 2,  // D
            3 => 9,  // A
            4 => 4,  // E
            5 => 11, // B
            6 => 6,  // F#
            _ => 0
        };
        
        return 261.63f * Mathf.Pow(2f, tonicSemitone / 12f);
    }
    
    void TestConvertToSolfege()
    {
        Debug.Log("--- 测试ConvertToSolfege方法 ---");
        
        // 由于ConvertToSolfege是私有方法，我们测试其预期行为
        // 根据修复，该方法现在应该直接返回五线谱音名
        string[] testNotes = { "C4", "D4", "E4", "F4", "G4", "A4", "B4", "C5", "rest", "R" };
        
        foreach (string note in testNotes)
        {
            // 模拟ConvertToSolfege的预期行为
            string expectedResult = TestConvertToSolfegeLogic(note, 0);
            Debug.Log($"音符 {note} 预期转换结果: {expectedResult}");
        }
    }
    
    // 模拟ConvertToSolfege方法的逻辑
    private string TestConvertToSolfegeLogic(string noteName, int key)
    {
        if (string.IsNullOrEmpty(noteName))
            return "";
            
        // 检查是否为休止符
        string noteBase = noteName.ToLower();
        if (noteBase == "rest" || noteBase == "r" || noteBase == "pause" || noteBase == "0")
        {
            return "休止符";
        }
            
        // 直接返回五线谱音名，不进行简谱转换
        return noteName;
    }
}