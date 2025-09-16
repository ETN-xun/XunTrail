using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 加载标题场景
    public void LoadTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }
    
    // 加载挑战模式场景
    public void LoadChallengeScene()
    {
        SceneManager.LoadScene("ChallengeScene");
    }
    
    // 加载SampleScene场景
    public void LoadSampleScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
    
    // 退出游戏
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}