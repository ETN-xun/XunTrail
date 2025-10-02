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
            LoadKeySettings();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
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
            // 使用新的持久化系统保存
            bool success = KeySettingsPersistence.SaveKeySettings(currentSettings);
            if (success)
            {
                Debug.Log("键位设置已保存到文件和PlayerPrefs");
            }
            else
            {
                Debug.LogError("键位设置保存失败");
            }
        }
    }
    
    public void LoadKeySettings()
    {
        // 使用新的持久化系统加载
        currentSettings = KeySettingsPersistence.LoadKeySettings();
        Debug.Log("键位设置已从文件或PlayerPrefs加载");
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