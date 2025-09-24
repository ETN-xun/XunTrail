using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 键位绑定测试辅助脚本
/// 用于验证十孔模式键位修改功能是否正常工作
/// </summary>
public class KeyBindingTestHelper : MonoBehaviour
{
    [Header("测试设置")]
    public bool enableDebugOutput = true;
    public KeyCode testKey = KeyCode.T; // 用于触发测试的键位
    
    private ToneGenerator toneGenerator;
    private KeySettingsManager keySettingsManager;
    
    void Start()
    {
        toneGenerator = FindObjectOfType<ToneGenerator>();
        keySettingsManager = KeySettingsManager.Instance;
        
        if (enableDebugOutput)
        {
            Debug.Log("[键位测试] 测试辅助脚本已启动");
            Debug.Log("[键位测试] 按 T 键进行键位状态测试");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            PerformKeyBindingTest();
        }
    }
    
    /// <summary>
    /// 执行键位绑定测试
    /// </summary>
    public void PerformKeyBindingTest()
    {
        if (!enableDebugOutput) return;
        
        Debug.Log("=== 键位绑定测试开始 ===");
        
        // 测试1: 检查当前键位设置
        TestCurrentKeySettings();
        
        // 测试2: 检查ToneGenerator的键位状态
        TestToneGeneratorKeyStates();
        
        // 测试3: 模拟键位修改
        TestKeyModification();
        
        Debug.Log("=== 键位绑定测试结束 ===");
    }
    
    private void TestCurrentKeySettings()
    {
        Debug.Log("--- 测试1: 当前键位设置 ---");
        
        var eightHoleKeys = keySettingsManager.GetEightHoleKeys();
        var tenHoleKeys = keySettingsManager.GetTenHoleKeys();
        
        Debug.Log($"八孔键位: {string.Join(", ", eightHoleKeys)}");
        Debug.Log($"十孔键位: {string.Join(", ", tenHoleKeys)}");
    }
    
    private void TestToneGeneratorKeyStates()
    {
        Debug.Log("--- 测试2: ToneGenerator键位状态 ---");
        
        if (toneGenerator == null)
        {
            Debug.LogError("未找到ToneGenerator实例！");
            return;
        }
        
        // 通过反射访问私有字段_keyStates
        var keyStatesField = typeof(ToneGenerator).GetField("_keyStates", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (keyStatesField != null)
        {
            var keyStates = keyStatesField.GetValue(toneGenerator) as Dictionary<KeyCode, bool>;
            if (keyStates != null)
            {
                Debug.Log($"ToneGenerator中已注册的键位数量: {keyStates.Count}");
                
                // 显示前10个键位作为示例
                int count = 0;
                foreach (var kvp in keyStates)
                {
                    if (count >= 10) break;
                    Debug.Log($"  {kvp.Key}: {kvp.Value}");
                    count++;
                }
                
                if (keyStates.Count > 10)
                {
                    Debug.Log($"  ... 还有 {keyStates.Count - 10} 个键位");
                }
            }
            else
            {
                Debug.LogError("_keyStates字典为空！");
            }
        }
        else
        {
            Debug.LogError("无法访问_keyStates字段！");
        }
    }
    
    private void TestKeyModification()
    {
        Debug.Log("--- 测试3: 模拟键位修改 ---");
        
        // 获取当前十孔键位
        var originalKeys = keySettingsManager.GetTenHoleKeys();
        Debug.Log($"修改前十孔键位: {string.Join(", ", originalKeys)}");
        
        // 创建一个修改后的键位数组（将第一个键位改为F1）
        var modifiedKeys = new KeyCode[originalKeys.Length];
        System.Array.Copy(originalKeys, modifiedKeys, originalKeys.Length);
        modifiedKeys[0] = KeyCode.F1;
        
        Debug.Log($"模拟修改后键位: {string.Join(", ", modifiedKeys)}");
        
        // 保存修改后的键位
        keySettingsManager.SetTenHoleKeys(modifiedKeys);
        
        // 通知ToneGenerator重新加载
        if (toneGenerator != null)
        {
            toneGenerator.LoadDynamicKeySettings();
            Debug.Log("已通知ToneGenerator重新加载键位设置");
        }
        
        // 验证修改是否生效
        var newKeys = keySettingsManager.GetTenHoleKeys();
        Debug.Log($"验证修改后键位: {string.Join(", ", newKeys)}");
        
        // 恢复原始键位
        keySettingsManager.SetTenHoleKeys(originalKeys);
        if (toneGenerator != null)
        {
            toneGenerator.LoadDynamicKeySettings();
        }
        Debug.Log("已恢复原始键位设置");
    }
}