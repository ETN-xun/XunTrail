using UnityEngine;

public class ManualCalculationTest : MonoBehaviour
{
    void Start()
    {
        ManualTest();
    }
    
    void ManualTest()
    {
        Debug.Log("=== 手动计算验证 ===");
        
        // 标准音符频率
        float c4 = 261.63f;
        float f4 = c4 * Mathf.Pow(2f, 5f/12f); // F4 = C4 * 2^(5/12)
        float f5 = f4 * 2f; // F5 = F4 * 2
        
        Debug.Log($"C4频率: {c4:F2} Hz");
        Debug.Log($"F4频率: {f4:F2} Hz");
        Debug.Log($"F5频率: {f5:F2} Hz");
        
        // 测试1=F调号（key=5）下的计算
        int fKey = 5;
        
        // 根据GetTonicFrequency计算的F调主音频率
        float calculatedF4 = GetTonicFrequency(fKey);
        Debug.Log($"GetTonicFrequency计算的F4: {calculatedF4:F2} Hz");
        Debug.Log($"标准F4频率: {f4:F2} Hz");
        Debug.Log($"频率差异: {Mathf.Abs(calculatedF4 - f4):F2} Hz");
        
        // 测试F4在1=F调号下应该显示什么
        Debug.Log("\n=== F4在1=F调号下的计算过程 ===");
        
        float frequency = f4;
        float tonicFrequency = calculatedF4;
        
        // 计算相对于当前调号主音的半音数差
        float semitonesFromTonic = 12f * Mathf.Log(frequency / tonicFrequency, 2f);
        int semitones = Mathf.RoundToInt(semitonesFromTonic);
        
        Debug.Log($"semitonesFromTonic: {semitonesFromTonic:F3}");
        Debug.Log($"semitones: {semitones}");
        
        // 计算音符在简谱中的位置
        int noteIndex = semitones % 12;
        if (noteIndex < 0) noteIndex += 12;
        
        Debug.Log($"noteIndex: {noteIndex}");
        
        // 计算八度
        float octaveFromTonic4th = Mathf.Log(frequency / tonicFrequency, 2f);
        Debug.Log($"octaveFromTonic4th: {octaveFromTonic4th:F3}");
        
        string prefix;
        if (octaveFromTonic4th < 0)
            prefix = "低音";
        else if (octaveFromTonic4th >= 1)
            prefix = "高音";
        else
            prefix = "中音";
            
        string[] solfegeNames = { "1", "1♯", "2", "2♯", "3", "4", "4♯", "5", "5♯", "6", "6♯", "7" };
        string result = prefix + solfegeNames[noteIndex];
        
        Debug.Log($"F4在1=F调号下应该显示: {result}");
        
        // 验证期望结果
        Debug.Log($"期望结果: 中音1");
        Debug.Log($"实际结果: {result}");
        Debug.Log($"结果正确: {result == "中音1"}");
    }
    
    private static float GetTonicFrequency(int keyValue)
    {
        int tonicSemitone = keyValue switch
        {
            -4 => 8,  // A♭
            -3 => 9,  // A
            -2 => 10, // B♭
            -1 => 11, // B
            0 => 0,   // C
            1 => 1,   // D♭
            2 => 2,   // D
            3 => 3,   // E♭
            4 => 4,   // E
            5 => 5,   // F
            6 => 6,   // F♯
            7 => 7,   // G
            _ => 0    // 默认C
        };
        
        float c4Frequency = 261.63f;
        return c4Frequency * Mathf.Pow(2f, tonicSemitone / 12f);
    }
}