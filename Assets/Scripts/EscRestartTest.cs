using UnityEngine;
using UnityEngine.UI;

public class EscRestartTest : MonoBehaviour
{
    [Header("测试UI")]
    public Text statusText;
    public Text instructionText;
    public Button testButton;
    
    private float testStartTime;
    private bool testActive = false;
    
    void Start()
    {
        // 自动查找UI元素
        if (statusText == null)
            statusText = GameObject.Find("StatusText")?.GetComponent<Text>();
        if (instructionText == null)
            instructionText = GameObject.Find("InstructionText")?.GetComponent<Text>();
        if (testButton == null)
            testButton = GameObject.Find("TestButton")?.GetComponent<Button>();
        
        // 设置初始UI
        UpdateUI();
        
        // 绑定测试按钮
        if (testButton != null)
            testButton.onClick.AddListener(StartTest);
    }
    
    void Update()
    {
        if (testActive)
        {
            // 更新测试状态
            float elapsedTime = Time.time - testStartTime;
            if (statusText != null)
            {
                statusText.text = $"测试进行中... {elapsedTime:F1}秒\n按ESC键测试重启功能";
            }
        }
    }
    
    void UpdateUI()
    {
        if (statusText != null)
        {
            statusText.text = "ESC键重启功能测试\n状态: 准备就绪";
        }
        
        if (instructionText != null)
        {
            instructionText.text = "点击'开始测试'按钮，然后按ESC键测试重启功能";
        }
    }
    
    public void StartTest()
    {
        testActive = true;
        testStartTime = Time.time;
        
        Debug.Log("ESC重启功能测试开始");
        
        if (statusText != null)
        {
            statusText.text = "测试开始！\n现在按ESC键测试重启功能";
        }
        
        // 检查GameManager是否存在
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager实例不存在！请确保场景中有GameManager对象");
            if (statusText != null)
            {
                statusText.text = "错误: GameManager不存在！\n请在场景中添加GameManager";
            }
        }
        else
        {
            Debug.Log("GameManager实例已找到，ESC重启功能应该可以正常工作");
        }
    }
    
    void OnDestroy()
    {
        Debug.Log("EscRestartTest: 测试脚本被销毁（可能是因为场景重启）");
    }
}