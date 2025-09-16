using UnityEngine;

public class ChallengeTest : MonoBehaviour
{
    void Start()
    {
        // 等待一秒后开始测试
        Invoke("StartChallengeTest", 1f);
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
            MusicSheetParser parser = FindObjectOfType<MusicSheetParser>();
            if (parser == null)
            {
                GameObject parserObj = new GameObject("MusicSheetParser");
                parser = parserObj.AddComponent<MusicSheetParser>();
            }
            
            var musicSheet = parser.ParseMusicSheet(filePath);
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
}