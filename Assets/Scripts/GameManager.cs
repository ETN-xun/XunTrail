using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("重启设置")]
    [Tooltip("是否启用ESC键重启功能")]
    public bool enableEscRestart = true;
    
    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager 初始化完成");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        // 检测ESC键输入
        if (enableEscRestart && Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }
    
    /// <summary>
    /// 重启游戏 - 重新加载当前场景
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("ESC键被按下，正在重启游戏...");
        
        // 获取当前场景名称
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // 清理游戏状态
        CleanupGameState();
        
        // 重新加载当前场景
        SceneManager.LoadScene(currentSceneName);
        
        Debug.Log($"游戏已重启，重新加载场景: {currentSceneName}");
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("ESC键被按下，正在退出游戏...");
        
        // 清理游戏状态
        CleanupGameState();
        
        // 退出应用程序
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        
        Debug.Log("游戏已退出");
    }
    
    /// <summary>
    /// 清理游戏状态
    /// </summary>
    private void CleanupGameState()
    {
        // 重置游戏模式管理器状态
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetFreeMode();
        }
        
        // 清理挑战数据管理器状态
        if (ChallengeDataManager.Instance != null)
        {
            ChallengeDataManager.Instance.ClearSelectedMusicSheet();
        }
        
        // 停止所有音频
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        
        Debug.Log("游戏状态已清理");
    }
    
    /// <summary>
    /// 启用或禁用ESC键重启功能
    /// </summary>
    /// <param name="enable">是否启用</param>
    public void SetEscRestartEnabled(bool enable)
    {
        enableEscRestart = enable;
        Debug.Log($"ESC键重启功能已{(enable ? "启用" : "禁用")}");
    }
    
    /// <summary>
    /// 手动重启游戏的公共方法（可以被UI按钮调用）
    /// </summary>
    public void ManualRestartGame()
    {
        RestartGame();
    }
}