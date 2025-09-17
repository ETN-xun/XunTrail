using UnityEngine;
using UnityEngine.UI;

public class SimpleChallengeTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 简单挑战模式测试开始 ===");
        TestBasicComponents();
    }
    
    void TestBasicComponents()
    {
        // 测试MusicSheetParser
        var parser = FindObjectOfType<MusicSheetParser>();
        if (parser == null)
        {
            Debug.Log("创建MusicSheetParser实例...");
            var go = new GameObject("MusicSheetParser");
            parser = go.AddComponent<MusicSheetParser>();
        }
        
        if (parser != null)
        {
            Debug.Log("✓ MusicSheetParser可用");
            
            // 测试解析简单乐谱
            string testContent = "120\nC4 1\nD4 1\nE4 1";
            try
            {
                var musicSheet = parser.ParseMusicSheetFromText(testContent, "测试乐谱");
                if (musicSheet != null)
                {
                    Debug.Log($"✓ 乐谱解析成功: BPM={musicSheet.bpm}, 音符数={musicSheet.notes.Count}");
                }
                else
                {
                    Debug.LogError("✗ 乐谱解析返回null");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ 乐谱解析异常: {e.Message}");
            }
        }
        
        // 测试ChallengeSceneManager
        var sceneManager = FindObjectOfType<ChallengeSceneManager>();
        if (sceneManager != null)
        {
            Debug.Log("✓ ChallengeSceneManager找到");
        }
        else
        {
            Debug.LogError("✗ ChallengeSceneManager未找到");
        }
        
        // 测试ChallengeDataManager
        var dataManager = FindObjectOfType<ChallengeDataManager>();
        if (dataManager != null)
        {
            Debug.Log("✓ ChallengeDataManager找到");
        }
        else
        {
            Debug.LogError("✗ ChallengeDataManager未找到");
        }
        
        // 测试UI元素
        TestUIElements();
    }
    
    void TestUIElements()
    {
        Debug.Log("--- 测试UI元素 ---");
        
        var selectButton = GameObject.Find("SelectFileButton");
        if (selectButton != null)
        {
            Debug.Log("✓ SelectFileButton找到");
            var button = selectButton.GetComponent<Button>();
            if (button != null)
            {
                Debug.Log("✓ SelectFileButton有Button组件");
            }
        }
        else
        {
            Debug.LogError("✗ SelectFileButton未找到");
        }
        
        var startButton = GameObject.Find("StartButton");
        if (startButton != null)
        {
            Debug.Log("✓ StartButton找到");
        }
        else
        {
            Debug.LogError("✗ StartButton未找到");
        }
        
        var backButton = GameObject.Find("BackButton");
        if (backButton != null)
        {
            Debug.Log("✓ BackButton找到");
        }
        else
        {
            Debug.LogError("✗ BackButton未找到");
        }
    }
    
    void Update()
    {
        // 按空格键测试文件选择
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestFileSelection();
        }
        
        // 按回车键测试开始挑战
        if (Input.GetKeyDown(KeyCode.Return))
        {
            TestStartChallenge();
        }
    }
    
    void TestFileSelection()
    {
        Debug.Log("--- 测试文件选择 ---");
        var sceneManager = FindObjectOfType<ChallengeSceneManager>();
        if (sceneManager != null)
        {
            // 模拟选择示例乐谱
            string testPath = Application.streamingAssetsPath + "/示例乐谱.txt";
            if (System.IO.File.Exists(testPath))
            {
                Debug.Log($"找到测试乐谱文件: {testPath}");
                // 这里可以调用场景管理器的方法来加载乐谱
            }
            else
            {
                Debug.LogError($"测试乐谱文件不存在: {testPath}");
            }
        }
    }
    
    void TestStartChallenge()
    {
        Debug.Log("--- 测试开始挑战 ---");
        var sceneManager = FindObjectOfType<ChallengeSceneManager>();
        if (sceneManager != null)
        {
            // 创建测试乐谱
            var parser = FindObjectOfType<MusicSheetParser>();
            if (parser != null)
            {
                string testContent = "120\nC4 1\nD4 1\nE4 1\nF4 1\nG4 1";
                var musicSheet = parser.ParseMusicSheetFromText(testContent, "测试乐谱");
                if (musicSheet != null)
                {
                    Debug.Log("准备开始挑战测试...");
                    // 这里可以调用开始挑战的方法
                }
            }
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("简单挑战模式测试", GUI.skin.box);
        GUILayout.Space(10);
        
        GUILayout.Label("按空格键: 测试文件选择");
        GUILayout.Label("按回车键: 测试开始挑战");
        
        GUILayout.EndArea();
    }
}