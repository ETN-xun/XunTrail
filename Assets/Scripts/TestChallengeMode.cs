using UnityEngine;
using UnityEngine.UI;

public class TestChallengeMode : MonoBehaviour
{
    [Header("测试UI")]
    public Button startTestButton;
    public Button simulateNoteButton;
    public Text statusText;
    public Text logText;
    
    [Header("测试设置")]
    public ChallengeManager challengeManager;
    public float testDuration = 30f;
    
    private bool isTestRunning = false;
    private string[] testNotes = { "C4", "D4", "E4", "F4", "G4", "A4", "B4" };
    private int currentTestNoteIndex = 0;
    
    private void Start()
    {
        if (startTestButton != null)
            startTestButton.onClick.AddListener(StartTest);
            
        if (simulateNoteButton != null)
            simulateNoteButton.onClick.AddListener(SimulateRandomNote);
            
        if (challengeManager == null)
            challengeManager = FindObjectOfType<ChallengeManager>();
            
        UpdateStatus("测试准备就绪");
    }
    
    public void StartTest()
    {
        if (isTestRunning)
        {
            UpdateStatus("测试已在运行中");
            return;
        }
        
        if (challengeManager == null)
        {
            UpdateStatus("错误：未找到ChallengeManager");
            return;
        }
        
        isTestRunning = true;
        UpdateStatus("开始测试挑战模式...");
        
        // 启动挑战
        challengeManager.StartChallenge();
        
        // 开始自动模拟音符演奏
        InvokeRepeating("AutoSimulateNote", 2f, 0.8f);
        
        // 设置测试结束时间
        Invoke("EndTest", testDuration);
    }
    
    public void SimulateRandomNote()
    {
        if (challengeManager == null) return;
        
        string randomNote = testNotes[Random.Range(0, testNotes.Length)];
        challengeManager.OnNoteDetected(randomNote);
        
        AddLog($"模拟演奏: {randomNote}");
    }
    
    private void AutoSimulateNote()
    {
        if (!isTestRunning || challengeManager == null) return;
        
        // 循环使用测试音符
        string note = testNotes[currentTestNoteIndex];
        currentTestNoteIndex = (currentTestNoteIndex + 1) % testNotes.Length;
        
        challengeManager.OnNoteDetected(note);
        AddLog($"自动演奏: {note}");
    }
    
    private void EndTest()
    {
        isTestRunning = false;
        CancelInvoke("AutoSimulateNote");
        
        if (challengeManager != null)
        {
            challengeManager.ExitChallenge();
        }
        
        UpdateStatus("测试完成");
        AddLog("=== 测试结束 ===");
    }
    
    private void UpdateStatus(string status)
    {
        if (statusText != null)
            statusText.text = $"状态: {status}";
            
        Debug.Log($"[TestChallengeMode] {status}");
    }
    
    private void AddLog(string message)
    {
        if (logText != null)
        {
            logText.text += $"\n{System.DateTime.Now:HH:mm:ss} - {message}";
            
            // 限制日志长度
            string[] lines = logText.text.Split('\n');
            if (lines.Length > 10)
            {
                string newLog = "";
                for (int i = lines.Length - 10; i < lines.Length; i++)
                {
                    newLog += lines[i] + "\n";
                }
                logText.text = newLog;
            }
        }
        
        Debug.Log($"[TestChallengeMode] {message}");
    }
    
    private void OnDestroy()
    {
        CancelInvoke();
    }
}