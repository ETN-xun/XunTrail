using System.Collections.Generic;
using UnityEngine;

public class ChallengeDataManager : MonoBehaviour
{
    public static ChallengeDataManager Instance { get; private set; }
    
    [Header("挑战数据")]
    public List<MusicSheet> availableMusicSheets = new List<MusicSheet>();
    
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
    
    void LoadAvailableMusicSheets()
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
}

[System.Serializable]
public class ChallengeSettings
{
    public float timeLimit = 60f;
    public int targetScore = 1000;
    public bool enableHints = true;
}