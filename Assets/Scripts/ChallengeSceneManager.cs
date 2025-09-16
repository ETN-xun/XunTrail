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
        
        // 绑定按钮事件
        if (selectFileButton != null)
            selectFileButton.onClick.AddListener(OnSelectFileClicked);
            
        if (startButton != null)
            startButton.onClick.AddListener(OnStartChallengeClicked);
            
        // 初始状态下禁用开始按钮
        if (startButton != null)
            startButton.interactable = false;
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
        
        // 查找文本组件
        if (selectFileText == null)
        {
            GameObject selectTextObj = GameObject.Find("SelectFileText");
            if (selectTextObj != null)
                selectFileText = selectTextObj.GetComponent<Text>();
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
        
        // 使用简单的文件选择逻辑
        // 在实际项目中，这里应该打开文件选择对话框
        string[] musicFiles = Directory.GetFiles(Application.streamingAssetsPath, "*.json");
        
        if (musicFiles.Length > 0)
        {
            selectedFilePath = musicFiles[0]; // 选择第一个找到的文件
            string fileName = Path.GetFileNameWithoutExtension(selectedFilePath);
            
            if (selectFileText != null)
                selectFileText.text = $"已选择: {fileName}";
                
            // 尝试加载乐谱
            LoadMusicSheet(selectedFilePath);
            
            Debug.Log($"ChallengeSceneManager: 选择了文件 {fileName}");
        }
        else
        {
            Debug.LogWarning("ChallengeSceneManager: 未找到音乐文件");
            if (selectFileText != null)
                selectFileText.text = "未找到音乐文件";
        }
    }
    
    void LoadMusicSheet(string filePath)
    {
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            selectedMusicSheet = JsonUtility.FromJson<MusicSheet>(jsonContent);
            
            if (selectedMusicSheet != null && selectedMusicSheet.notes != null && selectedMusicSheet.notes.Count > 0)
            {
                // 启用开始按钮
                if (startButton != null)
                    startButton.interactable = true;
                    
                Debug.Log($"ChallengeSceneManager: 成功加载乐谱，包含 {selectedMusicSheet.notes.Count} 个音符");
            }
            else
            {
                Debug.LogWarning("ChallengeSceneManager: 乐谱数据无效");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChallengeSceneManager: 加载乐谱失败 - {e.Message}");
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