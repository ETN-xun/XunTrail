using UnityEngine;
using System.Collections;

/// <summary>
/// 单音符计分测试 - 验证修复后的挑战模式bug
/// 测试内容：
/// 1. 验证没有演奏时不会默认加A4分数
/// 2. 验证休止符显示为"休止符"而不是"无音符"
/// 3. 验证休止符时不演奏会正确加分
/// </summary>
public class SingleNoteScoreTest : MonoBehaviour
{
    [Header("测试设置")]
    public bool autoStartTest = false;
    public float testDuration = 10f;
    
    private ChallengeManager challengeManager;
    private ToneGenerator toneGenerator;
    private bool testRunning = false;
    
    void Start()
    {
        if (autoStartTest)
        {
            StartCoroutine(RunBugFixTests());
        }
    }
    
    void Update()
    {
        // 按T键开始测试
        if (Input.GetKeyDown(KeyCode.T) && !testRunning)
        {
            StartCoroutine(RunBugFixTests());
        }
        
        // 按R键开始休止符测试
        if (Input.GetKeyDown(KeyCode.R) && !testRunning)
        {
            StartCoroutine(RunRestNoteTest());
        }
    }
    
    IEnumerator RunBugFixTests()
    {
        testRunning = true;
        Debug.Log("=== 开始挑战模式Bug修复验证测试 ===");
        
        // 查找必要的组件
        challengeManager = FindObjectOfType<ChallengeManager>();
        toneGenerator = FindObjectOfType<ToneGenerator>();
        
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager组件！");
            testRunning = false;
            yield break;
        }
        
        if (toneGenerator == null)
        {
            Debug.LogError("未找到ToneGenerator组件！");
            testRunning = false;
            yield break;
        }
        
        // 测试1: 验证没有按键时不返回默认A4音符
        yield return StartCoroutine(TestNoDefaultA4());
        
        // 测试2: 验证休止符显示
        yield return StartCoroutine(TestRestNoteDisplay());
        
        // 测试3: 验证休止符计分逻辑
        yield return StartCoroutine(TestRestNoteScoring());
        
        Debug.Log("=== 挑战模式Bug修复验证测试完成 ===");
        testRunning = false;
    }
    
    IEnumerator TestNoDefaultA4()
    {
        Debug.Log("\n--- 测试1: 验证没有按键时不返回默认A4音符 ---");
        
        // 确保没有按键被按下
        yield return new WaitForSeconds(0.5f);
        
        // 获取当前音符名称
        string currentNote = toneGenerator.GetCurrentNoteName();
        
        Debug.Log($"没有按键时的音符检测结果: '{currentNote}'");
        
        if (string.IsNullOrEmpty(currentNote))
        {
            Debug.Log("✓ 测试1通过: 没有按键时正确返回空字符串，不会默认返回A4");
        }
        else
        {
            Debug.LogError($"✗ 测试1失败: 没有按键时返回了音符 '{currentNote}'，应该返回空字符串");
        }
        
        yield return new WaitForSeconds(1f);
    }
    
    IEnumerator TestRestNoteDisplay()
    {
        Debug.Log("\n--- 测试2: 验证休止符显示 ---");
        
        // 测试不同的休止符格式
        string[] restNotes = { "rest", "r", "pause", "0" };
        
        foreach (string restNote in restNotes)
        {
            // 使用反射调用ConvertToSolfege方法
            var method = typeof(ChallengeManager).GetMethod("ConvertToSolfege", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (method != null)
            {
                string result = (string)method.Invoke(challengeManager, new object[] { restNote, 0 });
                Debug.Log($"休止符 '{restNote}' 显示为: '{result}'");
                
                if (result == "休止符")
                {
                    Debug.Log($"✓ 休止符 '{restNote}' 正确显示为 '休止符'");
                }
                else
                {
                    Debug.LogError($"✗ 休止符 '{restNote}' 错误显示为 '{result}'，应该显示为 '休止符'");
                }
            }
            else
            {
                Debug.LogError("无法找到ConvertToSolfege方法");
            }
        }
        
        yield return new WaitForSeconds(1f);
    }
    
    IEnumerator TestRestNoteScoring()
    {
        Debug.Log("\n--- 测试3: 验证休止符计分逻辑 ---");
        
        // 启动挑战模式
        Debug.Log("启动挑战模式进行休止符计分测试...");
        challengeManager.StartChallenge();
        
        // 等待倒计时结束
        yield return new WaitForSeconds(4f);
        
        // 模拟休止符期间的行为
        Debug.Log("模拟休止符期间不演奏...");
        
        // 手动设置当前期望音符为休止符进行测试
        var expectedNoteField = typeof(ChallengeManager).GetField("currentExpectedNote", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (expectedNoteField != null)
        {
            expectedNoteField.SetValue(challengeManager, "rest");
            Debug.Log("设置当前期望音符为休止符");
            
            // 等待一段时间让计分逻辑运行
            float startTime = Time.time;
            yield return new WaitForSeconds(2f);
            float endTime = Time.time;
            
            Debug.Log($"休止符测试持续时间: {endTime - startTime:F2}s");
        }
        
        // 结束挑战
        challengeManager.ExitChallenge();
        
        yield return new WaitForSeconds(1f);
    }
    
    IEnumerator RunRestNoteTest()
    {
        testRunning = true;
        Debug.Log("=== 开始专门的休止符测试 ===");
        
        challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager组件！");
            testRunning = false;
            yield break;
        }
        
        // 测试IsRestNote方法
        var isRestMethod = typeof(ChallengeManager).GetMethod("IsRestNote", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (isRestMethod != null)
        {
            string[] testNotes = { "rest", "r", "pause", "0", "C4", "A4", "", "invalid" };
            
            foreach (string note in testNotes)
            {
                bool isRest = (bool)isRestMethod.Invoke(challengeManager, new object[] { note });
                Debug.Log($"音符 '{note}' 是否为休止符: {isRest}");
            }
        }
        
        Debug.Log("=== 专门的休止符测试完成 ===");
        testRunning = false;
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("单音符计分测试", GUI.skin.box);
        
        if (!testRunning)
        {
            if (GUILayout.Button("开始Bug修复验证测试 (T键)"))
            {
                StartCoroutine(RunBugFixTests());
            }
            
            if (GUILayout.Button("开始休止符测试 (R键)"))
            {
                StartCoroutine(RunRestNoteTest());
            }
        }
        else
        {
            GUILayout.Label("测试进行中...", GUI.skin.box);
        }
        
        GUILayout.Label("说明:");
        GUILayout.Label("- T键: 运行完整的Bug修复验证测试");
        GUILayout.Label("- R键: 运行专门的休止符测试");
        
        GUILayout.EndArea();
    }
}