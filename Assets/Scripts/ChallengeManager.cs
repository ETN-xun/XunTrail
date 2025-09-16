using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeManager : MonoBehaviour
{
    public static ChallengeManager Instance { get; private set; }
    
    [Header("挑战模式UI")]
    public GameObject challengeUI;
    public Text scoreText;
    public Text targetNoteText;
    public Text timeText;
    public Button exitChallengeButton;
    
    [Header("挑战设置")]
    public float challengeDuration = 60f; // 默认挑战时长（秒）
    private float currentChallengeDuration; // 当前挑战的实际时长
    public int targetScore = 10; // 目标分数
    
    // 挑战状态
    private bool isInChallenge = false;
    private float challengeStartTime;
    private int currentScore = 0;
    private string currentTargetNote = "";
    private List<string> noteSequence = new List<string>();
    private int currentNoteIndex = 0;
    private MusicSheet currentMusicSheet; // 当前使用的乐谱
    
    // 音符列表
    private readonly string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
void Start()
    {
        // 自动查找UI元素
        FindUIElements();
        
        if (challengeUI != null)
            challengeUI.SetActive(false);
            
        if (exitChallengeButton != null)
            exitChallengeButton.onClick.AddListener(ExitChallenge);
    }

void FindUIElements()
    {
        // 查找ChallengeUI对象（包括非激活的）
        if (challengeUI == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "ChallengeUI" && obj.scene.name != null)
                {
                    challengeUI = obj;
                    Debug.Log("自动找到ChallengeUI对象");
                    break;
                }
            }
            if (challengeUI == null)
            {
                Debug.LogWarning("未找到ChallengeUI对象");
            }
        }
        
        // 查找ScoreText
        if (scoreText == null)
        {
            Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name == "ScoreText" && text.gameObject.scene.name != null)
                {
                    scoreText = text;
                    Debug.Log("自动找到ScoreText对象");
                    break;
                }
            }
            if (scoreText == null)
            {
                Debug.LogWarning("未找到ScoreText对象");
            }
        }
        
        // 查找UpcomingNotesText作为targetNoteText
        if (targetNoteText == null)
        {
            Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name == "UpcomingNotesText" && text.gameObject.scene.name != null)
                {
                    targetNoteText = text;
                    Debug.Log("自动找到UpcomingNotesText对象作为targetNoteText");
                    break;
                }
            }
            if (targetNoteText == null)
            {
                Debug.LogWarning("未找到UpcomingNotesText对象");
            }
        }
        
        // 查找ProgressText作为timeText
        if (timeText == null)
        {
            Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name == "ProgressText" && text.gameObject.scene.name != null)
                {
                    timeText = text;
                    Debug.Log("自动找到ProgressText对象作为timeText");
                    break;
                }
            }
            if (timeText == null)
            {
                Debug.LogWarning("未找到ProgressText对象");
            }
        }
    }

    
    void Update()
    {
        if (isInChallenge)
        {
            UpdateChallengeUI();
            CheckChallengeTimeout();
        }
    }
    
    public void StartChallenge(MusicSheet musicSheet)
    {
        isInChallenge = true;
        challengeStartTime = Time.time;
        currentScore = 0;
        currentNoteIndex = 0;
        currentMusicSheet = musicSheet;
        
        // 根据乐谱设置挑战时长
        if (musicSheet != null && musicSheet.totalDuration > 0)
        {
            currentChallengeDuration = musicSheet.totalDuration + 5f; // 额外给5秒缓冲时间
            Debug.Log($"使用乐谱时长: {musicSheet.totalDuration:F2}秒，挑战时长: {currentChallengeDuration:F2}秒");
        }
        else
        {
            currentChallengeDuration = challengeDuration; // 使用默认时长
            Debug.Log($"使用默认挑战时长: {currentChallengeDuration:F2}秒");
        }
        
        // 如果提供了乐谱，使用乐谱中的音符；否则生成随机音符
        if (musicSheet != null && musicSheet.notes != null && musicSheet.notes.Count > 0)
        {
            GenerateNoteSequenceFromSheet(musicSheet);
        }
        else
        {
            GenerateNoteSequence();
        }
        
        if (challengeUI != null)
            challengeUI.SetActive(true);
            
        Debug.Log("挑战模式开始！");
    }
    
    public void StartChallenge()
    {
        StartChallenge(null);
    }

    public void ExitChallenge()
    {
        isInChallenge = false;
        
        if (challengeUI != null)
            challengeUI.SetActive(false);
            
        Debug.Log($"挑战结束！最终得分: {currentScore}");
    }
    
    public bool IsInChallenge()
    {
        return isInChallenge;
    }
    
    public string GetCurrentExpectedNote()
    {
        if (!isInChallenge || currentNoteIndex >= noteSequence.Count)
            return "";
            
        return noteSequence[currentNoteIndex];
    }
    
    public void OnNoteDetected(string detectedNote)
    {
        if (!isInChallenge)
            return;
            
        string expectedNote = GetCurrentExpectedNote();
        if (detectedNote == expectedNote)
        {
            currentScore++;
            currentNoteIndex++;
            
            Debug.Log($"正确！得分: {currentScore}");
            
            // 检查是否完成所有音符
            if (currentNoteIndex >= noteSequence.Count)
            {
                GenerateNoteSequence(); // 生成新的音符序列
                currentNoteIndex = 0;
            }
            
            // 检查是否达到目标分数
            if (currentScore >= targetScore)
            {
                Debug.Log("挑战完成！");
                ExitChallenge();
            }
        }
    }
    
    private void GenerateNoteSequence()
    {
        noteSequence.Clear();
        
        // 生成5个随机音符
        for (int i = 0; i < 5; i++)
        {
            string randomNote = notes[Random.Range(0, notes.Length)];
            noteSequence.Add(randomNote);
        }
        
        Debug.Log($"新的音符序列: {string.Join(", ", noteSequence)}");
    }

private void GenerateNoteSequenceFromSheet(MusicSheet musicSheet)
    {
        noteSequence.Clear();
        
        // 从乐谱中提取前10个音符作为挑战序列
        int maxNotes = Mathf.Min(10, musicSheet.notes.Count);
        for (int i = 0; i < maxNotes; i++)
        {
            var note = musicSheet.notes[i];
            // 将音符转换为简单的字符串格式
            string noteString = ConvertNoteToString(note);
            if (!string.IsNullOrEmpty(noteString))
            {
                noteSequence.Add(noteString);
            }
        }
        
        // 如果乐谱音符不够，用随机音符填充
        while (noteSequence.Count < 5)
        {
            string randomNote = notes[Random.Range(0, notes.Length)];
            noteSequence.Add(randomNote);
        }
        
        Debug.Log($"从乐谱生成的音符序列: {string.Join(", ", noteSequence)}");
    }
    
private string ConvertNoteToString(Note note)
    {
        // 将Note对象转换为简单的音符字符串
        if (note == null || note.isRest || string.IsNullOrEmpty(note.noteName))
            return "";
            
        // 提取音符名称的主要部分（去掉八度数字）
        string noteName = note.noteName;
        
        // 如果包含数字（八度），只保留字母部分
        string result = "";
        foreach (char c in noteName)
        {
            if (char.IsLetter(c) || c == '#')
            {
                result += c;
            }
            else
            {
                break; // 遇到数字就停止
            }
        }
        
        return result;
    }

    
private void UpdateChallengeUI()
    {
        if (scoreText != null)
            scoreText.text = $"得分: {currentScore}/{targetScore}";
            
        if (targetNoteText != null)
        {
            currentTargetNote = GetCurrentExpectedNote();
            targetNoteText.text = $"目标音符: {currentTargetNote}";
        }
        
        if (timeText != null)
        {
            float remainingTime = currentChallengeDuration - (Time.time - challengeStartTime);
            remainingTime = Mathf.Max(0, remainingTime);
            timeText.text = $"剩余时间: {remainingTime:F1}s";
        }
    }
    
private void CheckChallengeTimeout()
    {
        float elapsedTime = Time.time - challengeStartTime;
        if (elapsedTime >= currentChallengeDuration)
        {
            Debug.Log($"挑战超时！用时: {elapsedTime:F2}秒，限制: {currentChallengeDuration:F2}秒");
            ExitChallenge();
        }
    }
}