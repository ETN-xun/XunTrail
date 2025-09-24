using UnityEngine;
using System.Reflection;

public class BreathKeyTestHelper : MonoBehaviour
{
    [Header("测试控制")]
    public KeyCode testBreathKey = KeyCode.B; // 测试用的新吹气键
    
    void Start()
    {
        Debug.Log("=== 八孔模式吹气键修改测试开始 ===");
        
        // 等待一帧确保所有组件初始化完成
        Invoke("RunTest", 0.1f);
    }
    
    void RunTest()
    {
        TestCurrentBreathKey();
        TestBreathKeyModification();
    }
    
    void TestCurrentBreathKey()
    {
        Debug.Log("--- 测试当前吹气键设置 ---");
        
        var keySettingsManager = KeySettingsManager.Instance;
        if (keySettingsManager == null)
        {
            Debug.LogError("KeySettingsManager未找到！");
            return;
        }
        
        var eightHoleKeys = keySettingsManager.GetEightHoleKeys();
        Debug.Log($"八孔键位: {string.Join(", ", eightHoleKeys)}");
        
        if (eightHoleKeys.Length > 9)
        {
            Debug.Log($"当前吹气键: {eightHoleKeys[9]}");
        }
        else
        {
            Debug.LogError("八孔键位数组长度不足！");
        }
        
        // 测试ToneGenerator的GetBreathKey方法
        var toneGenerator = ToneGenerator.Instance;
        if (toneGenerator != null)
        {
            // 使用反射调用私有方法GetBreathKey
            var getBreathKeyMethod = typeof(ToneGenerator).GetMethod("GetBreathKey", BindingFlags.NonPublic | BindingFlags.Instance);
            if (getBreathKeyMethod != null)
            {
                var breathKey = (KeyCode)getBreathKeyMethod.Invoke(toneGenerator, null);
                Debug.Log($"ToneGenerator检测到的吹气键: {breathKey}");
            }
            else
            {
                Debug.LogError("GetBreathKey方法未找到！");
            }
        }
        else
        {
            Debug.LogError("ToneGenerator实例未找到！");
        }
    }
    
    void TestBreathKeyModification()
    {
        Debug.Log("--- 测试吹气键修改功能 ---");
        
        var keySettingsManager = KeySettingsManager.Instance;
        if (keySettingsManager == null)
        {
            Debug.LogError("KeySettingsManager未找到！");
            return;
        }
        
        // 保存原始设置
        var originalKeys = keySettingsManager.GetEightHoleKeys();
        var originalBreathKey = originalKeys[9];
        Debug.Log($"原始吹气键: {originalBreathKey}");
        
        // 修改吹气键
        var modifiedKeys = (KeyCode[])originalKeys.Clone();
        modifiedKeys[9] = testBreathKey;
        
        Debug.Log($"将吹气键修改为: {testBreathKey}");
        keySettingsManager.SetEightHoleKeys(modifiedKeys);
        
        // 验证修改是否生效
        var newKeys = keySettingsManager.GetEightHoleKeys();
        Debug.Log($"修改后的八孔键位: {string.Join(", ", newKeys)}");
        Debug.Log($"修改后的吹气键: {newKeys[9]}");
        
        // 检查ToneGenerator是否正确更新
        var toneGenerator = ToneGenerator.Instance;
        if (toneGenerator != null)
        {
            // 触发重新加载键位设置
            toneGenerator.LoadDynamicKeySettings();
            
            // 验证ToneGenerator的GetBreathKey方法
            var getBreathKeyMethod = typeof(ToneGenerator).GetMethod("GetBreathKey", BindingFlags.NonPublic | BindingFlags.Instance);
            if (getBreathKeyMethod != null)
            {
                var breathKey = (KeyCode)getBreathKeyMethod.Invoke(toneGenerator, null);
                Debug.Log($"ToneGenerator更新后的吹气键: {breathKey}");
                
                if (breathKey == testBreathKey)
                {
                    Debug.Log("✅ 吹气键修改功能正常工作！");
                }
                else
                {
                    Debug.LogError($"❌ 吹气键修改失败！期望: {testBreathKey}, 实际: {breathKey}");
                }
            }
        }
        
        // 恢复原始设置
        Debug.Log("恢复原始键位设置...");
        keySettingsManager.SetEightHoleKeys(originalKeys);
        if (toneGenerator != null)
        {
            toneGenerator.LoadDynamicKeySettings();
        }
        
        Debug.Log("=== 八孔模式吹气键修改测试完成 ===");
    }
    
    void Update()
    {
        // 实时显示当前按键状态（用于手动测试）
        if (Input.GetKeyDown(testBreathKey))
        {
            Debug.Log($"检测到测试吹气键按下: {testBreathKey}");
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("检测到Space键按下");
        }
    }
}