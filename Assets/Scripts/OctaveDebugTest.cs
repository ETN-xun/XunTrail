using UnityEngine;

public class OctaveDebugTest : MonoBehaviour
{
    void Start()
    {
        TestOctaveDisplay();
    }
    
    void TestOctaveDisplay()
    {
        Debug.Log("=== 八度显示调试测试 ===");
        
        // 测试1=F调号（key=5）
        int fKey = 5;
        
        // 计算F调主音频率（F4）
        float fTonicFreq = GetTonicFrequency(fKey);
        Debug.Log($"F调主音频率（F4）: {fTonicFreq:F2} Hz");
        
        // 测试不同八度的F音符
        float f3 = fTonicFreq / 2f;  // F3
        float f4 = fTonicFreq;       // F4
        float f5 = fTonicFreq * 2f;  // F5
        
        Debug.Log($"F3频率: {f3:F2} Hz -> {TestFrequencyToSolfege(f3, fKey)}");
        Debug.Log($"F4频率: {f4:F2} Hz -> {TestFrequencyToSolfege(f4, fKey)}");
        Debug.Log($"F5频率: {f5:F2} Hz -> {TestFrequencyToSolfege(f5, fKey)}");
        
        // 测试F4附近的其他音符
        float f4Sharp = f4 * Mathf.Pow(2f, 1f/12f);  // F#4
        float g4 = f4 * Mathf.Pow(2f, 2f/12f);       // G4
        
        Debug.Log($"F#4频率: {f4Sharp:F2} Hz -> {TestFrequencyToSolfege(f4Sharp, fKey)}");
        Debug.Log($"G4频率: {g4:F2} Hz -> {TestFrequencyToSolfege(g4, fKey)}");
        
        // 测试八度计算逻辑
        float octaveFromTonic4th_F4 = Mathf.Log(f4 / fTonicFreq, 2f);
        float octaveFromTonic4th_F5 = Mathf.Log(f5 / fTonicFreq, 2f);
        
        Debug.Log($"F4相对于F4的八度数: {octaveFromTonic4th_F4:F3}");
        Debug.Log($"F5相对于F4的八度数: {octaveFromTonic4th_F5:F3}");
        
        // 验证标准音符频率
        Debug.Log("=== 标准音符频率参考 ===");
        Debug.Log($"C4标准频率: 261.63 Hz");
        Debug.Log($"F4标准频率: {261.63f * Mathf.Pow(2f, 5f/12f):F2} Hz");
        Debug.Log($"F5标准频率: {261.63f * Mathf.Pow(2f, 17f/12f):F2} Hz");
    }
    
    // 复制并调试FrequencyToSolfege方法
    string TestFrequencyToSolfege(float frequency, int key)
    {
        if (frequency <= 0f) return "";
        
        // 根据当前调号获取主音的频率（第4八度）
        float tonicFrequency = GetTonicFrequency(key);
        
        // 计算相对于当前调号主音的半音数差
        float semitonesFromTonic = 12f * Mathf.Log(frequency / tonicFrequency, 2f);
        int semitones = Mathf.RoundToInt(semitonesFromTonic);
        
        Debug.Log($"频率 {frequency:F2} Hz: semitonesFromTonic={semitonesFromTonic:F2}, semitones={semitones}");
        
        // 计算音符在简谱中的位置（1-7）
        int noteIndex = semitones % 12;
        if (noteIndex < 0) noteIndex += 12;
        
        Debug.Log($"noteIndex = {noteIndex}");
        
        // 简谱音符映射（相对于调号主音）
        string[] solfegeNames = { "1", "1♯", "2", "2♯", "3", "4", "4♯", "5", "5♯", "6", "6♯", "7" };
        
        // 计算当前频率相对于主音第4八度的八度数
        float octaveFromTonic4th = Mathf.Log(frequency / tonicFrequency, 2f);
        
        Debug.Log($"octaveFromTonic4th = {octaveFromTonic4th:F3}");
        
        string prefix;
        if (octaveFromTonic4th < 0) // 低于第4八度
        {
            prefix = "低音";
        }
        else if (octaveFromTonic4th >= 1) // 第5八度及以上
        {
            prefix = "高音";
        }
        else // 第4八度（包含第4八度的所有音符）
        {
            prefix = "中音";
        }
        
        string result = prefix + solfegeNames[noteIndex];
        Debug.Log($"最终结果: {result}");
        return result;
    }
    
    // 复制GetTonicFrequency方法用于测试
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