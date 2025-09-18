// 简单的验证脚本，用于测试修改后的逻辑
using System;

public class TestVerification
{
    // 模拟GetTonicFrequency方法
    public static float GetTonicFrequency(int keyValue)
    {
        int tonicSemitone = keyValue * 7 % 12; // 五度圈计算主音半音数
        return 261.63f * (float)Math.Pow(2, tonicSemitone / 12.0); // C4为基准
    }
    
    // 模拟GetFrequencyFromSolfege方法
    public static float GetFrequencyFromSolfege(int solfegeNote, float tonicFrequency)
    {
        int baseSemitone;
        
        // 简谱音名到半音数的映射
        switch (solfegeNote % 10) // 取个位数
        {
            case 1: baseSemitone = 0; break;  // 1 (do)
            case 2: baseSemitone = 2; break;  // 2 (re)
            case 3: baseSemitone = 4; break;  // 3 (mi)
            case 4: baseSemitone = 5; break;  // 4 (fa)
            case 5: baseSemitone = 7; break;  // 5 (sol)
            case 6: baseSemitone = 9; break;  // 6 (la)
            case 7: baseSemitone = 11; break; // 7 (si)
            default: baseSemitone = 0; break;
        }
        
        int semitoneOffset;
        
        // 根据具体的简谱音名确定八度
        if (solfegeNote >= 5 && solfegeNote <= 7) // 低音区
        {
            semitoneOffset = baseSemitone - 12;
        }
        else if (solfegeNote >= 11 && solfegeNote <= 17) // 高音区
        {
            semitoneOffset = baseSemitone + 12;
        }
        else // 中音区
        {
            semitoneOffset = baseSemitone;
        }
        
        return tonicFrequency * (float)Math.Pow(2, semitoneOffset / 12.0);
    }
    
    public static void Main()
    {
        Console.WriteLine("=== 音符匹配修复验证 ===");
        
        // 测试不同调号的主音频率
        Console.WriteLine("\n--- 主音频率测试 ---");
        for (int key = 0; key <= 6; key++)
        {
            float frequency = GetTonicFrequency(key);
            string[] keyNames = { "C", "G", "D", "A", "E", "B", "F#" };
            Console.WriteLine($"{keyNames[key]}调主音频率: {frequency:F2} Hz");
        }
        
        // 测试C调下的简谱音名频率
        Console.WriteLine("\n--- C调简谱音名频率测试 ---");
        float cTonicFreq = GetTonicFrequency(0); // C调
        int[] testNotes = { 5, 6, 7, 1, 2, 3, 4, 5, 11 }; // 低音5,6,7, 中音1,2,3,4,5, 高音1
        string[] noteNames = { "低音5", "低音6", "低音7", "中音1", "中音2", "中音3", "中音4", "中音5", "高音1" };
        
        for (int i = 0; i < testNotes.Length; i++)
        {
            float freq = GetFrequencyFromSolfege(testNotes[i], cTonicFreq);
            Console.WriteLine($"{noteNames[i]}: {freq:F2} Hz");
        }
        
        // 测试G调下的简谱音名频率
        Console.WriteLine("\n--- G调简谱音名频率测试 ---");
        float gTonicFreq = GetTonicFrequency(1); // G调
        
        for (int i = 0; i < testNotes.Length; i++)
        {
            float freq = GetFrequencyFromSolfege(testNotes[i], gTonicFreq);
            Console.WriteLine($"{noteNames[i]}: {freq:F2} Hz");
        }
        
        Console.WriteLine("\n=== 验证完成 ===");
        Console.WriteLine("修改说明:");
        Console.WriteLine("1. GetBaseFrequency方法现在根据调号计算频率");
        Console.WriteLine("2. ConvertToSolfege方法现在直接返回五线谱音名");
        Console.WriteLine("3. 频率计算考虑了调号的影响");
    }
}