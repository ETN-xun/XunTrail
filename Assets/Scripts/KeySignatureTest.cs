using UnityEngine;
using UnityEngine.UI;

public class KeySignatureTest : MonoBehaviour
{
    [Header("测试组件")]
    public ToneGenerator toneGenerator;
    public ChallengeManager challengeManager;
    public Text debugText;
    
    [Header("测试参数")]
    public string testNoteName = "C4";
    
    private int lastKey = 0;
    
    private void Start()
    {
        // 自动查找组件
        if (toneGenerator == null)
            toneGenerator = FindObjectOfType<ToneGenerator>();
        if (challengeManager == null)
            challengeManager = FindObjectOfType<ChallengeManager>();
            
        if (debugText != null)
        {
            debugText.text = "调号变化调试器已启动\n按左右箭头键改变调号";
        }
    }
    
    private void Update()
    {
        if (toneGenerator != null && challengeManager != null)
        {
            int currentKey = toneGenerator.key;
            
            // 检测调号变化
            if (currentKey != lastKey)
            {
                lastKey = currentKey;
                UpdateDebugInfo();
            }
            
            // 按T键手动更新调试信息
            if (Input.GetKeyDown(KeyCode.T))
            {
                UpdateDebugInfo();
            }
        }
    }
    
    private void UpdateDebugInfo()
    {
        if (toneGenerator == null || challengeManager == null)
            return;
            
        int currentKey = challengeManager.GetCurrentKey();
        string solfegeName = challengeManager.ConvertToSolfege(testNoteName, currentKey);
        
        string debugInfo = $"当前调号: {currentKey}\n";
        debugInfo += $"测试音符: {testNoteName}\n";
        debugInfo += $"简谱音名: {solfegeName}\n";
        debugInfo += $"ToneGenerator.key: {toneGenerator.key}\n";
        debugInfo += "按左右箭头键改变调号";
        
        Debug.Log($"调号变化: {currentKey}, {testNoteName} -> {solfegeName}");
        
        if (debugText != null)
        {
            debugText.text = debugInfo;
        }
    }
}