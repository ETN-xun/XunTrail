using UnityEngine;

public class ChallengeTest : MonoBehaviour
{
    void Start()
    {
        // 等待一秒后开始测试
        Invoke("TestMusicSheetParserOnly", 1f);
    }
    
    void StartChallengeTest()
    {
        Debug.Log("开始挑战测试...");
        
        // 查找ChallengeManager
        ChallengeManager challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager == null)
        {
            Debug.LogError("未找到ChallengeManager!");
            return;
        }
        
        // 查找ToneGenerator
        ToneGenerator toneGenerator = FindObjectOfType<ToneGenerator>();
        if (toneGenerator == null)
        {
            Debug.LogError("未找到ToneGenerator!");
            return;
        }
        
        // 加载示例乐谱
        string filePath = Application.dataPath + "/乐谱/示例乐谱.txt";
        if (System.IO.File.Exists(filePath))
        {
            Debug.Log("找到示例乐谱文件: " + filePath);
            
            // 解析乐谱
            var musicSheet = MusicSheetParser.Instance.ParseMusicSheet(filePath);
            if (musicSheet != null && musicSheet.notes.Count > 0)
            {
                Debug.Log($"成功解析乐谱，包含 {musicSheet.notes.Count} 个音符");
                
                // 开始挑战
                challengeManager.StartChallenge(musicSheet);
                Debug.Log("挑战已开始!");
                
                // 激活UI元素
                ActivateUIElements();
            }
            else
            {
                Debug.LogError("乐谱解析失败!");
            }
        }
        else
        {
            Debug.LogError("未找到示例乐谱文件: " + filePath);
        }
    }
    
    void ActivateUIElements()
    {
        // 激活挑战相关的UI元素
        GameObject progressText = GameObject.Find("ProgressText");
        if (progressText) progressText.SetActive(true);
        
        GameObject upcomingNotesText = GameObject.Find("UpcomingNotesText");
        if (upcomingNotesText) upcomingNotesText.SetActive(true);
        
        GameObject scoreText = GameObject.Find("ScoreText");
        if (scoreText) scoreText.SetActive(true);
        
        GameObject progressSlider = GameObject.Find("ProgressSlider");
        if (progressSlider) progressSlider.SetActive(true);
        
        GameObject noteDisplayText = GameObject.Find("NoteDisplayText");
        if (noteDisplayText) noteDisplayText.SetActive(true);
        
        GameObject octaveDisplayText = GameObject.Find("OctaveDisplayText");
        if (octaveDisplayText) octaveDisplayText.SetActive(true);
        
        GameObject keyDisplayText = GameObject.Find("KeyDisplayText");
        if (keyDisplayText) keyDisplayText.SetActive(true);
        
        Debug.Log("UI元素已激活");
    }
    
    void TestMusicSheetParserOnly()
    {
        Debug.Log("=== 开始测试MusicSheetParser ===");
        
        try
        {
            // 测试单例模式
            var parser1 = MusicSheetParser.Instance;
            var parser2 = MusicSheetParser.Instance;
            
            if (parser1 == parser2)
            {
                Debug.Log("✓ 单例模式工作正常");
            }
            else
            {
                Debug.LogError("✗ 单例模式失败");
                return;
            }
            
            // 测试解析示例乐谱
            string filePath = Application.dataPath + "/乐谱/示例乐谱.txt";
            if (System.IO.File.Exists(filePath))
            {
                Debug.Log($"找到示例乐谱文件: {filePath}");
                
                var musicSheet = MusicSheetParser.Instance.ParseMusicSheet(filePath);
                if (musicSheet != null && musicSheet.notes.Count > 0)
                {
                    Debug.Log($"✓ 乐谱解析成功! 文件名: {musicSheet.fileName}, BPM: {musicSheet.bpm}, 音符数: {musicSheet.notes.Count}");
                    
                    // 显示前几个音符
                    for (int i = 0; i < Mathf.Min(3, musicSheet.notes.Count); i++)
                    {
                        var note = musicSheet.notes[i];
                        Debug.Log($"  音符 {i+1}: {note.noteName}, 时长: {note.duration}, 频率: {note.frequency}Hz");
                    }
                }
                else
                {
                    Debug.LogError("✗ 乐谱解析失败");
                }
            }
            else
            {
                Debug.LogWarning($"示例乐谱文件不存在: {filePath}");
            }
            
            Debug.Log("=== MusicSheetParser测试完成! ===");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"测试过程中发生错误: {e.Message}");
            Debug.LogError($"堆栈跟踪: {e.StackTrace}");
        }
    }
}