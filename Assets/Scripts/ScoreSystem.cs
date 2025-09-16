using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [System.Serializable]
    public class NotePerformance
    {
        public string expectedNote;
        public string playedNote;
        public float timingAccuracy; // 0-1, 1为完美时机
        public float pitchAccuracy;  // 0-1, 1为完美音高
        public bool isCorrect;
        
        public NotePerformance(string expected, string played, float timing, float pitch)
        {
            expectedNote = expected;
            playedNote = played;
            timingAccuracy = timing;
            pitchAccuracy = pitch;
            isCorrect = (expected == played) && (timing > 0.7f) && (pitch > 0.8f);
        }
    }
    
    [Header("评分设置")]
    public float perfectScore = 100f;
    public float goodScore = 80f;
    public float okScore = 60f;
    public float missScore = 0f;
    
    [Header("准确度阈值")]
    public float perfectThreshold = 0.95f;
    public float goodThreshold = 0.8f;
    public float okThreshold = 0.6f;
    
    private List<NotePerformance> performances = new List<NotePerformance>();
    private int totalNotes = 0;
    private float totalScore = 0f;
    
    public void Initialize(int noteCount)
    {
        totalNotes = noteCount;
        performances.Clear();
        totalScore = 0f;
    }
    
    public void RecordNotePerformance(string expectedNote, string playedNote, float timingAccuracy, float pitchAccuracy)
    {
        NotePerformance performance = new NotePerformance(expectedNote, playedNote, timingAccuracy, pitchAccuracy);
        performances.Add(performance);
        
        // 计算这个音符的得分
        float noteScore = CalculateNoteScore(performance);
        totalScore += noteScore;
        
        Debug.Log($"音符评分: 期望={expectedNote}, 演奏={playedNote}, 时机={timingAccuracy:F2}, 音高={pitchAccuracy:F2}, 得分={noteScore:F1}");
    }
    
    public void RecordMissedNote(string expectedNote)
    {
        NotePerformance performance = new NotePerformance(expectedNote, "MISS", 0f, 0f);
        performances.Add(performance);
        totalScore += missScore;
        
        Debug.Log($"错过音符: {expectedNote}");
    }
    
    private float CalculateNoteScore(NotePerformance performance)
    {
        if (!performance.isCorrect)
        {
            return missScore;
        }
        
        // 综合时机和音高准确度
        float overallAccuracy = (performance.timingAccuracy + performance.pitchAccuracy) / 2f;
        
        if (overallAccuracy >= perfectThreshold)
        {
            return perfectScore;
        }
        else if (overallAccuracy >= goodThreshold)
        {
            return goodScore;
        }
        else if (overallAccuracy >= okThreshold)
        {
            return okScore;
        }
        else
        {
            return missScore;
        }
    }
    
    public ScoreResult GetFinalScore()
    {
        ScoreResult result = new ScoreResult();
        result.totalScore = totalScore;
        result.maxPossibleScore = totalNotes * perfectScore;
        result.percentage = (totalScore / result.maxPossibleScore) * 100f;
        result.totalNotes = totalNotes;
        result.correctNotes = 0;
        result.perfectNotes = 0;
        result.goodNotes = 0;
        result.okNotes = 0;
        result.missedNotes = 0;
        
        foreach (var performance in performances)
        {
            if (performance.isCorrect)
            {
                result.correctNotes++;
                
                float overallAccuracy = (performance.timingAccuracy + performance.pitchAccuracy) / 2f;
                if (overallAccuracy >= perfectThreshold)
                    result.perfectNotes++;
                else if (overallAccuracy >= goodThreshold)
                    result.goodNotes++;
                else
                    result.okNotes++;
            }
            else
            {
                result.missedNotes++;
            }
        }
        
        // 确定等级
        if (result.percentage >= 95f)
            result.grade = "S";
        else if (result.percentage >= 90f)
            result.grade = "A";
        else if (result.percentage >= 80f)
            result.grade = "B";
        else if (result.percentage >= 70f)
            result.grade = "C";
        else if (result.percentage >= 60f)
            result.grade = "D";
        else
            result.grade = "F";
            
        return result;
    }
    
    public List<NotePerformance> GetPerformances()
    {
        return new List<NotePerformance>(performances);
    }
}

[System.Serializable]
public class ScoreResult
{
    public float totalScore;
    public float maxPossibleScore;
    public float percentage;
    public string grade;
    public int totalNotes;
    public int correctNotes;
    public int perfectNotes;
    public int goodNotes;
    public int okNotes;
    public int missedNotes;
}