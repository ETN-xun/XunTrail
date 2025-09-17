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
        
    // 玩家演奏记录
    private List<PlayerNote> playerPerformance = new List<PlayerNote>();
    private float challengeStartRealTime; // 挑战实际开始时间
    
    [System.Serializable]
    public class PlayerNote
    {
        public string noteName;
        public float timestamp; // 相对于挑战开始的时间
        public float duration;
        
        public PlayerNote(string note, float time, float dur = 0.5f)
        {
            noteName = note;
            timestamp = time;
            duration = dur;
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
        
        // 初始化玩家演奏记录
        playerPerformance.Clear();
        
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
        
        // 记录玩家演奏的音符
        float currentTime = Time.time - challengeStartRealTime;
        PlayerNote playerNote = new PlayerNote(detectedNote, currentTime);
        playerPerformance.Add(playerNote);
        
        Debug.Log($"记录玩家演奏: {detectedNote} 在时间 {currentTime:F2}s");
        
        // 检查是否与当前期望音符匹配
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
        
        // 更新进度条
        if (progressSlider != null && totalNotesCount > 0)
        {
            progressSlider.value = (float)playedNotesCount / totalNotesCount;
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
        // 计算基于时间和音符匹配的相似度
        float similarity = CalculatePerformanceSimilarity();
        
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
    
    // 将音符转换为简谱音名
    private string ConvertToSolfege(string noteName, int key)
    {
        if (string.IsNullOrEmpty(noteName))
            return "";
            
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
        if (currentMusicSheet == null || currentMusicSheet.notes == null || currentMusicSheet.notes.Count == 0)
        {
            // 如果没有乐谱，只基于正确音符数量计算
            return totalNotesCount > 0 ? (float)currentScore / totalNotesCount * 100f : 0f;
        }
        
        float totalScore = 0f;
        float maxScore = 0f;
        
        // 计算时间窗口内的音符匹配度
        float beatDuration = 60f / currentMusicSheet.bpm;
        
        for (int i = 0; i < currentMusicSheet.notes.Count && i < noteSequence.Count; i++)
        {
            Note expectedNote = currentMusicSheet.notes[i];
            float expectedTime = i * beatDuration * expectedNote.duration;
            
            // 在时间窗口内查找最匹配的玩家音符
            float timeWindow = beatDuration * 0.5f; // 允许半拍的误差
            PlayerNote bestMatch = null;
            float bestTimeDiff = float.MaxValue;
            
            foreach (PlayerNote playerNote in playerPerformance)
            {
                float timeDiff = Mathf.Abs(playerNote.timestamp - expectedTime);
                if (timeDiff <= timeWindow && timeDiff < bestTimeDiff)
                {
                    bestTimeDiff = timeDiff;
                    bestMatch = playerNote;
                }
            }
            
            maxScore += 1f;
            
            if (bestMatch != null)
            {
                // 音符匹配度 (0-0.7)
                float noteScore = IsCorrectNote(bestMatch.noteName, ConvertNoteToString(expectedNote)) ? 0.7f : 0f;
                
                // 时间准确度 (0-0.3)
                float timeScore = 0.3f * (1f - (bestTimeDiff / timeWindow));
                
                totalScore += noteScore + timeScore;
            }
        }
        
        return maxScore > 0 ? (totalScore / maxScore) * 100f : 0f;
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