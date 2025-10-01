using UnityEngine;
using System.IO;

public class QuickKeySettingsTest : MonoBehaviour
{
    void Start()
    {
        // 延迟执行测试，确保所有系统初始化完成
        Invoke("RunQuickTest", 2f);
    }
    
    void RunQuickTest()
    {
        Debug.Log("=== 开始快速键位测试 ===");
        
        var manager = KeySettingsManager.Instance;
        if (manager == null)
        {
            Debug.LogError("❌ KeySettingsManager未找到！");
            return;
        }
        
        // 1. 显示当前键位
        var currentEight = manager.GetEightHoleKeys();
        var currentTen = manager.GetTenHoleKeys();
        Debug.Log($"当前八孔键位: {string.Join(", ", currentEight)}");
        Debug.Log($"当前十孔键位: {string.Join(", ", currentTen)}");
        
        // 2. 检查AppData文件
        string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        string settingsFile = Path.Combine(appDataPath, "XunTrailSettings", "KeySettings.json");
        
        if (File.Exists(settingsFile))
        {
            Debug.Log($"✓ AppData文件存在: {settingsFile}");
            string content = File.ReadAllText(settingsFile);
            Debug.Log($"文件内容长度: {content.Length} 字符");
        }
        else
        {
            Debug.Log($"❌ AppData文件不存在: {settingsFile}");
        }
        
        // 3. 测试修改和保存
        Debug.Log("📝 测试修改键位...");
        KeyCode[] testKeys = new KeyCode[] 
        {
            KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R,
            KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I
        };
        
        manager.SetEightHoleKeys(testKeys);
        Debug.Log("✓ 已设置测试键位");
        
        // 4. 验证保存
        Invoke("VerifySave", 1f);
    }
    
    void VerifySave()
    {
        Debug.Log("🔍 验证保存结果...");
        
        // 检查文件是否更新
        string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        string settingsFile = Path.Combine(appDataPath, "XunTrailSettings", "KeySettings.json");
        
        if (File.Exists(settingsFile))
        {
            string content = File.ReadAllText(settingsFile);
            if (content.Contains("Q") && content.Contains("W"))
            {
                Debug.Log("✓ 文件已更新，包含测试键位");
            }
            else
            {
                Debug.Log("❌ 文件未包含测试键位");
            }
        }
        
        // 强制重新加载并验证
        var manager = KeySettingsManager.Instance;
        manager.LoadKeySettings();
        
        var loadedKeys = manager.GetEightHoleKeys();
        Debug.Log($"重新加载的键位: {string.Join(", ", loadedKeys)}");
        
        if (loadedKeys.Length >= 2 && loadedKeys[0] == KeyCode.Q && loadedKeys[1] == KeyCode.W)
        {
            Debug.Log("🎉 测试成功！键位保存和加载正常工作");
        }
        else
        {
            Debug.Log("❌ 测试失败！键位未正确保存或加载");
        }
        
        Debug.Log("=== 快速键位测试完成 ===");
    }
    
    void OnGUI()
    {
        if (GUILayout.Button("运行快速测试", GUILayout.Height(40)))
        {
            RunQuickTest();
        }
        
        if (GUILayout.Button("检查AppData文件", GUILayout.Height(40)))
        {
            string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string settingsFile = Path.Combine(appDataPath, "XunTrailSettings", "KeySettings.json");
            
            if (File.Exists(settingsFile))
            {
                string content = File.ReadAllText(settingsFile);
                Debug.Log($"AppData文件内容:\n{content}");
            }
            else
            {
                Debug.Log("AppData文件不存在");
            }
        }
    }
}