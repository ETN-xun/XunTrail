using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleScoreTest : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(TestScoreSystem());
    }

    IEnumerator TestScoreSystem()
    {
        Debug.Log("SimpleScoreTest: 开始评分系统测试");
        
        // 查找ScoreSystem组件
        ScoreSystem scoreSystem = FindObjectOfType<ScoreSystem>();
        if (scoreSystem == null)
        {
            Debug.LogError("SimpleScoreTest: 未找到ScoreSystem组件");
            yield break;
        }

        Debug.Log("SimpleScoreTest: 找到ScoreSystem，开始测试");
        
        // 创建测试数据
        int noteCount = 5;
        scoreSystem.Initialize(noteCount);
        
        // 测试1: 完美演奏
        Debug.Log("=== 测试1: 完美演奏 ===");
        for (int i = 0; i < noteCount; i++)
        {
            scoreSystem.RecordNotePerformance("C", "C", 1.0f, 1.0f);
        }
        
        ScoreResult perfectResult = scoreSystem.GetFinalScore();
        Debug.Log($"完美演奏结果:");
        Debug.Log($"  总分: {perfectResult.totalScore}");
        Debug.Log($"  百分比: {perfectResult.percentage:F1}%");
        Debug.Log($"  等级: {perfectResult.grade}");
        Debug.Log($"  完美音符: {perfectResult.perfectNotes}");
        Debug.Log($"  良好音符: {perfectResult.goodNotes}");
        Debug.Log($"  错过音符: {perfectResult.missedNotes}");
        
        yield return new WaitForSeconds(1f);
        
        // 测试2: 部分演奏
        Debug.Log("=== 测试2: 部分演奏 ===");
        scoreSystem.Initialize(noteCount);
        
        // 演奏前3个音符，错过后2个
        scoreSystem.RecordNotePerformance("C", "C", 1.0f, 1.0f);
        scoreSystem.RecordNotePerformance("D", "D", 0.8f, 0.9f);
        scoreSystem.RecordNotePerformance("E", "E", 0.6f, 0.7f);
        scoreSystem.RecordMissedNote("F");
        scoreSystem.RecordMissedNote("G");
        
        ScoreResult partialResult = scoreSystem.GetFinalScore();
        Debug.Log($"部分演奏结果:");
        Debug.Log($"  总分: {partialResult.totalScore}");
        Debug.Log($"  百分比: {partialResult.percentage:F1}%");
        Debug.Log($"  等级: {partialResult.grade}");
        Debug.Log($"  完美音符: {partialResult.perfectNotes}");
        Debug.Log($"  良好音符: {partialResult.goodNotes}");
        Debug.Log($"  错过音符: {partialResult.missedNotes}");
        
        Debug.Log("SimpleScoreTest: 评分系统测试完成");
    }
}