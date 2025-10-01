using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChallengeSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public Button selectFileButton;
    public Button startButton;
    [SerializeField] public Button backButton;
    public Button prevFileButton;
    public Button nextFileButton;
    public Text selectFileText;
    public Text progressText;
    public Slider progressSlider;
    
    [Header("Challenge Settings")]
    private string selectedFilePath = "";
    private MusicSheet selectedMusicSheet;
    
    [Header("File Management")]
    private string[] availableFiles;
    private int currentFileIndex = 0;
    
    void Start()
    {
        // 确保GameModeManager存在
        if (GameModeManager.Instance == null)
        {
            Debug.LogWarning("ChallengeSceneManager: GameModeManager.Instance为null，正在创建新实例");
            GameObject gameModeManagerObj = new GameObject("GameModeManager");
            gameModeManagerObj.AddComponent<GameModeManager>();
        }
        
        // 确保ChallengeDataManager存在
        if (ChallengeDataManager.Instance == null)
        {
            Debug.LogWarning("ChallengeSceneManager: ChallengeDataManager.Instance为null，正在创建新实例");
            GameObject challengeDataManagerObj = new GameObject("ChallengeDataManager");
            challengeDataManagerObj.AddComponent<ChallengeDataManager>();
        }
        
        // 自动查找UI元素
        FindUIElements();
        
        // 强制分配UI引用（解决序列化问题）
        ForceAssignUIReferences();
        
        // 初始化文件列表
        InitializeFileList();
        
        // 绑定按钮事件
        if (selectFileButton != null)
            selectFileButton.onClick.AddListener(OnSelectFileClicked);
        
        if (startButton != null)
            startButton.onClick.AddListener(OnStartChallengeClicked);
            
        if (backButton != null)
            backButton.onClick.AddListener(OnBackToMenuClicked);
            
        if (prevFileButton != null)
            prevFileButton.onClick.AddListener(OnPrevFileClicked);
            
        if (nextFileButton != null)
            nextFileButton.onClick.AddListener(OnNextFileClicked);
        
        // 初始状态：禁用开始按钮
        if (startButton != null)
            startButton.interactable = false;
            
        // 自动加载第一个文件
        if (availableFiles != null && availableFiles.Length > 0)
        {
            LoadCurrentFile();
        }
    }
    
    void Update()
    {
        // 检测ESC键按下，直接退出游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }
    
    void ForceAssignUIReferences()
    {
        Debug.Log("ChallengeSceneManager: ForceAssignUIReferences 方法开始执行");
        // 强制查找并分配所有UI引用
        if (selectFileButton == null)
        {
            GameObject obj = GameObject.Find("SelectFileButton");
            if (obj != null) selectFileButton = obj.GetComponent<Button>();
        }
        
        if (startButton == null)
        {
            GameObject obj = GameObject.Find("StartButton");
            if (obj != null) startButton = obj.GetComponent<Button>();
        }
        
        if (backButton == null)
        {
            GameObject obj = GameObject.Find("BackButton");
            if (obj != null) backButton = obj.GetComponent<Button>();
        }
        
        if (prevFileButton == null)
        {
            GameObject obj = GameObject.Find("PrevFileButton");
            if (obj != null) prevFileButton = obj.GetComponent<Button>();
        }
        
        if (nextFileButton == null)
        {
            GameObject obj = GameObject.Find("NextFileButton");
            if (obj != null) nextFileButton = obj.GetComponent<Button>();
        }
        
        if (selectFileText == null)
        {
            GameObject obj = GameObject.Find("SelectFileText");
            if (obj != null) selectFileText = obj.GetComponent<Text>();
        }
        
        if (progressText == null)
        {
            GameObject obj = GameObject.Find("ProgressText");
            if (obj != null) progressText = obj.GetComponent<Text>();
        }
        
        if (progressSlider == null)
        {
            GameObject obj = GameObject.Find("ProgressSlider");
            if (obj != null) progressSlider = obj.GetComponent<Slider>();
        }
        
        Debug.Log($"ChallengeSceneManager: UI引用状态 - SelectFileButton: {selectFileButton != null}, StartButton: {startButton != null}, BackButton: {backButton != null}, ProgressText: {progressText != null}, ProgressSlider: {progressSlider != null}");
    }
    
    void FindUIElements()
    {
        // 查找按钮
        if (selectFileButton == null)
        {
            GameObject selectFileObj = GameObject.Find("SelectFileButton");
            if (selectFileObj != null)
                selectFileButton = selectFileObj.GetComponent<Button>();
        }
        
        if (startButton == null)
        {
            GameObject startObj = GameObject.Find("StartButton");
            if (startObj != null)
                startButton = startObj.GetComponent<Button>();
        }
        
        if (backButton == null)
        {
            GameObject backObj = GameObject.Find("BackButton");
            if (backObj != null)
                backButton = backObj.GetComponent<Button>();
        }
        
        if (prevFileButton == null)
        {
            GameObject prevObj = GameObject.Find("PrevFileButton");
            if (prevObj != null)
                prevFileButton = prevObj.GetComponent<Button>();
        }
        
        if (nextFileButton == null)
        {
            GameObject nextObj = GameObject.Find("NextFileButton");
            if (nextObj != null)
                nextFileButton = nextObj.GetComponent<Button>();
        }
        
        // 查找文本组件
        if (selectFileText == null)
        {
            GameObject selectTextObj = GameObject.Find("SelectFileText");
            if (selectTextObj != null)
                selectFileText = selectTextObj.GetComponent<Text>();
        }
        
        if (progressText == null)
        {
            GameObject progressTextObj = GameObject.Find("ProgressText");
            if (progressTextObj != null)
                progressText = progressTextObj.GetComponent<Text>();
        }
        
        // 查找进度相关组件
        if (progressSlider == null)
        {
            GameObject progressObj = GameObject.Find("ProgressSlider");
            if (progressObj != null)
                progressSlider = progressObj.GetComponent<Slider>();
        }
        
        Debug.Log($"ChallengeSceneManager: 找到UI元素 - SelectFileButton: {selectFileButton != null}, StartButton: {startButton != null}");
    }
    
    public void OnSelectFileClicked()
    {
        Debug.Log("ChallengeSceneManager: 选择文件按钮被点击");
        
        // 直接加载当前选中的文件
        if (availableFiles != null && availableFiles.Length > 0)
        {
            LoadCurrentFile();
        }
        else
        {
            Debug.LogWarning("ChallengeSceneManager: 没有可用的乐谱文件");
            if (selectFileText != null)
                selectFileText.text = "没有可用的乐谱文件";
        }
    }
    
    void InitializeFileList()
    {
        try
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Debug.LogWarning("ChallengeSceneManager: StreamingAssets文件夹不存在");
                availableFiles = new string[0];
                return;
            }
            
            // 查找所有txt文件
            availableFiles = Directory.GetFiles(Application.streamingAssetsPath, "*.txt");
            currentFileIndex = 0;
            
            Debug.Log($"ChallengeSceneManager: 找到 {availableFiles.Length} 个txt文件");
            
            // 更新按钮状态
            UpdateNavigationButtons();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChallengeSceneManager: 初始化文件列表时出错 - {e.Message}");
            availableFiles = new string[0];
        }
    }
    
    void LoadCurrentFile()
    {
        if (availableFiles == null || availableFiles.Length == 0)
        {
            Debug.LogWarning("ChallengeSceneManager: 没有可用的文件");
            return;
        }
        
        if (currentFileIndex < 0 || currentFileIndex >= availableFiles.Length)
        {
            currentFileIndex = 0;
        }
        
        selectedFilePath = availableFiles[currentFileIndex];
        string fileName = Path.GetFileNameWithoutExtension(selectedFilePath);
        
        if (selectFileText != null)
            selectFileText.text = $"{fileName} ({currentFileIndex + 1}/{availableFiles.Length})";
            
        // 加载并解析选择的乐谱文件
        LoadMusicSheet(selectedFilePath);
        
        // 更新按钮状态
        UpdateNavigationButtons();
        
        Debug.Log($"ChallengeSceneManager: 加载文件 {fileName} (索引: {currentFileIndex})");
    }
    
    void UpdateNavigationButtons()
    {
        if (availableFiles == null || availableFiles.Length <= 1)
        {
            // 如果只有一个或没有文件，禁用导航按钮
            if (prevFileButton != null)
                prevFileButton.interactable = false;
            if (nextFileButton != null)
                nextFileButton.interactable = false;
        }
        else
        {
            // 启用导航按钮
            if (prevFileButton != null)
                prevFileButton.interactable = true;
            if (nextFileButton != null)
                nextFileButton.interactable = true;
        }
    }
    
    public void OnPrevFileClicked()
    {
        Debug.Log("ChallengeSceneManager: 上一个文件按钮被点击");
        
        if (availableFiles == null || availableFiles.Length == 0)
            return;
            
        currentFileIndex--;
        if (currentFileIndex < 0)
            currentFileIndex = availableFiles.Length - 1; // 循环到最后一个文件
            
        LoadCurrentFile();
    }
    
    public void OnNextFileClicked()
    {
        Debug.Log("ChallengeSceneManager: 下一个文件按钮被点击");
        
        if (availableFiles == null || availableFiles.Length == 0)
            return;
            
        currentFileIndex++;
        if (currentFileIndex >= availableFiles.Length)
            currentFileIndex = 0; // 循环到第一个文件
            
        LoadCurrentFile();
    }
    
    void LoadFromStreamingAssets()
    {
        try
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Debug.LogWarning("ChallengeSceneManager: StreamingAssets文件夹不存在");
                if (selectFileText != null)
                    selectFileText.text = "未找到StreamingAssets文件夹";
                return;
            }
            
            // 查找txt文件
            string[] musicFiles = Directory.GetFiles(Application.streamingAssetsPath, "*.txt");
            
            if (musicFiles.Length > 0)
            {
                selectedFilePath = musicFiles[0]; // 选择第一个找到的文件
                string fileName = Path.GetFileNameWithoutExtension(selectedFilePath);
                
                if (selectFileText != null)
                    selectFileText.text = $"{fileName}";
                    
                // 尝试加载乐谱
                LoadMusicSheet(selectedFilePath);
                
                Debug.Log($"ChallengeSceneManager: 从StreamingAssets选择了文件 {fileName}");
            }
            else
            {
                Debug.LogWarning("ChallengeSceneManager: StreamingAssets中未找到txt乐谱文件");
                if (selectFileText != null)
                    selectFileText.text = "未找到txt乐谱文件";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChallengeSceneManager: 加载StreamingAssets文件时出错 - {e.Message}");
            if (selectFileText != null)
                selectFileText.text = "加载出错";
        }
    }
    
    void LoadMusicSheet(string filePath)
    {
        try
        {
            Debug.Log($"ChallengeSceneManager: 正在加载乐谱 {filePath}");
            
            // 使用MusicSheetParser解析乐谱文件
            selectedMusicSheet = MusicSheetParser.Instance.ParseMusicSheet(filePath);
            
            if (selectedMusicSheet != null)
            {
                Debug.Log($"ChallengeSceneManager: 成功加载乐谱 {selectedMusicSheet.fileName}");
                
                // 启用开始挑战按钮
                if (startButton != null)
                    startButton.interactable = true;
            }
            else
            {
                Debug.LogError("ChallengeSceneManager: 乐谱解析失败");
                selectedFilePath = null;
                
                if (selectFileText != null)
                    selectFileText.text = "解析失败";
                    
                if (startButton != null)
                    startButton.interactable = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChallengeSceneManager: 加载乐谱失败 - {e.Message}");
            selectedMusicSheet = null;
            selectedFilePath = null;
            
            if (selectFileText != null)
                selectFileText.text = "加载失败";
                
            if (startButton != null)
                startButton.interactable = false;
        }
    }
    
    public void OnStartChallengeClicked()
    {
        Debug.Log("ChallengeSceneManager: 开始挑战按钮被点击");
        
        if (selectedMusicSheet != null)
        {
            // 将选中的乐谱数据传递给ChallengeDataManager
            if (ChallengeDataManager.Instance != null)
            {
                ChallengeDataManager.Instance.SetSelectedMusicSheet(selectedMusicSheet);
                Debug.Log("ChallengeSceneManager: 乐谱数据已设置");
            }
            
            // 设置游戏模式为挑战模式
            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.SetChallengeMode(selectedMusicSheet);
                Debug.Log("ChallengeSceneManager: 已设置为挑战模式");
            }
            
            // 跳转到SampleScene开始挑战
            Debug.Log("ChallengeSceneManager: 正在跳转到SampleScene");
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.LogWarning("ChallengeSceneManager: 请先选择一个有效的音乐文件");
        }
    }
    
    // 退出游戏
    private void QuitGame()
    {
        Debug.Log("ChallengeSceneManager: 检测到ESC键，退出游戏");
        
        // 退出应用程序
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // 返回主菜单
    public void OnBackToMenuClicked()
    {
        Debug.Log("ChallengeSceneManager: 返回主菜单");
        SceneManager.LoadScene("TitleScene");
    }
}