using UnityEngine;
using UnityEngine.SceneManagement;

public class ChallengeModeTest : MonoBehaviour
{
    [Header("测试设置")]
    public bool autoStartTest = true;
    public float testDelay = 2f;
    
    void Start()
    {
        if (autoStartTest)
        {
            Invoke(nameof(StartChallengeTest), testDelay);
        }
    }
    
    void Update()
    {
        // 按T键开始测试
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartChallengeTest();
        }
        
        // 按R键重新加载场景
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        // 按Escape键退出
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    
    void StartChallengeTest()
    {
        Debug.Log("=== 开始挑战模式测试 ===");
        
        // 1. 测试MusicSheetParser
        TestMusicSheetParser();
        
        // 2. 测试ChallengeManager
        TestChallengeManager();
        
        // 3. 测试ToneGenerator
        TestToneGenerator();
    }
    
    void TestMusicSheetParser()
    {
        Debug.Log("--- 测试MusicSheetParser ---");
        
        var parser = MusicSheetParser.Instance;
        if (parser != null)
        {
            Debug.Log("✓ MusicSheetParser实例创建成功");
            
            // 测试解析示例乐谱
            string testContent = "120\nC4 1\nD4 1\nE4 1\nF4 1\nG4 1";
            var musicSheet = parser.ParseMusicSheetFromText(testContent, "测试乐谱");
            
            if (musicSheet != null)
            {
                Debug.Log($"✓ 乐谱解析成功: BPM={musicSheet.bpm}, 音符数={musicSheet.notes.Count}, 总时长={musicSheet.totalDuration:F2}秒");
            }
            else
            {
                Debug.LogError("✗ 乐谱解析失败");
            }
        }
        else
        {
            Debug.LogError("✗ MusicSheetParser实例创建失败");
        }
    }
    
    void TestChallengeManager()
    {
        Debug.Log("--- 测试ChallengeManager ---");
        
        var challengeManager = ChallengeManager.Instance;
        if (challengeManager != null)
        {
            Debug.Log("✓ ChallengeManager实例找到");
            
            // 创建测试乐谱
            string testContent = "120\nC4 1\nD4 1\nE4 1\nF4 1\nG4 1";
            var musicSheet = MusicSheetParser.Instance.ParseMusicSheetFromText(testContent, "测试乐谱");
            
            if (musicSheet != null)
            {
                Debug.Log("开始挑战模式测试...");
                challengeManager.StartChallenge(musicSheet);
            }
        }
        else
        {
            Debug.LogError("✗ ChallengeManager实例未找到");
        }
    }
    
    void TestToneGenerator()
    {
        Debug.Log("--- 测试ToneGenerator ---");
        
        var toneGenerator = ToneGenerator.Instance;
        if (toneGenerator != null)
        {
            Debug.Log("✓ ToneGenerator实例找到");
        }
        else
        {
            Debug.LogError("✗ ToneGenerator实例未找到");
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("挑战模式测试", GUI.skin.box);
        GUILayout.Space(10);
        
        if (GUILayout.Button("开始测试 (T)"))
        {
            StartChallengeTest();
        }
        
        if (GUILayout.Button("重新加载场景 (R)"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        if (GUILayout.Button("退出 (Esc)"))
        {
            Application.Quit();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("当前场景: " + SceneManager.GetActiveScene().name);
        
        var challengeManager = ChallengeManager.Instance;
        if (challengeManager != null)
        {
            GUILayout.Label($"挑战状态: {(challengeManager.IsInChallenge() ? "进行中" : "未开始")}");
        }
        
        GUILayout.EndArea();
    }
}