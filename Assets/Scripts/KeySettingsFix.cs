using UnityEngine;
using System.IO;
using System;

/// <summary>
/// 键位设置修复脚本 - 确保键位设置能够正确保存和加载
/// </summary>
public class KeySettingsFix : MonoBehaviour
{
    [Header("自动修复设置")]
    public bool autoFixOnStart = true;
    public bool enableDebugMode = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            Invoke("RunFix", 0.5f); // 延迟执行确保所有系统初始化完成
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            RunFix();
        }
    }
    
    public void RunFix()
    {
        if (enableDebugMode)
        {
            Debug.Log("=== 键位设置修复开始 ===");
        }
        
        try
        {
            // 1. 确保KeySettingsManager正确初始化
            EnsureManagerInitialized();
            
            // 2. 验证AppData目录结构
            EnsureDirectoryStructure();
            
            // 3. 测试保存功能
            TestSaveFunction();
            
            // 4. 测试加载功能
            TestLoadFunction();
            
            // 5. 验证持久性
            TestPersistence();
            
            if (enableDebugMode)
            {
                Debug.Log("=== 键位设置修复完成 ===");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"键位设置修复失败: {e.Message}");
        }
    }
    
    private void EnsureManagerInitialized()
    {
        var manager = KeySettingsManager.Instance;
        if (manager == null)
        {
            Debug.LogError("KeySettingsManager未正确初始化！");
            return;
        }
        
        // 强制重新加载设置
        manager.LoadKeySettings();
        
        if (enableDebugMode)
        {
            Debug.Log("✓ KeySettingsManager已正确初始化");
        }
    }
    
    private void EnsureDirectoryStructure()
    {
        try
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsFolder = Path.Combine(appDataPath, "XunTrailSettings");
            
            if (!Directory.Exists(settingsFolder))
            {
                Directory.CreateDirectory(settingsFolder);
                Debug.Log($"创建设置目录: {settingsFolder}");
            }
            
            if (enableDebugMode)
            {
                Debug.Log($"✓ AppData目录结构正确: {settingsFolder}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"创建目录结构失败: {e.Message}");
        }
    }
    
    private void TestSaveFunction()
    {
        try
        {
            var manager = KeySettingsManager.Instance;
            
            // 获取当前设置
            var currentEightHole = manager.GetEightHoleKeys();
            var currentTenHole = manager.GetTenHoleKeys();
            
            // 创建测试设置
            var testEightHole = new KeyCode[currentEightHole.Length];
            Array.Copy(currentEightHole, testEightHole, currentEightHole.Length);
            testEightHole[0] = KeyCode.F8; // 临时修改用于测试
            
            // 保存测试设置
            manager.SetEightHoleKeys(testEightHole);
            
            // 验证保存
            var savedEightHole = manager.GetEightHoleKeys();
            bool saveSuccess = savedEightHole[0] == KeyCode.F8;
            
            if (saveSuccess)
            {
                if (enableDebugMode)
                {
                    Debug.Log("✓ 保存功能测试通过");
                }
                
                // 恢复原始设置
                manager.SetEightHoleKeys(currentEightHole);
            }
            else
            {
                Debug.LogError("✗ 保存功能测试失败");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"保存功能测试出错: {e.Message}");
        }
    }
    
    private void TestLoadFunction()
    {
        try
        {
            var manager = KeySettingsManager.Instance;
            
            // 强制重新加载
            manager.LoadKeySettings();
            
            var eightHole = manager.GetEightHoleKeys();
            var tenHole = manager.GetTenHoleKeys();
            
            bool loadSuccess = eightHole != null && eightHole.Length == 10 && 
                              tenHole != null && tenHole.Length == 12;
            
            if (loadSuccess)
            {
                if (enableDebugMode)
                {
                    Debug.Log("✓ 加载功能测试通过");
                }
            }
            else
            {
                Debug.LogError("✗ 加载功能测试失败");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"加载功能测试出错: {e.Message}");
        }
    }
    
    private void TestPersistence()
    {
        try
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsFile = Path.Combine(appDataPath, "XunTrailSettings", "KeySettings.json");
            
            bool fileExists = File.Exists(settingsFile);
            bool playerPrefsExists = PlayerPrefs.HasKey("KeySettings");
            
            if (fileExists || playerPrefsExists)
            {
                if (enableDebugMode)
                {
                    Debug.Log($"✓ 持久性验证通过 - 文件存在: {fileExists}, PlayerPrefs存在: {playerPrefsExists}");
                }
            }
            else
            {
                Debug.LogWarning("⚠ 没有找到保存的键位设置文件");
                
                // 尝试强制保存一次
                var manager = KeySettingsManager.Instance;
                manager.ForceSave();
                
                Debug.Log("已执行强制保存");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"持久性测试出错: {e.Message}");
        }
    }
    
    void OnGUI()
    {
        if (enableDebugMode)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "键位设置修复工具");
            GUI.Label(new Rect(10, 30, 300, 20), "按F12运行修复");
            
            if (GUI.Button(new Rect(10, 60, 120, 30), "运行修复"))
            {
                RunFix();
            }
            
            if (GUI.Button(new Rect(140, 60, 120, 30), "强制保存"))
            {
                KeySettingsManager.Instance.ForceSave();
                Debug.Log("强制保存完成");
            }
            
            if (GUI.Button(new Rect(270, 60, 120, 30), "重新加载"))
            {
                KeySettingsManager.Instance.LoadKeySettings();
                Debug.Log("重新加载完成");
            }
        }
    }
}