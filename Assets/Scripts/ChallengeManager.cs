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
    public Slider progressSlider; // 进度条
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
        
    // 玩家演奏记录 - 按时间记录
    private List<PlayerNote> playerPerformance = new List<PlayerNote>();
    private float challengeStartRealTime; // 挑战实际开始时间
    
    // 新增：基于时间的挑战逻辑
    private List<TimedNote> timedNoteSequence = new List<TimedNote>(); // 带时间信息的音符序列
    private float currentChallengeTime = 0f; // 当前挑战进行时间
    private bool challengeCompleted = false; // 挑战是否完成
    
    // 实时音符检测
    private string lastDetectedNote = "";
    private float lastNoteDetectionTime = 0f;
    
    // SampleSceneManager引用
    private SampleSceneManager sampleSceneManager;
    private const float NOTE_DETECTION_INTERVAL = 0.1f; // 每0.1秒检测一次音符
    
    // 新的基于时长的积分系统
    private float totalMusicDuration = 0f; // 乐谱总时长
    private float correctPlayTime = 0f; // 正确演奏的累计时长
    private string currentExpectedNote = ""; // 当前应该演奏的音符
    
    [System.Serializable]
    public class PlayerNote
    {
        public string noteName;
        public float timestamp; // 相对于挑战开始的时间
        public float duration;
        
        public PlayerNote(string note, float time, float dur = 0.1f)
        {
            noteName = note;
            timestamp = time;
            duration = dur;
        }
    }
    
    [System.Serializable]
    public class TimedNote
    {
        public string noteName;
        public float startTime; // 音符开始时间（秒）
        public float duration; // 音符持续时间（秒）
        public float endTime; // 音符结束时间（秒）
        
        public TimedNote(string note, float start, float dur)
        {
            noteName = note;
            startTime = start;
            duration = dur;
            endTime = start + dur;
        }
    }
private MusicSheet currentMusicSheet; // 当前使用的乐谱
    
    // 音符列表
    private readonly string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    
    // ToneGenerator引用，用于获取当前调号
    private ToneGenerator toneGenerator;
    
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
        
        // 查找ToneGenerator引用
        if (toneGenerator == null)
        {
            toneGenerator = FindObjectOfType<ToneGenerator>();
            if (toneGenerator != null)
            {
                Debug.Log("自动找到ToneGenerator对象");
            }
            else
            {
                Debug.LogWarning("未找到ToneGenerator对象");
            }
        }
        
        // 查找SampleSceneManager引用
        if (sampleSceneManager == null)
        {
            sampleSceneManager = FindObjectOfType<SampleSceneManager>();
            if (sampleSceneManager != null)
            {
                Debug.Log("自动找到SampleSceneManager对象");
                // 如果scoreText为null，尝试从SampleSceneManager获取
                if (scoreText == null)
                {
                    scoreText = sampleSceneManager.GetScoreText();
                    if (scoreText != null)
                    {
                        Debug.Log("从SampleSceneManager获取到ScoreText引用");
                    }
                }
            }
            else
            {
                Debug.LogWarning("未找到SampleSceneManager对象");
            }
        }
        
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
            Debug.Log($"正在搜索ScoreText，找到 {allTexts.Length} 个Text组件");
            foreach (Text text in allTexts)
            {
                Debug.Log($"检查Text组件: {text.name}, 场景: {text.gameObject.scene.name}");
                if (text.name == "ScoreText" && text.gameObject.scene.name != null)
                {
                    scoreText = text;
                    Debug.Log($"自动找到ScoreText对象: {text.name}, 当前文本: {text.text}");
                    break;
                }
            }
            if (scoreText == null)
            {
                Debug.LogWarning("未找到ScoreText对象");
            }
        }
        else
        {
            Debug.Log($"ScoreText已存在: {scoreText.name}, 当前文本: {scoreText.text}");
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
        
        // 查找ProgressSlider
        if (progressSlider == null)
        {
            Slider[] allSliders = Resources.FindObjectsOfTypeAll<Slider>();
            foreach (Slider slider in allSliders)
            {
                if (slider.name == "ProgressSlider" && slider.gameObject.scene.name != null)
                {
                    progressSlider = slider;
                    Debug.Log("自动找到ProgressSlider对象");
                    break;
                }
            }
            if (progressSlider == null)
            {
                Debug.LogWarning("未找到ProgressSlider对象");
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
            UpdateChallengeTime();
            ProcessRealtimeNoteDetection();
            UpdateChallengeUI();
            CheckChallengeCompletion();
        }
    }
    
    private void UpdateChallengeTime()
    {
        currentChallengeTime = Time.time - challengeStartRealTime;
        
        // 更新当前期望的音符
        UpdateCurrentExpectedNote();
        
        // 累计正确演奏时长
        UpdateCorrectPlayTime();
        
        // 检查挑战是否完成
        if (!challengeCompleted && currentChallengeTime >= currentChallengeDuration)
        {
            challengeCompleted = true;
            CalculateFinalScore();
        }
    }
    
public void StartChallenge(MusicSheet musicSheet)
    {
        currentMusicSheet = musicSheet;
        
        // 验证UI元素是否正确初始化
        if (scoreText == null)
        {
            Debug.LogWarning("挑战开始时scoreText为null，尝试重新查找");
            FindUIElements(); // 重新查找UI元素
        }
        
        if (scoreText != null)
        {
            Debug.Log($"挑战开始时scoreText状态正常: {scoreText.name}");
        }
        else
        {
            Debug.LogError("挑战开始时scoreText仍然为null！");
        }
        
        // 根据乐谱设置挑战时长
        if (musicSheet != null && musicSheet.totalDuration > 0)
        {
            currentChallengeDuration = musicSheet.totalDuration;
            Debug.Log($"使用乐谱时长: {musicSheet.totalDuration:F2}秒");
        }
        else
        {
            currentChallengeDuration = challengeDuration; // 使用默认时长
            Debug.Log($"使用默认挑战时长: {currentChallengeDuration:F2}秒");
        }
        
        // 生成带时间信息的音符序列
        if (musicSheet != null && musicSheet.notes != null && musicSheet.notes.Count > 0)
        {
            GenerateTimedNoteSequenceFromSheet(musicSheet);
        }
        else
        {
            GenerateTimedNoteSequence();
        }
        
        // 重置挑战状态
        totalNotesCount = timedNoteSequence.Count;
        playedNotesCount = 0;
        currentNoteIndex = 0;
        currentScore = 0;
        currentChallengeTime = 0f;
        challengeCompleted = false;
        
        // 初始化新的积分系统
        totalMusicDuration = currentChallengeDuration;
        correctPlayTime = 0f;
        currentExpectedNote = "";
        Debug.Log($"初始化新积分系统: 总时长={totalMusicDuration:F2}秒, 正确时长=0秒");
        
        // 初始化玩家演奏记录
        playerPerformance.Clear();
        
        // 初始化得分显示
        if (scoreText != null)
        {
            scoreText.text = "当前得分: 0.0%";
            Debug.Log("初始化得分显示为0.0%");
        }
        
        // 同时通过SampleSceneManager初始化得分显示
        if (sampleSceneManager != null)
        {
            sampleSceneManager.UpdateScoreDisplay(0.0f);
        }
        
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
        challengeStartRealTime = Time.time; // 记录实际开始时间
        
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
        challengeCompleted = true;
        
        // 计算并显示最终得分
        float similarity = CalculateNewScore();
        Debug.Log($"挑战结束！最终得分: {similarity:F1}% (正确时长: {correctPlayTime:F2}s / 总时长: {totalMusicDuration:F2}s)");
        
        // 更新UI显示最终得分
        if (scoreText != null)
        {
            scoreText.text = $"最终得分: {similarity:F1}%";
            Debug.Log($"UI得分已更新: {similarity:F1}%");
        }
        else
        {
            Debug.LogWarning("scoreText为null，无法更新UI得分显示");
        }
        
        // 清理其他UI显示，但保留得分显示
        if (countdownText != null)
            countdownText.text = "";
        if (progressText != null)
            progressText.text = "";
        if (upcomingNotesText != null)
            upcomingNotesText.text = "";
            
        // 延迟3秒后隐藏UI，让用户能看到最终得分
        Invoke("HideChallengeUI", 3f);
        
        // 重置状态
        currentNoteIndex = 0;
        currentChallengeTime = 0f;
        playerPerformance.Clear();
        noteSequence.Clear();
        timedNoteSequence.Clear();
        
        // 音调生成器会在没有按键时自动停止
    }
    
    // 更新当前期望的音符
    private void UpdateCurrentExpectedNote()
    {
        string expectedNote = GetCurrentExpectedNoteByTime();
        if (expectedNote != currentExpectedNote)
        {
            currentExpectedNote = expectedNote;
            currentTargetNote = expectedNote; // 同步更新目标音符
            Debug.Log($"当前期望音符更新为: {currentExpectedNote}");
        }
    }
    
    // 累计正确演奏时长
    private void UpdateCorrectPlayTime()
    {
        if (string.IsNullOrEmpty(currentExpectedNote))
            return;
            
        // 获取当前正在演奏的音符
        string currentPlayingNote = GetCurrentPlayingNote();
        
        bool shouldAddScore = false;
        
        // 检查是否应该加分
        if (IsRestNote(currentExpectedNote))
        {
            // 如果当前期望音符是休止符，玩家不演奏就应该加分
            if (string.IsNullOrEmpty(currentPlayingNote))
            {
                shouldAddScore = true;
                Debug.Log($"休止符正确处理: 期望休止符，玩家未演奏 -> 加分");
            }
            else
            {
                Debug.Log($"休止符错误处理: 期望休止符，但玩家演奏了 '{currentPlayingNote}' -> 不加分");
            }
        }
        else
        {
            // 如果当前期望音符不是休止符，只有演奏正确音符才加分
            if (!string.IsNullOrEmpty(currentPlayingNote) && IsNoteMatch(currentExpectedNote, currentPlayingNote))
            {
                shouldAddScore = true;
                Debug.Log($"音符正确演奏: 期望 '{currentExpectedNote}', 演奏 '{currentPlayingNote}' -> 加分");
            }
            else if (string.IsNullOrEmpty(currentPlayingNote))
            {
                Debug.Log($"音符未演奏: 期望 '{currentExpectedNote}', 玩家未演奏 -> 不加分");
            }
            else
            {
                Debug.Log($"音符演奏错误: 期望 '{currentExpectedNote}', 演奏 '{currentPlayingNote}' -> 不加分");
            }
        }
        
        if (shouldAddScore)
        {
            correctPlayTime += Time.deltaTime;
            // 如果这是一个新的正确音符，增加计数
            if (!string.IsNullOrEmpty(currentPlayingNote) && !IsRestNote(currentExpectedNote))
            {
                // 这里可以添加逻辑来避免重复计数同一个音符
                // 暂时简单处理，每次正确演奏都计数
            }
            // Debug.Log($"正确演奏累计时长: {correctPlayTime:F2}s / {totalMusicDuration:F2}s");
        }
    }
    
    // 获取当前正在演奏的音符
    private string GetCurrentPlayingNote()
    {
        // 从ToneGenerator获取当前检测到的音符
        if (toneGenerator != null)
        {
            return toneGenerator.GetCurrentNoteName();
        }
        return "";
    }
    
    // 新的简化积分计算方法
    private float CalculateNewScore()
    {
        if (totalMusicDuration <= 0f)
        {
            currentScore = 0;
            return 0f;
        }
            
        float scorePercentage = (correctPlayTime / totalMusicDuration) * 100f;
        // 使用向上取整让玩家更容易获得高分，增加成就感
        float clampedScore = Mathf.Clamp(Mathf.Ceil(scorePercentage), 0f, 100f);
        currentScore = Mathf.RoundToInt(clampedScore);
        return clampedScore;
    }
    
    private void HideChallengeUI()
    {
        if (challengeUI != null)
            challengeUI.SetActive(false);
    }
    
    public string GetCurrentExpectedNote()
    {
        if (!isInChallenge || currentNoteIndex >= noteSequence.Count)
            return "";
            
        return noteSequence[currentNoteIndex];
    }
    
    public string GetCurrentExpectedNoteByTime()
    {
        if (!isInChallenge || timedNoteSequence.Count == 0)
            return "";
            
        // 根据当前时间找到应该演奏的音符
        foreach (var timedNote in timedNoteSequence)
        {
            if (currentChallengeTime >= timedNote.startTime && currentChallengeTime < timedNote.endTime)
            {
                return timedNote.noteName;
            }
        }
        
        return ""; // 当前时间没有音符
    }
    
public void OnNoteDetected(string detectedNote)
    {
        if (!isInChallenge || isCountingDown || challengeCompleted) 
        {
            Debug.Log($"音符检测被忽略: {detectedNote} (挑战状态: isInChallenge={isInChallenge}, isCountingDown={isCountingDown}, challengeCompleted={challengeCompleted})");
            return;
        }
        
        // 实时记录玩家演奏的音符
        float currentTime = Time.time - challengeStartRealTime;
        
        // 避免重复记录相同的音符（在短时间内）
        if (detectedNote != lastDetectedNote || (currentTime - lastNoteDetectionTime) > NOTE_DETECTION_INTERVAL)
        {
            PlayerNote playerNote = new PlayerNote(detectedNote, currentTime, NOTE_DETECTION_INTERVAL);
            playerPerformance.Add(playerNote);
            
            lastDetectedNote = detectedNote;
            lastNoteDetectionTime = currentTime;
            
            Debug.Log($"✓ 记录玩家演奏: {detectedNote} 在时间 {currentTime:F2}s, 持续时间: {NOTE_DETECTION_INTERVAL}s (总记录数: {playerPerformance.Count})");
        }
        else
        {
            Debug.Log($"重复音符被忽略: {detectedNote} (距离上次检测: {currentTime - lastNoteDetectionTime:F3}s)");
        }
    }
    
    private void GenerateNoteSequence()
    {
        noteSequence.Clear();
        
        // 生成5个随机音符，包含八度信息
        for (int i = 0; i < 5; i++)
        {
            string randomNote = notes[Random.Range(0, notes.Length)];
            int randomOctave = Random.Range(3, 6); // 3-5八度范围
            string noteWithOctave = randomNote + randomOctave.ToString();
            noteSequence.Add(noteWithOctave);
        }
        
        Debug.Log($"新的音符序列: {string.Join(", ", noteSequence)}");
    }

private void GenerateTimedNoteSequenceFromSheet(MusicSheet musicSheet)
    {
        timedNoteSequence.Clear();
        noteSequence.Clear(); // 保持兼容性
        
        float currentTime = 0f;
        float beatDuration = 60f / musicSheet.bpm; // 每拍的时长（秒）
        
        for (int i = 0; i < musicSheet.notes.Count; i++)
        {
            var note = musicSheet.notes[i];
            float noteDuration = beatDuration * note.duration; // 音符实际持续时间
            
            // 修复：包含休止符在内的所有音符都应该被添加到序列中
            if (!string.IsNullOrEmpty(note.noteName))
            {
                TimedNote timedNote = new TimedNote(note.noteName, currentTime, noteDuration);
                timedNoteSequence.Add(timedNote);
                
                // 只有非休止符才添加到noteSequence（保持兼容性）
                if (!note.isRest)
                {
                    noteSequence.Add(note.noteName);
                }
            }
            
            currentTime += noteDuration;
        }
        
        Debug.Log($"从乐谱生成的带时间音符序列: {timedNoteSequence.Count}个音符，总时长: {currentTime:F2}秒");
        foreach (var timedNote in timedNoteSequence)
        {
            string noteType = IsRestNote(timedNote.noteName) ? "(休止符)" : "";
            Debug.Log($"音符: {timedNote.noteName}{noteType}, 开始: {timedNote.startTime:F2}s, 持续: {timedNote.duration:F2}s");
        }
    }
    
    private void GenerateTimedNoteSequence()
    {
        timedNoteSequence.Clear();
        noteSequence.Clear(); // 保持兼容性
        
        float currentTime = 0f;
        float defaultNoteDuration = 1f; // 默认每个音符1秒
        
        // 生成5个随机音符
        for (int i = 0; i < 5; i++)
        {
            string randomNote = notes[Random.Range(0, notes.Length)];
            int randomOctave = Random.Range(3, 6); // 3-5八度范围
            string noteWithOctave = randomNote + randomOctave.ToString();
            
            TimedNote timedNote = new TimedNote(noteWithOctave, currentTime, defaultNoteDuration);
            timedNoteSequence.Add(timedNote);
            noteSequence.Add(noteWithOctave); // 保持兼容性
            
            currentTime += defaultNoteDuration;
        }
        
        Debug.Log($"生成的随机带时间音符序列: {string.Join(", ", noteSequence)}");
    }
    
    private void GenerateNoteSequenceFromSheet(MusicSheet musicSheet)
    {
        noteSequence.Clear();
        
        // 从乐谱中提取前10个音符作为挑战序列
        int maxNotes = Mathf.Min(10, musicSheet.notes.Count);
        for (int i = 0; i < maxNotes; i++)
        {
            var note = musicSheet.notes[i];
            // 保持完整的音符名称（包含八度）
            string noteString = note.noteName;
            if (!string.IsNullOrEmpty(noteString) && !note.isRest)
            {
                noteSequence.Add(noteString);
            }
        }
        
        // 如果乐谱音符不够，用随机音符填充
        while (noteSequence.Count < 5)
        {
            string randomNote = notes[Random.Range(0, notes.Length)];
            int randomOctave = Random.Range(3, 6); // 3-5八度范围
            string noteWithOctave = randomNote + randomOctave.ToString();
            noteSequence.Add(noteWithOctave);
        }
        
        Debug.Log($"从乐谱生成的音符序列: {string.Join(", ", noteSequence)}");
    }
    
    private string ConvertNoteToString(Note note)
    {
        // 将Note对象转换为完整的音符字符串（保持八度信息）
        if (note == null || note.isRest || string.IsNullOrEmpty(note.noteName))
            return "";
            
        return note.noteName; // 直接返回完整的音符名称
    }

    private void UpdateChallengeUI()
    {
        // 实时更新得分显示
        if (scoreText != null)
        {
            if (challengeCompleted)
            {
                // 挑战完成时显示最终得分
                float finalSimilarity = CalculateNewScore();
                scoreText.text = $"最终得分: {finalSimilarity:F1}%";
                Debug.Log($"显示最终得分: {finalSimilarity:F1}% (正确时长: {correctPlayTime:F2}s / 总时长: {totalMusicDuration:F2}s)");
            }
            else
            {
                // 挑战进行中显示实时得分
                float currentSimilarity = CalculateNewScore();
                scoreText.text = $"当前得分: {currentSimilarity:F1}%";
            }
        }
        else
        {
            Debug.LogWarning("scoreText为null，无法更新得分显示");
        }
        
        // 同时通过SampleSceneManager更新得分
        if (sampleSceneManager != null)
        {
            float currentSimilarity = CalculateNewScore();
            sampleSceneManager.UpdateScoreDisplay(currentSimilarity);
        }
            
        if (targetNoteText != null)
        {
            string currentExpectedNote = GetCurrentExpectedNoteByTime();
            if (!string.IsNullOrEmpty(currentExpectedNote))
            {
                int currentKey = GetCurrentKey();
                string solfegeName = ConvertToSolfege(currentExpectedNote, currentKey);
                targetNoteText.text = $"目标音符: {currentExpectedNote} ({solfegeName})";
            }
            else
            {
                targetNoteText.text = "当前无音符";
            }
        }
        
        if (timeText != null)
        {
            float remainingTime = currentChallengeDuration - currentChallengeTime;
            remainingTime = Mathf.Max(0, remainingTime);
            timeText.text = $"剩余时间: {remainingTime:F1}s";
        }
        
        // 显示时间进度
        if (progressText != null)
        {
            float progressPercent = (currentChallengeTime / currentChallengeDuration) * 100f;
            progressPercent = Mathf.Min(100f, progressPercent);
            progressText.text = $"进度: {progressPercent:F1}%";
        }
        
        // 更新进度条（基于时间）
        if (progressSlider != null)
        {
            progressSlider.value = currentChallengeTime / currentChallengeDuration;
        }
        
        // 显示即将到来的音符
        if (upcomingNotesText != null)
        {
            string upcomingNotes = GetUpcomingNotesByTime(3);
            upcomingNotesText.text = upcomingNotes;
        }
    }

    private string GetUpcomingNotesByTime(int count)
    {
        if (timedNoteSequence.Count == 0)
            return "无音符";
            
        List<string> noteNames = new List<string>();
        List<string> solfegeNames = new List<string>();
        List<float> noteTimes = new List<float>();
        List<bool> isCurrentNote = new List<bool>();
        
        int currentKey = GetCurrentKey();
        int foundCount = 0;
        
        // 首先查找当前音符
        TimedNote currentTimedNote = null;
        foreach (var timedNote in timedNoteSequence)
        {
            if (currentChallengeTime >= timedNote.startTime && currentChallengeTime < timedNote.endTime)
            {
                currentTimedNote = timedNote;
                break;
            }
        }
        
        // 如果有当前音符，先添加它
        if (currentTimedNote != null && foundCount < count)
        {
            noteNames.Add(currentTimedNote.noteName);
            noteTimes.Add(currentTimedNote.startTime);
            isCurrentNote.Add(true);
            
            // 转换为简谱音名
            string solfegeName = ConvertToSolfege(currentTimedNote.noteName, currentKey);
            solfegeNames.Add(solfegeName);
            
            foundCount++;
        }
        
        // 然后找到接下来的音符
        foreach (var timedNote in timedNoteSequence)
        {
            if (timedNote.startTime > currentChallengeTime && foundCount < count)
            {
                noteNames.Add(timedNote.noteName);
                noteTimes.Add(timedNote.startTime);
                isCurrentNote.Add(false);
                
                // 转换为简谱音名
                string solfegeName = ConvertToSolfege(timedNote.noteName, currentKey);
                solfegeNames.Add(solfegeName);
                
                foundCount++;
            }
        }
        
        if (noteNames.Count == 0)
            return "无音符";
        
        // 构建显示格式
        string result = "音符序列：\n";
        for (int i = 0; i < noteNames.Count; i++)
        {
            if (isCurrentNote[i])
            {
                result += $"{noteNames[i]}({solfegeNames[i]})";
            }
            else
            {
                float timeUntil = noteTimes[i] - currentChallengeTime;
                result += $"{noteNames[i]}({solfegeNames[i]}) {timeUntil:F1}s后";
            }
            if (i < noteNames.Count - 1) result += " | ";
        }
        
        return result;
    }
    
    private string GetUpcomingNotes(int count)
    {
        List<string> upcomingNoteNames = new List<string>();
        List<string> upcomingSolfegeNames = new List<string>();
        
        int currentKey = GetCurrentKey();
        
        for (int i = 0; i < count && (currentNoteIndex + i) < noteSequence.Count; i++)
        {
            string noteName = noteSequence[currentNoteIndex + i];
            
            // 保持原有的八度信息显示
            upcomingNoteNames.Add(noteName);
            
            // 转换为简谱音名（包含八度标记）
            string solfegeName = ConvertToSolfege(noteName, currentKey);
            upcomingSolfegeNames.Add(solfegeName);
        }
        
        // 构建多行显示格式
        string result = "即将到来的音符：\n";
        result += string.Join(" ", upcomingNoteNames) + "\n";
        result += string.Join(" ", upcomingSolfegeNames);
        
        return result;
    }
    
private void CalculateSimilarityAndEndChallenge()
    {
        // 计算基于时长的相似度
        float similarity = CalculateNewScore();
        
        Debug.Log($"演奏完毕！相似度: {similarity:F1}% (正确时长: {correctPlayTime:F2}s / 总时长: {totalMusicDuration:F2}s)");
        
        // 显示最终得分
        if (scoreText != null)
        {
            scoreText.text = $"最终得分: {similarity:F1}%";
            Debug.Log($"UI得分已更新: {similarity:F1}%");
        }
        else
        {
            Debug.LogWarning("scoreText为null，无法更新UI得分显示");
        }
        
        // 等待3秒后退出挑战（不重新计算得分）
        Invoke("ExitChallengeWithoutRecalculation", 3f);
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
    
    // 检查是否为休止符
    private bool IsRestNote(string noteName)
    {
        if (string.IsNullOrEmpty(noteName))
            return false;
            
        // 常见的休止符表示方法
        string noteBase = ExtractNoteBase(noteName).ToLower();
        return noteBase == "rest" || noteBase == "r" || noteBase == "pause" || noteBase == "0";
    }
    
    // 将音符转换为简谱音名
    private string ConvertToSolfege(string noteName, int key)
    {
        if (string.IsNullOrEmpty(noteName))
            return "";
            
        // 检查是否为休止符
        if (IsRestNote(noteName))
        {
            return "休止符";
        }
            
        // 提取音符基础名称和八度
        string noteBase = ExtractNoteBase(noteName);
        int octave = ExtractOctave(noteName);
        
        // 音符到半音数的映射（C=0, C#=1, D=2, ...）
        Dictionary<string, int> noteToSemitone = new Dictionary<string, int>
        {
            {"C", 0}, {"C#", 1}, {"D", 2}, {"D#", 3}, {"E", 4}, {"F", 5},
            {"F#", 6}, {"G", 7}, {"G#", 8}, {"A", 9}, {"A#", 10}, {"B", 11}
        };
        
        if (!noteToSemitone.ContainsKey(noteBase))
            return noteBase;
            
        // 获取音符的半音数
        int noteSemitone = noteToSemitone[noteBase];
        
        // 根据调号计算主音的半音数
        // key范围：-4到7，对应A♭(-4)到G(7)
        int tonicSemitone = GetTonicSemitone(key);
        
        // 计算相对于主音的半音数
        int relativeSemitone = (noteSemitone - tonicSemitone + 12) % 12;
        
        // 简谱音名映射
        string[] solfegeNames = {"1", "♯1", "2", "♯2", "3", "4", "♯4", "5", "♯5", "6", "♯6", "7"};
        
        string solfegeName = solfegeNames[relativeSemitone];
        
        // 添加八度标记 - 根据当前调号判断音高区域
        string octavePrefix = GetOctavePrefixByKey(noteName, key);
        
        return octavePrefix + solfegeName;
    }
    
    // 提取八度信息
    private int ExtractOctave(string noteName)
    {
        if (string.IsNullOrEmpty(noteName))
            return 4; // 默认中音区
            
        // 从音符名称中提取数字部分
        for (int i = 0; i < noteName.Length; i++)
        {
            if (char.IsDigit(noteName[i]))
            {
                if (int.TryParse(noteName.Substring(i), out int octave))
                {
                    return octave;
                }
            }
        }
        
        return 4; // 默认中音区
    }
    
    // 根据调号获取主音的半音数
    private int GetTonicSemitone(int key)
    {
        // 根据ToneGenerator中的映射关系
        return key switch
        {
            -4 => 8,  // A♭
            -3 => 9,  // A
            -2 => 10, // B♭
            -1 => 11, // B
            0 => 0,   // C
            1 => 1,   // D♭
            2 => 2,   // D
            3 => 3,   // E♭
            4 => 4,   // E
            5 => 5,   // F
            6 => 6,   // F♯
            7 => 7,   // G
            _ => 0    // 默认C
        };
    }
    
    // 根据八度获取前缀（原方法，保留兼容性）
    private string GetOctavePrefix(int octave)
    {
        return octave switch
        {
            <= 3 => "低音",
            4 => "中音",
            >= 5 => "高音"
        };
    }
    
    // 根据调号和音符名称获取八度前缀
    private string GetOctavePrefixByKey(string noteName, int key)
    {
        if (string.IsNullOrEmpty(noteName))
            return "中音";
            
        // 提取音符基础名称和八度
        string noteBase = ExtractNoteBase(noteName);
        int octave = ExtractOctave(noteName);
        
        // 音符到半音数的映射（C=0, C#=1, D=2, ...）
        Dictionary<string, int> noteToSemitone = new Dictionary<string, int>
        {
            {"C", 0}, {"C#", 1}, {"D", 2}, {"D#", 3}, {"E", 4}, {"F", 5},
            {"F#", 6}, {"G", 7}, {"G#", 8}, {"A", 9}, {"A#", 10}, {"B", 11}
        };
        
        if (!noteToSemitone.ContainsKey(noteBase))
            return "中音";
            
        // 计算当前音符的绝对半音数（相对于C0）
        int noteAbsoluteSemitone = octave * 12 + noteToSemitone[noteBase];
        
        // 计算调号主音在第4八度的绝对半音数
        int tonicSemitone = GetTonicSemitone(key);
        int tonicAbsoluteSemitone = 4 * 12 + tonicSemitone; // 调号主音默认在第4八度
        
        // 计算音符相对于调号主音的半音差
        int semitoneDifference = noteAbsoluteSemitone - tonicAbsoluteSemitone;
        
        // 根据半音差判断音高区域
        if (semitoneDifference < 0)
        {
            return "低音"; // 比调号主音低的都是低音
        }
        else if (semitoneDifference >= 12)
        {
            return "高音"; // 比调号主音高一个八度及以上的都是高音
        }
        else
        {
            return "中音"; // 在调号主音到高一个八度之间的是中音
        }
    }

    // 获取当前调号
    private int GetCurrentKey()
    {
        if (toneGenerator != null)
        {
            return toneGenerator.key; // 直接访问public key变量
        }
        return 0; // 默认C调
    }
        
private float CalculatePerformanceSimilarity()
    {
        Debug.Log($"=== 开始计算评分相似度 ===");
        Debug.Log($"timedNoteSequence数量: {timedNoteSequence.Count}");
        Debug.Log($"playerPerformance数量: {playerPerformance.Count}");
        
        if (timedNoteSequence.Count == 0)
        {
            Debug.Log("timedNoteSequence为空，返回0分");
            return 0f;
        }

        float totalCorrectTime = 0f;  // 所有音符的正确演奏时间总和
        float totalExpectedTime = 0f; // 所有音符的期望时间总和

        // 计算每个音符的正确演奏时间占比
        for (int i = 0; i < timedNoteSequence.Count; i++)
        {
            var timedNote = timedNoteSequence[i];
            float noteDuration = timedNote.duration;
            totalExpectedTime += noteDuration;
            
            Debug.Log($"处理音符 {i+1}: {timedNote.noteName}, 开始时间: {timedNote.startTime:F2}s, 持续时间: {timedNote.duration:F2}s");
            
            // 计算在这个音符时间段内，玩家演奏正确的时间
            float correctTime = CalculateCorrectTimeForNote(timedNote);
            totalCorrectTime += correctTime;
            
            float correctPercentage = noteDuration > 0 ? (correctTime / noteDuration) * 100f : 0f;
            Debug.Log($"音符 {timedNote.noteName} 正确演奏时间: {correctTime:F2}s / {noteDuration:F2}s ({correctPercentage:F1}%)");
        }

        Debug.Log($"总正确演奏时间: {totalCorrectTime:F2}s");
        Debug.Log($"总期望时间: {totalExpectedTime:F2}s");

        // 计算相似度百分比
        if (totalExpectedTime <= 0f)
        {
            Debug.Log("总期望时间为0，返回0分");
            return 0f;
        }
            
        float similarity = (totalCorrectTime / totalExpectedTime) * 100f;
        Debug.Log($"计算得出相似度: {similarity:F2}%");
        
        float finalScore = Mathf.Clamp(similarity, 0f, 100f);
        Debug.Log($"最终得分: {finalScore:F2}%");
        Debug.Log($"=== 评分计算完成 ===");
        
        return finalScore;
    }
    
private float CalculateCorrectTimeForNote(TimedNote timedNote)
    {
        float correctTime = 0f;
        int matchingRecords = 0;
        
        Debug.Log($"  计算音符 {timedNote.noteName} 的正确演奏时间 (时间段: {timedNote.startTime:F2}s - {timedNote.endTime:F2}s)");
        
        // 检查是否为休止符
        if (IsRestNote(timedNote.noteName))
        {
            Debug.Log($"    当前音符是休止符: {timedNote.noteName}");
            
            // 对于休止符，检查玩家是否在这个时间段内没有演奏
            bool hasPlayedDuringRest = false;
            float totalPlayedTime = 0f;
            
            foreach (var record in playerPerformance)
            {
                float recordStartTime = record.timestamp;
                float recordEndTime = record.timestamp + record.duration;
                
                // 计算记录与休止符时间段的重叠部分
                float overlapStart = Mathf.Max(recordStartTime, timedNote.startTime);
                float overlapEnd = Mathf.Min(recordEndTime, timedNote.endTime);
                
                if (overlapStart < overlapEnd)
                {
                    float overlapDuration = overlapEnd - overlapStart;
                    hasPlayedDuringRest = true;
                    totalPlayedTime += overlapDuration;
                    Debug.Log($"    ✗ 休止符期间有演奏: {record.noteName} (重叠时长: {overlapDuration:F2}s)");
                }
            }
            
            if (!hasPlayedDuringRest)
            {
                // 休止符期间没有演奏，给满分
                correctTime = timedNote.duration;
                Debug.Log($"    ✓ 休止符正确处理: 期间无演奏，获得满分 {correctTime:F2}s");
            }
            else
            {
                // 休止符期间有演奏，扣除演奏的时间
                correctTime = Mathf.Max(0f, timedNote.duration - totalPlayedTime);
                Debug.Log($"    ✗ 休止符错误处理: 期间有演奏 {totalPlayedTime:F2}s，得分 {correctTime:F2}s");
            }
        }
        else
        {
            // 对于普通音符，检查玩家是否演奏了正确的音符
            Debug.Log($"    当前音符是普通音符: {timedNote.noteName}");
            
            foreach (var record in playerPerformance)
            {
                float recordStartTime = record.timestamp;
                float recordEndTime = record.timestamp + record.duration;
                
                // 计算记录与音符时间段的重叠部分
                float overlapStart = Mathf.Max(recordStartTime, timedNote.startTime);
                float overlapEnd = Mathf.Min(recordEndTime, timedNote.endTime);
                
                if (overlapStart < overlapEnd)
                {
                    float overlapDuration = overlapEnd - overlapStart;
                    
                    Debug.Log($"    检查演奏记录: {record.noteName} (时间: {recordStartTime:F2}s - {recordEndTime:F2}s), 重叠时长: {overlapDuration:F2}s");
                    
                    // 只有演奏的音符正确才计入正确时间
                    if (IsNoteMatch(timedNote.noteName, record.noteName))
                    {
                        correctTime += overlapDuration;
                        matchingRecords++;
                        Debug.Log($"    ✓ 音符匹配！累计正确时间: {correctTime:F2}s");
                    }
                    else
                    {
                        Debug.Log($"    ✗ 音符不匹配: 期望 {timedNote.noteName}, 实际 {record.noteName}");
                    }
                }
            }
            
            // 对于普通音符，如果没有任何演奏记录，得分为0
            if (matchingRecords == 0)
            {
                Debug.Log($"    ✗ 普通音符期间无正确演奏，得分为0");
            }
        }
        
        Debug.Log($"  音符 {timedNote.noteName} 总正确时间: {correctTime:F2}s, 匹配记录数: {matchingRecords}");
        
        // 确保正确时间不超过音符的总时长
        float finalCorrectTime = Mathf.Min(correctTime, timedNote.duration);
        if (finalCorrectTime != correctTime)
        {
            Debug.Log($"  正确时间被限制为音符时长: {finalCorrectTime:F2}s");
        }
        
        return finalCorrectTime;
    }
    
    // 检查两个音符是否匹配
    private bool IsNoteMatch(string expectedNote, string playedNote)
    {
        if (string.IsNullOrEmpty(expectedNote) || string.IsNullOrEmpty(playedNote))
        {
            Debug.Log($"      音符匹配检查: 空字符串 - 期望: '{expectedNote}', 演奏: '{playedNote}' -> false");
            return false;
        }
            
        // 提取音符的基础名称（去掉八度信息）
        string expectedBase = ExtractNoteBase(expectedNote);
        string playedBase = ExtractNoteBase(playedNote);
        
        // 比较基础音符名称（忽略大小写）
        bool isMatch = string.Equals(expectedBase, playedBase, System.StringComparison.OrdinalIgnoreCase);
        
        Debug.Log($"      音符匹配检查: 期望 '{expectedNote}' ({expectedBase}) vs 演奏 '{playedNote}' ({playedBase}) -> {isMatch}");
        
        return isMatch;
    }
    
    private void ProcessRealtimeNoteDetection()
    {
        // 实时处理音符检测，这里可以添加更复杂的逻辑
        // 目前主要通过OnNoteDetected方法来处理检测到的音符
        
        // 可以在这里添加音符检测的额外处理逻辑
        // 比如音符持续时间的计算、音符质量评估等
    }
    
    private void CheckChallengeCompletion()
    {
        if (challengeCompleted || !isInChallenge)
            return;
            
        // 检查挑战是否超时
        if (currentChallengeTime >= currentChallengeDuration)
        {
            challengeCompleted = true;
            CalculateFinalScore();
        }
    }
    
    private void CalculateFinalScore()
    {
        challengeCompleted = true;
        
        // 计算基于时长的相似度
        float similarity = CalculateNewScore();
        
        Debug.Log($"演奏完毕！相似度: {similarity:F1}% (正确时长: {correctPlayTime:F2}s / 总时长: {totalMusicDuration:F2}s)");
        
        // 显示最终得分
        if (scoreText != null)
        {
            scoreText.text = $"最终得分: {similarity:F1}%";
            Debug.Log($"UI得分已更新: {similarity:F1}%");
        }
        else
        {
            Debug.LogWarning("scoreText为null，无法更新UI得分显示");
        }
        
        // 等待3秒后退出挑战（不重新计算得分）
        Invoke("ExitChallengeWithoutRecalculation", 3f);
    }
    
    private void ExitChallengeWithoutRecalculation()
    {
        isInChallenge = false;
        isCountingDown = false;
        challengeCompleted = true;
        
        // 清理其他UI显示，但保留得分显示
        if (countdownText != null)
            countdownText.text = "";
        if (progressText != null)
            progressText.text = "";
        if (upcomingNotesText != null)
            upcomingNotesText.text = "";
            
        // 延迟3秒后隐藏UI，让用户能看到最终得分
        Invoke("HideChallengeUI", 3f);
        
        // 重置状态
        currentNoteIndex = 0;
        currentChallengeTime = 0f;
        playerPerformance.Clear();
        noteSequence.Clear();
        timedNoteSequence.Clear();
        
        // 音调生成器会在没有按键时自动停止
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
    
    // 获取当前分数（百分比）
    public float GetCurrentScore()
    {
        return CalculateNewScore();
    }
    
    // 获取当前目标音符
    public string GetCurrentTargetNote()
    {
        return currentTargetNote;
    }
    
    // 获取已演奏音符数量
    public int GetPlayedNotesCount()
    {
        return playedNotesCount;
    }
}