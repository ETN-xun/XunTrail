using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }
    
    public enum GameMode
    {
        Free,
        Challenge,
        Tutorial
    }
    
    [Header("游戏模式设置")]
    public GameMode currentMode = GameMode.Free;
    public MusicSheet selectedMusicSheet;
    
    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameModeManager 初始化完成");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetChallengeMode(MusicSheet musicSheet)
    {
        currentMode = GameMode.Challenge;
        selectedMusicSheet = musicSheet;
        Debug.Log($"设置为挑战模式，乐谱: {musicSheet?.name}");
    }
    
    public void SetFreeMode()
    {
        currentMode = GameMode.Free;
        selectedMusicSheet = null;
        Debug.Log("设置为自由模式");
    }
    
    public void SetTutorialMode()
    {
        currentMode = GameMode.Tutorial;
        selectedMusicSheet = null;
        Debug.Log("设置为教程模式");
    }
    
    public bool IsChallengeMode()
    {
        return currentMode == GameMode.Challenge;
    }
    
    public bool IsFreeMode()
    {
        return currentMode == GameMode.Free;
    }
    
    public bool IsTutorialMode()
    {
        return currentMode == GameMode.Tutorial;
    }
}