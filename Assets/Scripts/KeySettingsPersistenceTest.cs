using UnityEngine;
using System.Collections;

/// <summary>
/// 键位设置持久化功能测试脚本
/// 测试键位保存、加载、文件备份等功能
/// </summary>
public class KeySettingsPersistenceTest : MonoBehaviour
{
    [Header("测试控制")]
    public bool runTestOnStart = true;
    public bool showDetailedLogs = true;
    
    [Header("测试键位")]
    public KeyCode[] testEightHoleKeys = new KeyCode[] {
        KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R,
        KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I,
        KeyCode.O, KeyCode.P
    };
    
    public KeyCode[] testTenHoleKeys = new KeyCode[] {
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V,
        KeyCode.B, KeyCode.N, KeyCode.M, KeyCode.Comma,
        KeyCode.Period, KeyCode.Slash, KeyCode.RightShift, KeyCode.Return
    };
    
    void Start()
    {
        if (runTestOnStart)
        {
            Debug.Log("=== 键位设置持久化测试开始 ===");
            StartCoroutine(RunAllTests());
        }
    }
    
    IEnumerator RunAllTests()
    {
        yield return new WaitForSeconds(0.5f); // 等待其他组件初始化
        
        // 1. 显示当前设置信息
        TestShowCurrentSettings();
        yield return new WaitForSeconds(0.5f);
        
        // 2. 测试保存功能
        TestSaveSettings();
        yield return new WaitForSeconds(0.5f);
        
        // 3. 测试加载功能
        TestLoadSettings();
        yield return new WaitForSeconds(0.5f);
        
        // 4. 测试文件信息
        TestFileInfo();
        yield return new WaitForSeconds(0.5f);
        
        // 5. 测试重置功能
        TestResetSettings();
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("=== 键位设置持久化测试完成 ===");
    }
    
    void TestShowCurrentSettings()
    {
        Debug.Log("--- 测试1: 显示当前设置 ---");
        
        var keySettingsManager = KeySettingsManager.Instance;
        if (keySettingsManager == null)
        {
            Debug.LogError("KeySettingsManager未找到！");
            return;
        }
        
        var eightHoleKeys = keySettingsManager.GetEightHoleKeys();
        var tenHoleKeys = keySettingsManager.GetTenHoleKeys();
        
        if (showDetailedLogs)
        {
            Debug.Log($"当前八孔键位: {string.Join(", ", eightHoleKeys)}");
            Debug.Log($"当前十孔键位: {string.Join(", ", tenHoleKeys)}");
        }
        
        Debug.Log("✅ 当前设置显示完成");
    }
    
    void TestSaveSettings()
    {
        Debug.Log("--- 测试2: 保存设置功能 ---");
        
        var keySettingsManager = KeySettingsManager.Instance;
        if (keySettingsManager == null)
        {
            Debug.LogError("KeySettingsManager未找到！");
            return;
        }
        
        // 保存原始设置
        var originalEightHole = keySettingsManager.GetEightHoleKeys();
        var originalTenHole = keySettingsManager.GetTenHoleKeys();
        
        // 设置测试键位
        Debug.Log("设置测试键位...");
        keySettingsManager.SetEightHoleKeys(testEightHoleKeys);
        keySettingsManager.SetTenHoleKeys(testTenHoleKeys);
        
        if (showDetailedLogs)
        {
            Debug.Log($"测试八孔键位: {string.Join(", ", testEightHoleKeys)}");
            Debug.Log($"测试十孔键位: {string.Join(", ", testTenHoleKeys)}");
        }
        
        // 验证设置是否生效
        var newEightHole = keySettingsManager.GetEightHoleKeys();
        var newTenHole = keySettingsManager.GetTenHoleKeys();
        
        bool eightHoleMatch = ArraysEqual(newEightHole, testEightHoleKeys);
        bool tenHoleMatch = ArraysEqual(newTenHole, testTenHoleKeys);
        
        if (eightHoleMatch && tenHoleMatch)
        {
            Debug.Log("✅ 键位保存功能正常");
        }
        else
        {
            Debug.LogError("❌ 键位保存功能异常");
        }
        
        // 恢复原始设置
        keySettingsManager.SetEightHoleKeys(originalEightHole);
        keySettingsManager.SetTenHoleKeys(originalTenHole);
    }
    
    void TestLoadSettings()
    {
        Debug.Log("--- 测试3: 加载设置功能 ---");
        
        var keySettingsManager = KeySettingsManager.Instance;
        if (keySettingsManager == null)
        {
            Debug.LogError("KeySettingsManager未找到！");
            return;
        }
        
        // 强制重新加载设置
        keySettingsManager.LoadKeySettings();
        
        var eightHoleKeys = keySettingsManager.GetEightHoleKeys();
        var tenHoleKeys = keySettingsManager.GetTenHoleKeys();
        
        if (eightHoleKeys != null && eightHoleKeys.Length == 10 &&
            tenHoleKeys != null && tenHoleKeys.Length == 12)
        {
            Debug.Log("✅ 键位加载功能正常");
            
            if (showDetailedLogs)
            {
                Debug.Log($"加载的八孔键位: {string.Join(", ", eightHoleKeys)}");
                Debug.Log($"加载的十孔键位: {string.Join(", ", tenHoleKeys)}");
            }
        }
        else
        {
            Debug.LogError("❌ 键位加载功能异常");
        }
    }
    
    void TestFileInfo()
    {
        Debug.Log("--- 测试4: 文件信息功能 ---");
        
        var keySettingsManager = KeySettingsManager.Instance;
        if (keySettingsManager == null)
        {
            Debug.LogError("KeySettingsManager未找到！");
            return;
        }
        
        string fileInfo = keySettingsManager.GetSettingsInfo();
        Debug.Log(fileInfo);
        
        Debug.Log("✅ 文件信息显示完成");
    }
    
    void TestResetSettings()
    {
        Debug.Log("--- 测试5: 重置设置功能 ---");
        
        var keySettingsManager = KeySettingsManager.Instance;
        if (keySettingsManager == null)
        {
            Debug.LogError("KeySettingsManager未找到！");
            return;
        }
        
        // 重置为默认设置
        keySettingsManager.ResetToDefault();
        
        var eightHoleKeys = keySettingsManager.GetEightHoleKeys();
        var tenHoleKeys = keySettingsManager.GetTenHoleKeys();
        
        // 验证是否为默认设置
        var defaultSettings = new KeySettings();
        bool eightHoleDefault = ArraysEqual(eightHoleKeys, defaultSettings.eightHoleKeys);
        bool tenHoleDefault = ArraysEqual(tenHoleKeys, defaultSettings.tenHoleKeys);
        
        if (eightHoleDefault && tenHoleDefault)
        {
            Debug.Log("✅ 重置设置功能正常");
        }
        else
        {
            Debug.LogError("❌ 重置设置功能异常");
        }
        
        if (showDetailedLogs)
        {
            Debug.Log($"重置后八孔键位: {string.Join(", ", eightHoleKeys)}");
            Debug.Log($"重置后十孔键位: {string.Join(", ", tenHoleKeys)}");
        }
    }
    
    bool ArraysEqual(KeyCode[] array1, KeyCode[] array2)
    {
        if (array1 == null || array2 == null)
            return false;
            
        if (array1.Length != array2.Length)
            return false;
            
        for (int i = 0; i < array1.Length; i++)
        {
            if (array1[i] != array2[i])
                return false;
        }
        
        return true;
    }
    
    // 手动测试方法（可在Inspector中调用）
    [ContextMenu("运行完整测试")]
    public void RunFullTest()
    {
        StartCoroutine(RunAllTests());
    }
    
    [ContextMenu("显示文件信息")]
    public void ShowFileInfo()
    {
        TestFileInfo();
    }
    
    [ContextMenu("强制保存当前设置")]
    public void ForceSave()
    {
        var keySettingsManager = KeySettingsManager.Instance;
        if (keySettingsManager != null)
        {
            keySettingsManager.ForceSave();
        }
    }
    
    [ContextMenu("清除所有设置")]
    public void ClearAllSettings()
    {
        var keySettingsManager = KeySettingsManager.Instance;
        if (keySettingsManager != null)
        {
            keySettingsManager.ClearAllSettings();
            Debug.Log("所有设置已清除");
        }
    }
    
    void Update()
    {
        // 快捷键测试
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowFileInfo();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ForceSave();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            RunFullTest();
        }
    }
}