using UnityEngine;
using UnityEngine.UI;

// 游戏状态接口
public interface IGameState
{
    void Enter();
    void Update();
    void Exit();
}

// 自由模式状态
public class FreeModeState : IGameState
{
    private GameModeManager manager;
    
    public FreeModeState(GameModeManager manager)
    {
        this.manager = manager;
    }
    
    public void Enter()
    {
        Debug.Log("进入自由模式状态");
        // 在这里可以添加进入自由模式时的UI设置
    }
    
    public void Update()
    {
        // 自由模式的更新逻辑
    }
    
    public void Exit()
    {
        Debug.Log("退出自由模式状态");
    }
}

// 挑战模式状态
public class ChallengeModeState : IGameState
{
    private GameModeManager manager;
    
    public ChallengeModeState(GameModeManager manager)
    {
        this.manager = manager;
    }
    
    public void Enter()
    {
        Debug.Log("进入挑战模式状态");
        // 在这里可以添加进入挑战模式时的UI设置
    }
    
    public void Update()
    {
        // 挑战模式的更新逻辑
    }
    
    public void Exit()
    {
        Debug.Log("退出挑战模式状态");
    }
}

// 教程模式状态（部署状态）
public class TutorialModeState : IGameState
{
    private GameModeManager manager;
    private GameObject tutorialUI;
    
    public TutorialModeState(GameModeManager manager)
    {
        this.manager = manager;
    }
    
    public void Enter()
    {
        Debug.Log("进入教程模式状态（部署状态）");
        
        // 显示教程UI
        ShowTutorialUI();
        
        // 可以在这里添加其他进入教程模式时需要的设置
        SetupTutorialEnvironment();
    }
    
    public void Update()
    {
        // 教程模式的更新逻辑
        // 可以在这里检查教程进度等
    }
    
    public void Exit()
    {
        Debug.Log("退出教程模式状态");
        
        // 隐藏教程UI
        HideTutorialUI();
    }
    
    private void ShowTutorialUI()
    {
        // 查找或创建教程UI
        tutorialUI = GameObject.Find("TutorialUI");
        if (tutorialUI == null)
        {
            // 如果没有找到专门的TutorialUI，可以查找其他UI元素
            GameObject challengeUI = GameObject.Find("ChallengeUI");
            if (challengeUI != null)
            {
                challengeUI.SetActive(true);
                Debug.Log("显示教程模式UI（使用ChallengeUI）");
                
                // 更新UI文本显示教程信息
                Text progressText = challengeUI.transform.Find("ProgressText")?.GetComponent<Text>();
                if (progressText != null)
                {
                    progressText.text = "教程模式 - 正在部署学习环境...";
                }
                
                Text upcomingNotesText = challengeUI.transform.Find("UpcomingNotesText")?.GetComponent<Text>();
                if (upcomingNotesText != null)
                {
                    upcomingNotesText.text = "欢迎来到新手教程！请按照指示进行操作。";
                }
            }
        }
        else
        {
            tutorialUI.SetActive(true);
            Debug.Log("显示专用教程UI");
        }
    }
    
    private void HideTutorialUI()
    {
        if (tutorialUI != null)
        {
            tutorialUI.SetActive(false);
            Debug.Log("隐藏教程UI");
        }
    }
    
    private void SetupTutorialEnvironment()
    {
        // 设置教程环境，比如禁用某些功能、设置特定的游戏参数等
        Debug.Log("设置教程环境");
        
        // 可以在这里添加教程特有的设置
        // 例如：限制某些按键、显示提示信息等
    }
}

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
    
    // 状态机相关
    private IGameState currentState;
    private FreeModeState freeModeState;
    private ChallengeModeState challengeModeState;
    private TutorialModeState tutorialModeState;
    
    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameModeManager 初始化完成");
            
            // 初始化状态
            InitializeStates();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        // 调用当前状态的Update方法
        currentState?.Update();
    }
    
    private void InitializeStates()
    {
        // 创建所有状态实例
        freeModeState = new FreeModeState(this);
        challengeModeState = new ChallengeModeState(this);
        tutorialModeState = new TutorialModeState(this);
        
        // 设置初始状态
        ChangeState(freeModeState);
    }
    
    private void ChangeState(IGameState newState)
    {
        // 退出当前状态
        currentState?.Exit();
        
        // 切换到新状态
        currentState = newState;
        
        // 进入新状态
        currentState?.Enter();
    }
    
    public void SetChallengeMode(MusicSheet musicSheet)
    {
        currentMode = GameMode.Challenge;
        selectedMusicSheet = musicSheet;
        Debug.Log($"设置为挑战模式，乐谱: {musicSheet?.name}");
        
        // 切换到挑战模式状态
        ChangeState(challengeModeState);
    }
    
    public void SetFreeMode()
    {
        currentMode = GameMode.Free;
        selectedMusicSheet = null;
        Debug.Log("设置为自由模式");
        
        // 切换到自由模式状态
        ChangeState(freeModeState);
    }
    
    public void SetTutorialMode()
    {
        currentMode = GameMode.Tutorial;
        selectedMusicSheet = null;
        Debug.Log("设置为教程模式");
        
        // 切换到教程模式状态（部署状态）
        ChangeState(tutorialModeState);
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
    
    // 获取当前状态（用于调试或其他需要）
    public IGameState GetCurrentState()
    {
        return currentState;
    }
}