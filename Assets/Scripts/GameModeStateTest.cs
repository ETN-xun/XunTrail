using UnityEngine;

/// <summary>
/// 游戏模式状态机测试脚本
/// 用于测试GameModeManager的状态切换和UI显示功能
/// </summary>
public class GameModeStateTest : MonoBehaviour
{
    [Header("测试设置")]
    [Tooltip("是否启用键盘测试")]
    public bool enableKeyboardTest = true;
    
    [Tooltip("测试间隔时间（秒）")]
    public float testInterval = 3f;
    
    private float lastTestTime;
    private int currentTestMode = 0;
    
    void Start()
    {
        Debug.Log("GameModeStateTest: 状态机测试脚本已启动");
        Debug.Log("按键测试说明：");
        Debug.Log("F1 - 切换到自由模式");
        Debug.Log("F2 - 切换到挑战模式");
        Debug.Log("F3 - 切换到教程模式（部署状态）");
        Debug.Log("F4 - 显示当前状态信息");
    }
    
    void Update()
    {
        if (enableKeyboardTest)
        {
            HandleKeyboardInput();
        }
        
        // 自动循环测试（可选）
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T))
        {
            StartAutomaticTest();
        }
    }
    
    private void HandleKeyboardInput()
    {
        // F1 - 自由模式
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TestFreeMode();
        }
        
        // F2 - 挑战模式
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TestChallengeMode();
        }
        
        // F3 - 教程模式
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestTutorialMode();
        }
        
        // F4 - 显示状态信息
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ShowCurrentStateInfo();
        }
    }
    
    private void TestFreeMode()
    {
        Debug.Log("=== 测试自由模式 ===");
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetFreeMode();
            Debug.Log("已切换到自由模式");
        }
        else
        {
            Debug.LogError("GameModeManager.Instance 为 null");
        }
    }
    
    private void TestChallengeMode()
    {
        Debug.Log("=== 测试挑战模式 ===");
        if (GameModeManager.Instance != null)
        {
            // 创建一个测试用的MusicSheet（如果需要的话）
            GameModeManager.Instance.SetChallengeMode(null);
            Debug.Log("已切换到挑战模式");
        }
        else
        {
            Debug.LogError("GameModeManager.Instance 为 null");
        }
    }
    
    private void TestTutorialMode()
    {
        Debug.Log("=== 测试教程模式（部署状态） ===");
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetTutorialMode();
            Debug.Log("已切换到教程模式，应该显示部署UI");
        }
        else
        {
            Debug.LogError("GameModeManager.Instance 为 null");
        }
    }
    
    private void ShowCurrentStateInfo()
    {
        Debug.Log("=== 当前状态信息 ===");
        if (GameModeManager.Instance != null)
        {
            Debug.Log($"当前游戏模式: {GameModeManager.Instance.currentMode}");
            Debug.Log($"是否为自由模式: {GameModeManager.Instance.IsFreeMode()}");
            Debug.Log($"是否为挑战模式: {GameModeManager.Instance.IsChallengeMode()}");
            Debug.Log($"是否为教程模式: {GameModeManager.Instance.IsTutorialMode()}");
            
            IGameState currentState = GameModeManager.Instance.GetCurrentState();
            if (currentState != null)
            {
                Debug.Log($"当前状态类型: {currentState.GetType().Name}");
            }
            else
            {
                Debug.LogWarning("当前状态为 null");
            }
        }
        else
        {
            Debug.LogError("GameModeManager.Instance 为 null");
        }
    }
    
    private void StartAutomaticTest()
    {
        Debug.Log("=== 开始自动循环测试 ===");
        InvokeRepeating(nameof(CycleThroughModes), 1f, testInterval);
    }
    
    private void CycleThroughModes()
    {
        switch (currentTestMode)
        {
            case 0:
                TestFreeMode();
                break;
            case 1:
                TestChallengeMode();
                break;
            case 2:
                TestTutorialMode();
                break;
        }
        
        currentTestMode = (currentTestMode + 1) % 3;
        
        // 显示状态信息
        Invoke(nameof(ShowCurrentStateInfo), 0.5f);
    }
    
    void OnGUI()
    {
        if (!enableKeyboardTest) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("游戏模式状态机测试");
        GUILayout.Space(10);
        
        if (GUILayout.Button("自由模式 (F1)"))
        {
            TestFreeMode();
        }
        
        if (GUILayout.Button("挑战模式 (F2)"))
        {
            TestChallengeMode();
        }
        
        if (GUILayout.Button("教程模式 (F3)"))
        {
            TestTutorialMode();
        }
        
        if (GUILayout.Button("显示状态信息 (F4)"))
        {
            ShowCurrentStateInfo();
        }
        
        GUILayout.Space(10);
        if (GUILayout.Button("自动测试 (Shift+T)"))
        {
            StartAutomaticTest();
        }
        
        if (GUILayout.Button("停止自动测试"))
        {
            CancelInvoke();
        }
        
        GUILayout.EndArea();
    }
}