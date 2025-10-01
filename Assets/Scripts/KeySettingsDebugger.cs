using System;
using System.IO;
using UnityEngine;

public class KeySettingsDebugger : MonoBehaviour
{
    [Header("调试控制")]
    public KeyCode testKey = KeyCode.F11;
    public bool enableDebugLogs = true;
    
    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            RunFullDiagnostic();
        }
    }
    
    public void RunFullDiagnostic()
    {
        Debug.Log("=== 键位设置完整诊断开始 ===");
        
        // 1. 检查当前设置
        CheckCurrentSettings();
        
        // 2. 测试保存功能
        TestSaveFunction();
        
        // 3. 检查文件系统
        CheckFileSystem();
        
        // 4. 测试加载功能
        TestLoadFunction();
        
        Debug.Log("=== 键位设置完整诊断结束 ===");
    }
    
    private void CheckCurrentSettings()
    {
        Debug.Log("--- 检查当前设置 ---");
        
        var manager = KeySettingsManager.Instance;
        var eightHole = manager.GetEightHoleKeys();
        var tenHole = manager.GetTenHoleKeys();
        
        Debug.Log($"八孔键位: {string.Join(", ", eightHole)}");
        Debug.Log($"十孔键位: {string.Join(", ", tenHole)}");
        
        // 获取设置信息
        string settingsInfo = manager.GetSettingsInfo();
        Debug.Log($"设置文件信息:\n{settingsInfo}");
    }
    
    private void TestSaveFunction()
    {
        Debug.Log("--- 测试保存功能 ---");
        
        var manager = KeySettingsManager.Instance;
        
        // 修改一个键位进行测试
        var testEightHole = manager.GetEightHoleKeys();
        var originalKey = testEightHole[0];
        testEightHole[0] = KeyCode.F1; // 临时修改
        
        Debug.Log($"临时修改第一个八孔键位从 {originalKey} 到 {testEightHole[0]}");
        
        // 保存设置
        manager.SetEightHoleKeys(testEightHole);
        
        // 恢复原始键位
        testEightHole[0] = originalKey;
        manager.SetEightHoleKeys(testEightHole);
        
        Debug.Log("保存测试完成，已恢复原始键位");
    }
    
    private void CheckFileSystem()
    {
        Debug.Log("--- 检查文件系统 ---");
        
        try
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsFolder = Path.Combine(appDataPath, "XunTrailSettings");
            string settingsFile = Path.Combine(settingsFolder, "KeySettings.json");
            string backupFile = Path.Combine(settingsFolder, "KeySettings_Backup.json");
            
            Debug.Log($"AppData路径: {appDataPath}");
            Debug.Log($"设置文件夹: {settingsFolder}");
            Debug.Log($"设置文件: {settingsFile}");
            Debug.Log($"备份文件: {backupFile}");
            
            Debug.Log($"设置文件夹存在: {Directory.Exists(settingsFolder)}");
            Debug.Log($"设置文件存在: {File.Exists(settingsFile)}");
            Debug.Log($"备份文件存在: {File.Exists(backupFile)}");
            
            if (File.Exists(settingsFile))
            {
                var fileInfo = new FileInfo(settingsFile);
                Debug.Log($"设置文件大小: {fileInfo.Length} 字节");
                Debug.Log($"设置文件修改时间: {fileInfo.LastWriteTime}");
                
                // 读取文件内容
                string content = File.ReadAllText(settingsFile);
                Debug.Log($"设置文件内容:\n{content}");
            }
            
            // 检查PlayerPrefs
            bool hasPlayerPrefs = PlayerPrefs.HasKey("KeySettings");
            Debug.Log($"PlayerPrefs中有键位设置: {hasPlayerPrefs}");
            
            if (hasPlayerPrefs)
            {
                string playerPrefsContent = PlayerPrefs.GetString("KeySettings");
                Debug.Log($"PlayerPrefs内容:\n{playerPrefsContent}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"检查文件系统时出错: {e.Message}");
        }
    }
    
    private void TestLoadFunction()
    {
        Debug.Log("--- 测试加载功能 ---");
        
        var manager = KeySettingsManager.Instance;
        
        // 强制重新加载设置
        manager.LoadKeySettings();
        
        var eightHole = manager.GetEightHoleKeys();
        var tenHole = manager.GetTenHoleKeys();
        
        Debug.Log($"重新加载后的八孔键位: {string.Join(", ", eightHole)}");
        Debug.Log($"重新加载后的十孔键位: {string.Join(", ", tenHole)}");
    }
    
    void OnGUI()
    {
        if (enableDebugLogs)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"按 {testKey} 运行键位设置诊断");
            
            if (GUI.Button(new Rect(10, 40, 150, 30), "运行诊断"))
            {
                RunFullDiagnostic();
            }
            
            if (GUI.Button(new Rect(170, 40, 150, 30), "强制保存"))
            {
                KeySettingsManager.Instance.ForceSave();
                Debug.Log("强制保存完成");
            }
            
            if (GUI.Button(new Rect(10, 80, 150, 30), "清除所有设置"))
            {
                KeySettingsManager.Instance.ClearAllSettings();
                Debug.Log("所有设置已清除");
            }
        }
    }
}