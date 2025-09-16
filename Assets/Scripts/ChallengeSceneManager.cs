using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class ChallengeSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public Button selectFileButton;
    public Button startButton;
    [SerializeField] public Button backButton;
    public Text selectFileText;
    public Text progressText;
    public Slider progressSlider;
    
    [Header("Challenge Settings")]
    private string selectedFilePath = "";
    private MusicSheet selectedMusicSheet;
    
    void Start()
    {
        // 自动查找UI元素
        FindUIElements();
        
        // 强制分配UI引用（解决序列化问题）
        ForceAssignUIReferences();
        
        // 绑定按钮事件
        if (selectFileButton != null)
            selectFileButton.onClick.AddListener(OnSelectFileClicked);
        
        if (startButton != null)
            startButton.onClick.AddListener(OnStartChallengeClicked);
            
        if (backButton != null)
            backButton.onClick.AddListener(OnBackToMenuClicked);
        
        // 初始状态：禁用开始按钮
        if (startButton != null)
            startButton.interactable = false;
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
        
        // 尝试打开文件选择对话框
        string selectedFile = FileSelector.OpenFileDialog("选择乐谱文件", "文本文件\0*.txt\0所有文件\0*.*\0");
        
        if (!string.IsNullOrEmpty(selectedFile))
        {
            if (FileSelector.IsValidMusicSheetFile(selectedFile))
        {
            // 用户选择了有效的外部文件
            selectedFilePath = selectedFile;
            string fileName = Path.GetFileNameWithoutExtension(selectedFile);
            
            if (selectFileText != null)
                selectFileText.text = $"已选择: {fileName}";
                
            // 加载并解析选择的乐谱文件
            LoadMusicSheet(selectedFile);
                
            Debug.Log($"ChallengeSceneManager: 选择了外部文件 {fileName}");
            return;
            }
            else
            {
                // 选择的文件无效
                Debug.LogWarning($"ChallengeSceneManager: 选择的文件不是有效的乐谱文件: {selectedFile}");
                if (selectFileText != null)
                    selectFileText.text = "选择的文件格式无效";
                return;
            }
        }
        else
        {
            // 用户取消了文件选择
            Debug.Log("ChallengeSceneManager: 用户取消了文件选择");
            if (selectFileText != null)
                selectFileText.text = "未选择文件";
        }
        
        // 如果没有选择外部文件，则从StreamingAssets加载
        if (ChallengeDataManager.Instance != null)
        {
            var availableSheets = ChallengeDataManager.Instance.availableMusicSheets;
            
            if (availableSheets != null && availableSheets.Count > 0)
            {
                // 使用第一个可用的乐谱
                selectedMusicSheet = availableSheets[0];
                selectedFilePath = Path.Combine(Application.streamingAssetsPath, selectedMusicSheet.fileName + ".txt");
                
                if (selectFileText != null)
                    selectFileText.text = $"已选择: {selectedMusicSheet.fileName}";
                    
                // 启用开始按钮
                if (startButton != null)
                    startButton.interactable = true;
                    
                Debug.Log($"ChallengeSceneManager: 从StreamingAssets选择了文件 {selectedMusicSheet.fileName}");
                return;
            }
        }
        
        // 如果ChallengeDataManager不可用，直接查找StreamingAssets中的txt文件
        LoadFromStreamingAssets();
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
                    selectFileText.text = $"已选择: {fileName}";
                    
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
            // 启动挑战
            if (ChallengeManager.Instance != null)
            {
                ChallengeManager.Instance.StartChallenge(selectedMusicSheet);
                Debug.Log("ChallengeSceneManager: 挑战已启动");
            }
            else
            {
                Debug.LogError("ChallengeSceneManager: 未找到ChallengeManager实例");
            }
        }
        else
        {
            Debug.LogWarning("ChallengeSceneManager: 请先选择一个有效的音乐文件");
        }
    }
    
    // 返回主菜单
    public void OnBackToMenuClicked()
    {
        Debug.Log("ChallengeSceneManager: 返回主菜单");
        SceneManager.LoadScene("TitleScene");
    }
}