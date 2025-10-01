using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeySettings
{
    public KeyCode[] eightHoleKeys;
    public KeyCode[] tenHoleKeys;
    
    public KeySettings()
    {
        // 八孔默认键位
        eightHoleKeys = new KeyCode[] {
            KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F,
            KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon,
            KeyCode.P, KeyCode.Space
        };
        
        // 十孔默认键位
        tenHoleKeys = new KeyCode[] {
            KeyCode.Q, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.R,
            KeyCode.I, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.LeftBracket,
            KeyCode.C, KeyCode.M, KeyCode.Alpha1, KeyCode.Space
        };
    }
}

public class KeySettingsManager : MonoBehaviour
{
    private static KeySettingsManager _instance;
    public static KeySettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<KeySettingsManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("KeySettingsManager");
                    _instance = go.AddComponent<KeySettingsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    private KeySettings currentSettings;
    private const string SAVE_KEY = "KeySettings";
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 确保在下一帧加载设置，避免时序问题
            StartCoroutine(LoadSettingsNextFrame());
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private System.Collections.IEnumerator LoadSettingsNextFrame()
    {
        yield return null; // 等待一帧
        LoadKeySettings();
        Debug.Log("KeySettingsManager初始化完成，键位设置已加载");
    }
    
    public KeySettings GetCurrentSettings()
    {
        if (currentSettings == null)
        {
            LoadKeySettings();
        }
        return currentSettings;
    }
    
    public KeyCode[] GetEightHoleKeys()
    {
        return GetCurrentSettings().eightHoleKeys;
    }
    
    public KeyCode[] GetTenHoleKeys()
    {
        return GetCurrentSettings().tenHoleKeys;
    }
    
    public void SetEightHoleKeys(KeyCode[] keys)
    {
        if (currentSettings == null)
        {
            currentSettings = new KeySettings();
        }
        currentSettings.eightHoleKeys = (KeyCode[])keys.Clone();
        SaveKeySettings();
    }
    
    public void SetTenHoleKeys(KeyCode[] keys)
    {
        if (currentSettings == null)
        {
            currentSettings = new KeySettings();
        }
        currentSettings.tenHoleKeys = (KeyCode[])keys.Clone();
        SaveKeySettings();
    }
    
    public void SaveKeySettings()
    {
        if (currentSettings != null)
        {
            try
            {
                // 使用新的持久化系统保存
                bool success = KeySettingsPersistence.SaveKeySettings(currentSettings);
                if (success)
                {
                    Debug.Log("键位设置已保存到文件和PlayerPrefs");
                    
                    // 验证保存是否成功
                    StartCoroutine(VerifySaveAfterDelay());
                }
                else
                {
                    Debug.LogError("键位设置保存失败");
                    // 尝试备用保存方法
                    FallbackSave();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"保存键位设置时发生异常: {e.Message}");
                FallbackSave();
            }
        }
     }
     
     private System.Collections.IEnumerator VerifySaveAfterDelay()
     {
         yield return new WaitForSeconds(0.1f); // 等待文件系统操作完成
         
         // 尝试重新加载以验证保存
         var testSettings = KeySettingsPersistence.LoadKeySettings();
         if (testSettings != null)
         {
             Debug.Log("保存验证成功");
         }
         else
         {
             Debug.LogWarning("保存验证失败，尝试重新保存");
             KeySettingsPersistence.SaveKeySettings(currentSettings);
         }
     }
     
     private void FallbackSave()
     {
         try
         {
             // 备用保存方法：直接使用PlayerPrefs
             string json = JsonUtility.ToJson(currentSettings);
             PlayerPrefs.SetString("KeySettings_Backup", json);
             PlayerPrefs.Save();
             Debug.Log("使用备用方法保存键位设置");
         }
         catch (System.Exception e)
         {
             Debug.LogError($"备用保存方法也失败: {e.Message}");
         }
     }
    
    public void LoadKeySettings()
    {
        try
        {
            // 使用新的持久化系统加载
            currentSettings = KeySettingsPersistence.LoadKeySettings();
            
            if (currentSettings != null)
            {
                Debug.Log($"键位设置加载成功 - 八孔: {string.Join(", ", currentSettings.eightHoleKeys)}");
                Debug.Log($"键位设置加载成功 - 十孔: {string.Join(", ", currentSettings.tenHoleKeys)}");
                return;
            }
            
            // 如果主要方法失败，尝试从备用位置加载
            Debug.LogWarning("主要加载方法失败，尝试备用方法");
            TryLoadFromBackup();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载键位设置时发生异常: {e.Message}");
            TryLoadFromBackup();
        }
        
        // 如果所有方法都失败，使用默认设置
        if (currentSettings == null)
        {
            Debug.LogWarning("所有加载方法都失败，使用默认设置");
            currentSettings = new KeySettings();
            
            // 立即保存默认设置
            SaveKeySettings();
        }
    }
    
    private void TryLoadFromBackup()
    {
        try
        {
            if (PlayerPrefs.HasKey("KeySettings_Backup"))
            {
                string json = PlayerPrefs.GetString("KeySettings_Backup");
                currentSettings = JsonUtility.FromJson<KeySettings>(json);
                
                if (currentSettings != null)
                {
                    Debug.Log("从备用位置加载键位设置成功");
                    // 尝试将备用设置保存到主要位置
                    KeySettingsPersistence.SaveKeySettings(currentSettings);
                    return;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"从备用位置加载失败: {e.Message}");
        }
    }
    
    public void ResetToDefault()
    {
        currentSettings = new KeySettings();
        SaveKeySettings();
        Debug.Log("键位设置已重置为默认");
    }
    
    // 检查键位是否有冲突
    public bool HasKeyConflict(KeyCode[] keys)
    {
        HashSet<KeyCode> keySet = new HashSet<KeyCode>();
        foreach (KeyCode key in keys)
        {
            if (key != KeyCode.None && !keySet.Add(key))
            {
                return true; // 发现重复键位
            }
        }
        return false;
    }
    
    // 获取键位冲突的详细信息
    public List<KeyCode> GetConflictingKeys(KeyCode[] keys)
    {
        List<KeyCode> conflicts = new List<KeyCode>();
        HashSet<KeyCode> seen = new HashSet<KeyCode>();
        
        foreach (KeyCode key in keys)
        {
            if (key != KeyCode.None && !seen.Add(key))
            {
                if (!conflicts.Contains(key))
                {
                    conflicts.Add(key);
                }
            }
        }
        
        return conflicts;
    }
    
    /// <summary>
    /// 获取键位设置文件信息
    /// </summary>
    public string GetSettingsInfo()
    {
        return KeySettingsPersistence.GetSettingsInfo();
    }
    
    /// <summary>
    /// 清除所有保存的键位设置
    /// </summary>
    public void ClearAllSettings()
    {
        KeySettingsPersistence.ClearAllSettings();
        currentSettings = new KeySettings();
        Debug.Log("所有键位设置已清除，重置为默认设置");
    }
    
    /// <summary>
    /// 强制重新保存当前设置（用于修复损坏的保存文件）
    /// </summary>
    public void ForceSave()
    {
        if (currentSettings == null)
        {
            currentSettings = new KeySettings();
        }
        SaveKeySettings();
        Debug.Log("强制保存键位设置完成");
    }
}