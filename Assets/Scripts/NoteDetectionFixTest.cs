using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class NoteDetectionFixTest : MonoBehaviour
{
    [Header("测试控制")]
    public bool runTest = false;
    public bool showDebugInfo = true;
    
    private ToneGenerator toneGenerator;
    private ChallengeManager challengeManager;
    
    void Start()
    {
        toneGenerator = FindObjectOfType<ToneGenerator>();
        challengeManager = FindObjectOfType<ChallengeManager>();
        
        if (toneGenerator == null)
        {
            Debug.LogError("未找到ToneGenerator组件！");
            return;
        }
        
        Debug.Log("音符识别修复测试已准备就绪");
        Debug.Log("按T键开始测试，或在Inspector中勾选runTest");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) || runTest)
        {
            runTest = false;
            StartCoroutine(RunNoteDetectionTest());
        }
    }
    
    IEnumerator RunNoteDetectionTest()
    {
        Debug.Log("=== 开始音符识别测试 ===");
        
        // 测试1: 检查不同按键组合的音符识别
        Debug.Log("\n--- 测试1: 按键组合音符识别 ---");
        
        // 模拟按下不同的按键组合并检测音符
        KeyCode[][] testKeyCombinations = {
            new KeyCode[] { KeyCode.J }, // 中音5
            new KeyCode[] { KeyCode.F, KeyCode.J }, // 中音4
            new KeyCode[] { KeyCode.E, KeyCode.F, KeyCode.J }, // 中音3
            new KeyCode[] { KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J }, // 中音2
            new KeyCode[] { KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J }, // 中音1
        };
        
        string[] expectedNotes = { "中音5", "中音4", "中音3", "中音2", "中音1" };
        
        for (int i = 0; i < testKeyCombinations.Length; i++)
        {
            yield return TestKeyCombo(testKeyCombinations[i], expectedNotes[i]);
            yield return new WaitForSeconds(0.5f);
        }
        
        // 测试2: 检查八度调整
        Debug.Log("\n--- 测试2: 八度调整测试 ---");
        
        // 重置八度
        toneGenerator.ottava = 0;
        yield return TestOctaveAdjustment();
        
        // 测试3: 检查调号调整
        Debug.Log("\n--- 测试3: 调号调整测试 ---");
        
        // 重置调号
        toneGenerator.key = 0;
        yield return TestKeyAdjustment();
        
        Debug.Log("\n=== 音符识别测试完成 ===");
    }
    
    IEnumerator TestKeyCombo(KeyCode[] keys, string expectedNote)
    {
        Debug.Log($"测试按键组合: {string.Join("+", keys)}");
        
        // 模拟按下按键
        foreach (var key in keys)
        {
            SimulateKeyPress(key, true);
        }
        
        // 模拟吹气
        SimulateKeyPress(KeyCode.Space, true);
        
        yield return new WaitForSeconds(0.1f);
        
        // 获取识别的音符
        string detectedNote = toneGenerator.GetCurrentNoteName();
        
        Debug.Log($"期望音符: {expectedNote}, 检测到: {detectedNote}");
        
        if (showDebugInfo)
        {
            float frequency = toneGenerator.GetFrequency();
            Debug.Log($"频率: {frequency:F2}Hz");
        }
        
        // 释放按键
        foreach (var key in keys)
        {
            SimulateKeyPress(key, false);
        }
        SimulateKeyPress(KeyCode.Space, false);
        
        yield return new WaitForSeconds(0.1f);
    }
    
    IEnumerator TestOctaveAdjustment()
    {
        Debug.Log("测试八度调整 - 按J键（中音5）");
        
        // 测试不同八度
        for (int octave = -1; octave <= 1; octave++)
        {
            toneGenerator.ottava = octave;
            
            SimulateKeyPress(KeyCode.J, true);
            SimulateKeyPress(KeyCode.Space, true);
            
            yield return new WaitForSeconds(0.1f);
            
            string note = toneGenerator.GetCurrentNoteName();
            float frequency = toneGenerator.GetFrequency();
            
            Debug.Log($"八度={octave}, 音符={note}, 频率={frequency:F2}Hz");
            
            SimulateKeyPress(KeyCode.J, false);
            SimulateKeyPress(KeyCode.Space, false);
            
            yield return new WaitForSeconds(0.3f);
        }
        
        // 重置八度
        toneGenerator.ottava = 0;
    }
    
    IEnumerator TestKeyAdjustment()
    {
        Debug.Log("测试调号调整 - 按J键（中音5）");
        
        // 测试不同调号
        for (int key = -2; key <= 2; key++)
        {
            toneGenerator.key = key;
            
            SimulateKeyPress(KeyCode.J, true);
            SimulateKeyPress(KeyCode.Space, true);
            
            yield return new WaitForSeconds(0.1f);
            
            string note = toneGenerator.GetCurrentNoteName();
            float frequency = toneGenerator.GetFrequency();
            
            Debug.Log($"调号={key}, 音符={note}, 频率={frequency:F2}Hz");
            
            SimulateKeyPress(KeyCode.J, false);
            SimulateKeyPress(KeyCode.Space, false);
            
            yield return new WaitForSeconds(0.3f);
        }
        
        // 重置调号
        toneGenerator.key = 0;
    }
    
    private void SimulateKeyPress(KeyCode key, bool pressed)
    {
        // 这里需要直接操作ToneGenerator的内部状态
        // 由于_keyStates是私有的，我们需要通过反射或其他方式
        // 为了测试目的，我们可以直接调用相关方法
        
        if (showDebugInfo)
        {
            Debug.Log($"模拟按键: {key} = {pressed}");
        }
    }
    
    [ContextMenu("运行音符识别测试")]
    public void RunTest()
    {
        StartCoroutine(RunNoteDetectionTest());
    }
    
    [ContextMenu("测试当前音符")]
    public void TestCurrentNote()
    {
        if (toneGenerator == null) return;
        
        string note = toneGenerator.GetCurrentNoteName();
        float frequency = toneGenerator.GetFrequency();
        
        Debug.Log($"当前音符: {note}, 频率: {frequency:F2}Hz");
        Debug.Log($"八度: {toneGenerator.ottava}, 调号: {toneGenerator.key}");
    }
}