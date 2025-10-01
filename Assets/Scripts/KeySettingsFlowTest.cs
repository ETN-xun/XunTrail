using System.Collections;
using System.IO;
using UnityEngine;

public class KeySettingsFlowTest : MonoBehaviour
{
    [Header("测试控制")]
    public bool runTestOnStart = true;
    public bool showDebugGUI = true;
    
    private string testResult = "";
    private bool testCompleted = false;
    
    void Start()
    {
        if (runTestOnStart)
        {
            StartCoroutine(RunCompleteFlowTest());
        }
    }
    
    private System.Collections.IEnumerator RunCompleteFlowTest()
    {
        testResult = "开始键位保存/加载流程测试...\n";
        
        // 等待KeySettingsManager初始化
        yield return new WaitForSeconds(1f);
        
        var manager = KeySettingsManager.Instance;
        if (manager == null)
        {
            testResult += "❌ KeySettingsManager未找到！\n";
            testCompleted = true;
            yield break;
        }
        
        testResult += "✓ KeySettingsManager已找到\n";
        
        // 1. 记录原始设置
        var originalEightHole = manager.GetEightHoleKeys();
        var originalTenHole = manager.GetTenHoleKeys();
        testResult += $"✓ 原始八孔键位: {string.Join(", ", originalEightHole)}\n";
        testResult += $"✓ 原始十孔键位: {string.Join(", ", originalTenHole)}\n";
        
        // 2. 修改键位设置
        KeyCode[] testEightHole = new KeyCode[] 
        {
            KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R,
            KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I
        };
        
        KeyCode[] testTenHole = new KeyCode[]
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
            KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
        };
        
        testResult += "📝 设置测试键位...\n";
        manager.SetEightHoleKeys(testEightHole);
        manager.SetTenHoleKeys(testTenHole);
        
        // 3. 等待保存完成
        yield return new WaitForSeconds(0.5f);
        
        // 4. 检查AppData文件是否存在
        string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        string settingsFolder = Path.Combine(appDataPath, "XunTrailSettings");
        string settingsFile = Path.Combine(settingsFolder, "KeySettings.json");
        
        if (File.Exists(settingsFile))
        {
            testResult += "✓ AppData文件已创建: " + settingsFile + "\n";
            
            // 读取文件内容
            string fileContent = File.ReadAllText(settingsFile);
            testResult += "✓ 文件内容长度: " + fileContent.Length + " 字符\n";
            
            // 检查是否包含测试键位
            if (fileContent.Contains("Q") && fileContent.Contains("Alpha1"))
            {
                testResult += "✓ 文件包含测试键位数据\n";
            }
            else
            {
                testResult += "❌ 文件不包含测试键位数据\n";
            }
        }
        else
        {
            testResult += "❌ AppData文件未创建: " + settingsFile + "\n";
        }
        
        // 5. 强制重新加载设置
        testResult += "🔄 强制重新加载设置...\n";
        manager.LoadKeySettings();
        
        yield return new WaitForSeconds(0.2f);
        
        // 6. 验证加载的设置
        var loadedEightHole = manager.GetEightHoleKeys();
        var loadedTenHole = manager.GetTenHoleKeys();
        
        bool eightHoleMatch = true;
        bool tenHoleMatch = true;
        
        for (int i = 0; i < testEightHole.Length; i++)
        {
            if (i >= loadedEightHole.Length || loadedEightHole[i] != testEightHole[i])
            {
                eightHoleMatch = false;
                break;
            }
        }
        
        for (int i = 0; i < testTenHole.Length; i++)
        {
            if (i >= loadedTenHole.Length || loadedTenHole[i] != testTenHole[i])
            {
                tenHoleMatch = false;
                break;
            }
        }
        
        if (eightHoleMatch)
        {
            testResult += "✓ 八孔键位加载正确\n";
        }
        else
        {
            testResult += "❌ 八孔键位加载错误\n";
            testResult += $"期望: {string.Join(", ", testEightHole)}\n";
            testResult += $"实际: {string.Join(", ", loadedEightHole)}\n";
        }
        
        if (tenHoleMatch)
        {
            testResult += "✓ 十孔键位加载正确\n";
        }
        else
        {
            testResult += "❌ 十孔键位加载错误\n";
            testResult += $"期望: {string.Join(", ", testTenHole)}\n";
            testResult += $"实际: {string.Join(", ", loadedTenHole)}\n";
        }
        
        // 7. 恢复原始设置
        testResult += "🔄 恢复原始设置...\n";
        manager.SetEightHoleKeys(originalEightHole);
        manager.SetTenHoleKeys(originalTenHole);
        
        yield return new WaitForSeconds(0.2f);
        
        // 8. 最终结果
        if (eightHoleMatch && tenHoleMatch && File.Exists(settingsFile))
        {
            testResult += "\n🎉 测试通过！键位保存/加载功能正常\n";
        }
        else
        {
            testResult += "\n❌ 测试失败！存在键位保存/加载问题\n";
        }
        
        testCompleted = true;
        Debug.Log(testResult);
    }
    
    void OnGUI()
    {
        if (!showDebugGUI) return;
        
        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));
        GUILayout.Label("键位保存/加载流程测试", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });
        
        if (GUILayout.Button("运行测试", GUILayout.Height(30)))
        {
            testResult = "";
            testCompleted = false;
            StartCoroutine(RunCompleteFlowTest());
        }
        
        if (GUILayout.Button("清除所有设置", GUILayout.Height(30)))
        {
            KeySettingsManager.Instance?.ClearAllSettings();
            testResult += "已清除所有设置\n";
        }
        
        if (GUILayout.Button("强制保存当前设置", GUILayout.Height(30)))
        {
            KeySettingsManager.Instance?.ForceSave();
            testResult += "已强制保存当前设置\n";
        }
        
        GUILayout.Space(10);
        
        // 显示测试结果
        GUILayout.Label("测试结果:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        GUILayout.TextArea(testResult, GUILayout.ExpandHeight(true));
        
        GUILayout.EndArea();
    }
}