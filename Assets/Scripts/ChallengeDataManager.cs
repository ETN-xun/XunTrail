using System.Collections.Generic;
using UnityEngine;

public class ChallengeDataManager : MonoBehaviour
{
    public static ChallengeDataManager Instance { get; private set; }
    
    [Header("挑战数据")]
    public List<MusicSheet> availableMusicSheets = new List<MusicSheet>();
    private MusicSheet selectedMusicSheet;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ChallengeDataManager 初始化完成");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadAvailableMusicSheets();
    }
    
    public void LoadAvailableMusicSheets()
    {
        // 这里可以从文件或Resources加载可用的音乐文件列表
        Debug.Log("ChallengeDataManager: 加载可用音乐文件列表");
    }
    
    public MusicSheet GetMusicSheet(string fileName)
    {
        foreach (var sheet in availableMusicSheets)
        {
            if (sheet.name == fileName)
                return sheet;
        }
        return null;
    }
    
    public void SetSelectedMusicSheet(MusicSheet musicSheet)
    {
        selectedMusicSheet = musicSheet;
        Debug.Log($"ChallengeDataManager: 设置选中的乐谱 - {musicSheet?.name ?? "null"}");
    }
    
    public MusicSheet GetSelectedMusicSheet()
    {
        return selectedMusicSheet;
    }
}

[System.Serializable]
public class ChallengeSettings
{
    public float timeLimit = 60f;
    public int targetScore = 1000;
    public bool enableHints = true;
}