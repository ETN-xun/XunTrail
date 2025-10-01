using UnityEngine;
using System.IO;
using System;

public class SimpleKeySettingsTest : MonoBehaviour
{
    void Start()
    {
        // 在游戏启动时运行测试
        Invoke("RunTest", 1f); // 延迟1秒确保所有系统初始化完成
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            RunTest();
        }
    }
    
    void RunTest()
    {
        Debug.Log("=== 简单键位设置测试开始 ===");
        
        // 1. 获取当前设置
        var manager = KeySettingsManager.Instance;
        var originalEightHole = manager.GetEightHoleKeys();
        
        Debug.Log($"原始八孔键位: {string.Join(", ", originalEightHole)}");
        
        // 2. 修改第一个键位
        var modifiedEightHole = new KeyCode[originalEightHole.Length];
        Array.Copy(originalEightHole, modifiedEightHole, originalEightHole.Length);
        modifiedEightHole[0] = KeyCode.F9; // 改为F9
        
        Debug.Log($"修改后八孔键位: {string.Join(", ", modifiedEightHole)}");
        
        // 3. 保存修改
        manager.SetEightHoleKeys(modifiedEightHole);
        Debug.Log("键位修改已保存");
        
        // 4. 验证保存
        var savedEightHole = manager.GetEightHoleKeys();
        Debug.Log($"保存后读取的八孔键位: {string.Join(", ", savedEightHole)}");
        
        // 5. 检查文件是否存在
        CheckFiles();
        
        // 6. 强制重新加载
        manager.LoadKeySettings();
        var reloadedEightHole = manager.GetEightHoleKeys();
        Debug.Log($"重新加载后的八孔键位: {string.Join(", ", reloadedEightHole)}");
        
        // 7. 验证是否一致
        bool isConsistent = true;
        for (int i = 0; i < modifiedEightHole.Length; i++)
        {
            if (modifiedEightHole[i] != reloadedEightHole[i])
            {
                isConsistent = false;
                Debug.LogError($"键位不一致！位置{i}: 期望{modifiedEightHole[i]}, 实际{reloadedEightHole[i]}");
            }
        }
        
        if (isConsistent)
        {
            Debug.Log("✓ 键位保存和加载一致！");
        }
        else
        {
            Debug.LogError("✗ 键位保存和加载不一致！");
        }
        
        Debug.Log("=== 简单键位设置测试结束 ===");
    }
    
    void CheckFiles()
    {
        try
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsFolder = Path.Combine(appDataPath, "XunTrailSettings");
            string settingsFile = Path.Combine(settingsFolder, "KeySettings.json");
            
            Debug.Log($"检查文件: {settingsFile}");
            Debug.Log($"文件存在: {File.Exists(settingsFile)}");
            
            if (File.Exists(settingsFile))
            {
                string content = File.ReadAllText(settingsFile);
                Debug.Log($"文件内容: {content}");
            }
            
            // 检查PlayerPrefs
            bool hasPlayerPrefs = PlayerPrefs.HasKey("KeySettings");
            Debug.Log($"PlayerPrefs存在: {hasPlayerPrefs}");
            
            if (hasPlayerPrefs)
            {
                string playerPrefsContent = PlayerPrefs.GetString("KeySettings");
                Debug.Log($"PlayerPrefs内容: {playerPrefsContent}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"检查文件时出错: {e.Message}");
        }
    }
    
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), "按F10运行键位设置测试");
        
        if (GUI.Button(new Rect(10, 40, 100, 30), "运行测试"))
        {
            RunTest();
        }
    }
}