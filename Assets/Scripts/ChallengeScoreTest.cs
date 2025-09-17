using System.Collections;
using UnityEngine;

public class ChallengeScoreTest : MonoBehaviour
{
    [Header("测试设置")]
    public bool runTestOnStart = true;
    public bool showDebugInfo = true;
    
    private ChallengeManager challengeManager;
    
    void Start()
    {
        if (runTestOnStart)
        {
            StartCoroutine(RunScoreTest());
        }
    }
    
    IEnumerator RunScoreTest()
    {
        Debug.Log("=== 开始挑战模式评分测试 ===");
        
        // 查找ChallengeManager
        challengeManager = ChallengeManager.Instance;
        if (challengeManager == null)
        {
            challengeManager = FindObjectOfType<ChallengeManager>();
        }
        
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager，测试失败！");
            yield break;
        }
        
        Debug.Log("✓ 找到ChallengeManager");
        
        // 启动挑战模式
        Debug.Log("启动挑战模式...");
        challengeManager.StartChallenge();
        
        // 等待倒计时结束
        yield return new WaitForSeconds(4f);
        
        // 模拟演奏一些音符
        Debug.Log("开始模拟演奏...");
        
        // 模拟演奏正确的音符
        yield return new WaitForSeconds(0.5f);
        challengeManager.OnNoteDetected("C4");
        Debug.Log("模拟演奏: C4");
        
        yield return new WaitForSeconds(0.5f);
        challengeManager.OnNoteDetected("D4");
        Debug.Log("模拟演奏: D4");
        
        yield return new WaitForSeconds(0.5f);
        challengeManager.OnNoteDetected("E4");
        Debug.Log("模拟演奏: E4");
        
        // 模拟演奏错误的音符
        yield return new WaitForSeconds(0.5f);
        challengeManager.OnNoteDetected("F#4");
        Debug.Log("模拟演奏: F#4 (可能是错误的)");
        
        yield return new WaitForSeconds(0.5f);
        challengeManager.OnNoteDetected("G4");
        Debug.Log("模拟演奏: G4");
        
        // 等待一段时间让挑战继续
        yield return new WaitForSeconds(2f);
        
        // 手动结束挑战并查看得分
        Debug.Log("结束挑战并计算得分...");
        challengeManager.ExitChallenge();
        
        Debug.Log("=== 挑战模式评分测试完成 ===");
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("挑战模式评分测试", GUI.skin.box);
        
        if (challengeManager != null)
        {
            bool isInChallenge = challengeManager.IsInChallenge();
            GUILayout.Label($"挑战状态: {(isInChallenge ? "进行中" : "未开始")}");
        }
        else
        {
            GUILayout.Label("ChallengeManager: 未找到");
        }
        
        if (GUILayout.Button("开始测试"))
        {
            StartCoroutine(RunScoreTest());
        }
        
        if (GUILayout.Button("停止挑战"))
        {
            if (challengeManager != null)
            {
                challengeManager.ExitChallenge();
            }
        }
        
        GUILayout.EndArea();
    }
}