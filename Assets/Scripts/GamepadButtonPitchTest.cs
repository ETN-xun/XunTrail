using UnityEngine;

public class GamepadButtonPitchTest : MonoBehaviour
{
    public bool runTestOnStart = true;
    
    void Start()
    {
        if (runTestOnStart)
        {
            TestGamepadButtonPitches();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestGamepadButtonPitches();
        }
    }
    
    public void TestGamepadButtonPitches()
    {
        Debug.Log("=== 开始手柄按键音高修复测试 ===");
        
        ToneGenerator toneGenerator = FindObjectOfType<ToneGenerator>();
        if (toneGenerator == null)
        {
            Debug.LogError("未找到ToneGenerator组件！");
            return;
        }
        
        // 测试X键（应该是中音5）
        Debug.Log("\n--- 测试X键音高 ---");
        float xFrequency = TestButtonFrequency(toneGenerator, "X");
        string xNote = GetNoteFromFrequency(toneGenerator, xFrequency);
        Debug.Log($"X键频率: {xFrequency:F2} Hz -> {xNote}");
        Debug.Log($"期望: 中音5, 实际: {xNote}");
        bool xCorrect = xNote.Contains("中音5") || xNote.Contains("5");
        Debug.Log($"X键测试: {(xCorrect ? "✓ 通过" : "✗ 失败")}");
        
        // 测试Y键（应该是高音1）
        Debug.Log("\n--- 测试Y键音高 ---");
        float yFrequency = TestButtonFrequency(toneGenerator, "Y");
        string yNote = GetNoteFromFrequency(toneGenerator, yFrequency);
        Debug.Log($"Y键频率: {yFrequency:F2} Hz -> {yNote}");
        Debug.Log($"期望: 高音1, 实际: {yNote}");
        bool yCorrect = yNote.Contains("高音1") || yNote.Contains("高音") && yNote.Contains("1");
        Debug.Log($"Y键测试: {(yCorrect ? "✓ 通过" : "✗ 失败")}");
        
        // 总结
        Debug.Log("\n=== 测试结果总结 ===");
        if (xCorrect && yCorrect)
        {
            Debug.Log("✅ 手柄按键音高修复成功！X键和Y键都显示正确的音高。");
        }
        else
        {
            Debug.LogError("❌ 手柄按键音高仍有问题，需要进一步检查。");
        }
        
        Debug.Log("=== 手柄按键音高修复测试完成 ===");
    }
    
    private float TestButtonFrequency(ToneGenerator toneGenerator, string buttonName)
    {
        // 使用反射调用GetBaseFrequency方法
        var method = toneGenerator.GetType().GetMethod("GetBaseFrequency", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method != null)
        {
            if (buttonName == "X")
            {
                // 模拟X键按下
                return (float)method.Invoke(toneGenerator, new object[] { true, false, false, false, false, false, false, false, false, false, false, false });
            }
            else if (buttonName == "Y")
            {
                // 模拟Y键按下
                return (float)method.Invoke(toneGenerator, new object[] { false, true, false, false, false, false, false, false, false, false, false, false });
            }
        }
        
        Debug.LogError($"无法获取{buttonName}键的频率");
        return 440f; // 默认A4频率
    }
    
    private string GetNoteFromFrequency(ToneGenerator toneGenerator, float frequency)
    {
        // 使用反射调用GetNoteFromFrequency方法
        var method = toneGenerator.GetType().GetMethod("GetNoteFromFrequency", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method != null)
        {
            return (string)method.Invoke(toneGenerator, new object[] { frequency });
        }
        
        Debug.LogError("无法调用GetNoteFromFrequency方法");
        return "未知";
    }
}