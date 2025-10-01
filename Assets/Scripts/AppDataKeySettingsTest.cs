using UnityEngine;
using System.IO;
using System;

/// <summary>
/// AppData键位设置测试脚本
/// 用于验证键位设置是否正确保存到AppData目录
/// </summary>
public class AppDataKeySettingsTest : MonoBehaviour
{
    [Header("测试控制")]
    public bool runTestOnStart = true;
    public KeyCode testTriggerKey = KeyCode.F12;
    
    void Start()
    {
        if (runTestOnStart)
        {
            Invoke("RunTest", 1f); // 延迟1秒执行测试
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(testTriggerKey))
        {
            RunTest();
        }
    }
    
    public void RunTest()
    {
        Debug.Log("=== AppData键位设置测试开始 ===");
        
        // 测试1: 检查当前保存路径
        TestCurrentSavePath();
        
        // 测试2: 测试保存功能
        TestSaveFunction();
        
        // 测试3: 测试加载功能
        TestLoadFunction();
        
        // 测试4: 测试迁移功能
        TestMigrationFunction();
        
        Debug.Log("=== AppData键位设置测试结束 ===");
    }
    
    private void TestCurrentSavePath()
    {
        Debug.Log("--- 测试1: 检查当前保存路径 ---");
        
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string expectedPath = Path.Combine(appDataPath, "XunTrailSettings", "KeySettings.json");
        
        Debug.Log($"预期AppData路径: {expectedPath}");
        
        // 获取实际路径信息
        string settingsInfo = KeySettingsManager.Instance.GetSettingsInfo();
        Debug.Log($"实际设置信息:\n{settingsInfo}");
    }
    
    private void TestSaveFunction()
    {
        Debug.Log("--- 测试2: 测试保存功能 ---");
        
        var keySettingsManager = KeySettingsManager.Instance;
        
        // 创建测试键位
        KeyCode[] testEightHole = new KeyCode[] {
            KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R,
            KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I,
            KeyCode.O, KeyCode.P
        };
        
        KeyCode[] testTenHole = new KeyCode[] {
            KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V,
            KeyCode.B, KeyCode.N, KeyCode.M, KeyCode.Comma,
            KeyCode.Period, KeyCode.Slash, KeyCode.RightShift, KeyCode.Return
        };
        
        // 保存测试键位
        keySettingsManager.SetEightHoleKeys(testEightHole);
        keySettingsManager.SetTenHoleKeys(testTenHole);
        
        Debug.Log("测试键位已保存");
        
        // 验证文件是否存在
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string filePath = Path.Combine(appDataPath, "XunTrailSettings", "KeySettings.json");
        
        if (File.Exists(filePath))
        {
            Debug.Log("✅ 键位设置文件已成功保存到AppData");
            Debug.Log($"文件路径: {filePath}");
            
            // 读取文件内容验证
            string content = File.ReadAllText(filePath);
            Debug.Log($"文件内容: {content}");
        }
        else
        {
            Debug.LogError("❌ 键位设置文件未找到");
        }
    }
    
    private void TestLoadFunction()
    {
        Debug.Log("--- 测试3: 测试加载功能 ---");
        
        var keySettingsManager = KeySettingsManager.Instance;
        
        // 重新加载设置
        keySettingsManager.LoadKeySettings();
        
        var loadedEightHole = keySettingsManager.GetEightHoleKeys();
        var loadedTenHole = keySettingsManager.GetTenHoleKeys();
        
        Debug.Log($"加载的八孔键位: {string.Join(", ", loadedEightHole)}");
        Debug.Log($"加载的十孔键位: {string.Join(", ", loadedTenHole)}");
        
        Debug.Log("✅ 键位设置加载测试完成");
    }
    
    private void TestMigrationFunction()
    {
        Debug.Log("--- 测试4: 测试迁移功能信息 ---");
        
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string oldPath = Path.Combine(documentsPath, "XunTrailSettings", "KeySettings.json");
        
        Debug.Log($"旧Documents路径: {oldPath}");
        Debug.Log($"旧文件存在: {File.Exists(oldPath)}");
        
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string newPath = Path.Combine(appDataPath, "XunTrailSettings", "KeySettings.json");
        
        Debug.Log($"新AppData路径: {newPath}");
        Debug.Log($"新文件存在: {File.Exists(newPath)}");
        
        if (File.Exists(oldPath) && !File.Exists(newPath))
        {
            Debug.Log("检测到旧文件存在但新文件不存在，下次加载时将自动迁移");
        }
        else if (File.Exists(newPath))
        {
            Debug.Log("✅ 新AppData路径文件存在，迁移已完成或不需要迁移");
        }
    }
}