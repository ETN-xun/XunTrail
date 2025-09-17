using UnityEngine;

public class ScoreTestRunner : MonoBehaviour
{
    void Start()
    {
        TestScoreSystem();
    }

    void TestScoreSystem()
    {
        Debug.Log("=== 开始评分系统测试 ===");
        
        ScoreSystem scoreSystem = FindObjectOfType<ScoreSystem>();
        if (scoreSystem == null)
        {
            Debug.LogError("未找到ScoreSystem组件");
            return;
        }

        Debug.Log("找到ScoreSystem组件，开始测试...");
        
        // 测试1: 初始化
        Debug.Log("测试1: 初始化评分系统");
        scoreSystem.Initialize(5);
        Debug.Log("初始化完成");
        
        // 测试2: 记录完美演奏
        Debug.Log("测试2: 记录完美演奏");
        scoreSystem.RecordNotePerformance("C", "C", 1.0f, 1.0f);
        scoreSystem.RecordNotePerformance("D", "D", 1.0f, 1.0f);
        scoreSystem.RecordNotePerformance("E", "E", 1.0f, 1.0f);
        Debug.Log("记录了3个完美音符");
        
        // 测试3: 记录错过的音符
        Debug.Log("测试3: 记录错过的音符");
        scoreSystem.RecordMissedNote("F");
        scoreSystem.RecordMissedNote("G");
        Debug.Log("记录了2个错过的音符");
        
        // 测试4: 获取最终得分
        Debug.Log("测试4: 获取最终得分");
        ScoreResult result = scoreSystem.GetFinalScore();
        
        Debug.Log($"=== 评分结果 ===");
        Debug.Log($"总分: {result.totalScore}");
        Debug.Log($"百分比: {result.percentage:F1}%");
        Debug.Log($"等级: {result.grade}");
        Debug.Log($"完美音符: {result.perfectNotes}");
        Debug.Log($"良好音符: {result.goodNotes}");
        Debug.Log($"一般音符: {result.okNotes}");
        Debug.Log($"错过音符: {result.missedNotes}");
        Debug.Log($"总音符数: {result.totalNotes}");
        
        Debug.Log("=== 评分系统测试完成 ===");
    }
}