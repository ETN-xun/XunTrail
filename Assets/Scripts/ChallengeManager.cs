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
    public Text countdownText; // 倒计时显示文本
    public Text progressText; // 进度显示文本
    public Text upcomingNotesText; // 下三个音符显示文本
    public Button exitChallengeButton;
    
    [Header("挑战设置")]
    public float challengeDuration = 60f; // 默认挑战时长（秒）
    private float currentChallengeDuration; // 当前挑战的实际时长
    public int targetScore = 10; // 目标分数
    
    // 挑战状态
    private bool isInChallenge = false;
    private bool isCountingDown = false; // 是否在倒计时
    private float countdownStartTime; // 倒计时开始时间
    private float countdownDuration = 3f; // 倒计时时长（3秒）
    private int playedNotesCount = 0; // 已演奏的音符数量
    private int totalNotesCount = 0; // 总音符数量
    private float challengeStartTime;
    private int currentScore = 0;
    private string currentTargetNote = "";
    private List<string> noteSequence = new List<string>();
    private int currentNoteIndex = 0;
    private MusicSheet currentMusicSheet; // 当前使用的乐谱
    
    // 音符列表
    private readonly string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        // 查找CountdownText
        if (countdownText == null)
        {
            Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name == "CountdownText" && text.gameObject.scene.name != null)
                {
                    countdownText = text;
                    Debug.Log("自动找到CountdownText对象");
                    break;
                }
            }
            if (countdownText == null)
            {
                Debug.LogWarning("未找到CountdownText对象");
            }
        }
        
        // 查找ProgressText
        if (progressText == null)
        {
            Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name == "ProgressText" && text.gameObject.scene.name != null)
                {
                    progressText = text;
                    Debug.Log("自动找到ProgressText对象");
                    break;
                }
            }
            if (progressText == null)
            {
                Debug.LogWarning("未找到ProgressText对象");
            }
        }
        
        // 查找UpcomingNotesText
        if (upcomingNotesText == null)
        {
            Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name == "UpcomingNotesText" && text.gameObject.scene.name != null)
                {
                    upcomingNotesText = text;
                    Debug.Log("自动找到UpcomingNotesText对象");
                    break;
                }
            }
            if (upcomingNotesText == null)
            {
                Debug.LogWarning("未找到UpcomingNotesText对象");
            }
        }
    }
    
    private void Start()
    {
        // 自动查找UI元素
        FindUIElements();
        
        // 确保ChallengeUI被激活，这样UI元素才能正常显示
        if (challengeUI != null)
        {
            challengeUI.SetActive(true);
            Debug.Log("ChallengeUI已激活");
        }
            
        if (exitChallengeButton != null)
            exitChallengeButton.onClick.AddListener(ExitChallenge);
    }

    private void FindUIElements()
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
        
        // 查找UpcomingNotesText
        if (upcomingNotesText == null)
        {
            Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name == "UpcomingNotesText" && text.gameObject.scene.name != null)
                {
                    upcomingNotesText = text;
                    Debug.Log("自动找到UpcomingNotesText对象");
                    break;
                }
            }
            if (upcomingNotesText == null)
            {
                Debug.LogWarning("未找到UpcomingNotesText对象");
            }
        }
        
        // 查找ProgressText
        if (progressText == null)
        {
            Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name == "ProgressText" && text.gameObject.scene.name != null)
                {
                    progressText = text;
                    Debug.Log("自动找到ProgressText对象");
                    break;
                }
            }
            if (progressText == null)
            {
                Debug.LogWarning("未找到ProgressText对象");
            }
        }
        
        // 查找CountdownText
        if (countdownText == null)
        {
            Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name == "CountdownText" && text.gameObject.scene.name != null)
                {
                    countdownText = text;
                    Debug.Log("自动找到CountdownText对象");
                    break;
                }
            }
            if (countdownText == null)
            {
                Debug.LogWarning("未找到CountdownText对象");
            }
        }
        
        // 查找ExitChallengeButton
        if (exitChallengeButton == null)
        {
            Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
            foreach (Button button in allButtons)
            {
                if (button.name == "ExitChallengeButton" && button.gameObject.scene.name != null)
                {
                    exitChallengeButton = button;
                    Debug.Log("自动找到ExitChallengeButton对象");
                    break;
                }
            }
            if (exitChallengeButton == null)
            {
                Debug.LogWarning("未找到ExitChallengeButton对象");
            }
        }
    }

    private void Update()
    {
        if (isCountingDown)
        {
            UpdateCountdown();
        }
        else if (isInChallenge)
        {
            UpdateChallengeUI();
            CheckChallengeTimeout();
        }
    }
    
    public void StartChallenge(MusicSheet musicSheet)
    {
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
        
        // 设置总音符数量
        totalNotesCount = noteSequence.Count;
        playedNotesCount = 0;
        currentNoteIndex = 0;
        currentScore = 0;
        
        if (challengeUI != null)
            challengeUI.SetActive(true);
            
        // 开始倒计时
        StartCountdown();
        
        Debug.Log("挑战模式开始倒计时！");
    }

    private void StartCountdown()
    {
        isCountingDown = true;
        isInChallenge = false;
        countdownStartTime = Time.time;
        
        // 隐藏挑战UI，显示倒计时
        if (challengeUI != null)
            challengeUI.SetActive(false);
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);
            
        Debug.Log("开始3-2-1倒计时");
    }
    
    private void UpdateCountdown()
    {
        float elapsedTime = Time.time - countdownStartTime;
        float remainingTime = countdownDuration - elapsedTime;
        
        if (remainingTime > 0)
        {
            int countdownNumber = Mathf.CeilToInt(remainingTime);
            if (countdownText != null)
            {
                countdownText.text = countdownNumber.ToString();
                countdownText.fontSize = 72; // 大字体
            }
        }
        else
        {
            // 倒计时结束，开始挑战
            EndCountdown();
        }
    }
    
    private void EndCountdown()
    {
        isCountingDown = false;
        isInChallenge = true;
        challengeStartTime = Time.time;
        
        // 隐藏倒计时，显示挑战UI
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
        if (challengeUI != null)
            challengeUI.SetActive(true);
            
        Debug.Log("倒计时结束，挑战正式开始！");
    }

    public void StartChallenge()
    {
        StartChallenge(null);
    }

    public void ExitChallenge()
    {
        isInChallenge = false;
        isCountingDown = false;
        
        if (challengeUI != null)
            challengeUI.SetActive(false);
            
        // 清理UI显示
        if (countdownText != null)
            countdownText.text = "";
        if (progressText != null)
            progressText.text = "";
        if (upcomingNotesText != null)
            upcomingNotesText.text = "";
            
        Debug.Log($"挑战结束！最终得分: {currentScore}");
    }
    
    public string GetCurrentExpectedNote()
    {
        if (!isInChallenge || currentNoteIndex >= noteSequence.Count)
            return "";
            
        return noteSequence[currentNoteIndex];
    }
    
    public void OnNoteDetected(string detectedNote)
    {
        if (!isInChallenge || isCountingDown) return;
        
        string expectedNote = GetCurrentExpectedNote();
        
        if (IsCorrectNote(detectedNote, expectedNote))
        {
            currentScore++;
            Debug.Log($"正确！检测到: {detectedNote}, 期望: {expectedNote}");
        }
        else
        {
            Debug.Log($"错误！检测到: {detectedNote}, 期望: {expectedNote}");
        }
        
        currentNoteIndex++;
        playedNotesCount++;
        
        // 检查是否所有音符都演奏完毕
        if (currentNoteIndex >= noteSequence.Count)
        {
            CalculateSimilarityAndEndChallenge();
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
        
        // 显示当前进度
        if (progressText != null)
        {
            progressText.text = $"进度: {playedNotesCount}/{totalNotesCount}";
        }
        
        // 显示下三个音符
        if (upcomingNotesText != null)
        {
            string upcomingNotes = GetUpcomingNotes(3);
            upcomingNotesText.text = $"下三个音符: {upcomingNotes}";
        }
    }

    private string GetUpcomingNotes(int count)
    {
        List<string> upcoming = new List<string>();
        
        for (int i = 0; i < count && (currentNoteIndex + i) < noteSequence.Count; i++)
        {
            upcoming.Add(noteSequence[currentNoteIndex + i]);
        }
        
        return string.Join(", ", upcoming);
    }
    
    private void CalculateSimilarityAndEndChallenge()
    {
        // 计算相似度百分比
        float similarity = 0f;
        if (totalNotesCount > 0)
        {
            similarity = (float)currentScore / totalNotesCount * 100f;
        }
        
        Debug.Log($"演奏完毕！相似度: {similarity:F1}%，正确音符: {currentScore}/{totalNotesCount}");
        
        // 显示最终得分
        if (scoreText != null)
        {
            scoreText.text = $"最终得分: {similarity:F1}% ({currentScore}/{totalNotesCount})";
        }
        
        // 等待3秒后退出挑战
        Invoke("ExitChallenge", 3f);
    }

    public bool IsInChallenge()
    {
        return isInChallenge;
    }
    
    // 检查是否为正确的音符
    private bool IsCorrectNote(string currentNote, string expectedNote)
    {
        // 提取音符名称的主要部分（去掉八度数字）
        string currentNoteBase = ExtractNoteBase(currentNote);
        string expectedNoteBase = ExtractNoteBase(expectedNote);
        
        // 比较基础音符名称
        return string.Equals(currentNoteBase, expectedNoteBase, System.StringComparison.OrdinalIgnoreCase);
    }
    
    // 提取音符的基础名称（去掉八度信息）
    private string ExtractNoteBase(string noteName)
    {
        if (string.IsNullOrEmpty(noteName))
            return "";
            
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