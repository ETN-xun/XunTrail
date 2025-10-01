using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 键位设置持久化管理器
/// 支持文件保存和PlayerPrefs双重保存机制
/// </summary>
public static class KeySettingsPersistence
{
    private const string SETTINGS_FOLDER = "XunTrailSettings";
    private const string SETTINGS_FILE = "KeySettings.json";
    private const string BACKUP_FILE = "KeySettings_Backup.json";
    private const string PLAYERPREFS_KEY = "KeySettings";
    
    /// <summary>
    /// 获取设置文件的完整路径
    /// </summary>
    private static string GetSettingsPath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string settingsFolder = Path.Combine(appDataPath, SETTINGS_FOLDER);
        
        // 确保文件夹存在
        if (!Directory.Exists(settingsFolder))
        {
            Directory.CreateDirectory(settingsFolder);
            Debug.Log($"创建设置文件夹: {settingsFolder}");
        }
        
        return Path.Combine(settingsFolder, SETTINGS_FILE);
    }
    
    /// <summary>
    /// 获取备份文件的完整路径
    /// </summary>
    private static string GetBackupPath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string settingsFolder = Path.Combine(appDataPath, SETTINGS_FOLDER);
        
        // 确保文件夹存在
        if (!Directory.Exists(settingsFolder))
        {
            Directory.CreateDirectory(settingsFolder);
            Debug.Log($"创建设置文件夹: {settingsFolder}");
        }
        
        return Path.Combine(settingsFolder, BACKUP_FILE);
    }
    
    /// <summary>
    /// 保存键位设置到文件和PlayerPrefs
    /// </summary>
    /// <param name="settings">要保存的键位设置</param>
    /// <returns>保存是否成功</returns>
    public static bool SaveKeySettings(KeySettings settings)
    {
        if (settings == null)
        {
            Debug.LogError("尝试保存空的键位设置");
            return false;
        }
        
        bool fileSuccess = false;
        bool prefsSuccess = false;
        
        try
        {
            // 序列化设置
            string json = JsonUtility.ToJson(settings, true);
            
            // 1. 保存到文件
            string filePath = GetSettingsPath();
            
            // 如果文件已存在，先创建备份
            if (File.Exists(filePath))
            {
                string backupPath = GetBackupPath();
                File.Copy(filePath, backupPath, true);
                Debug.Log($"创建备份文件: {backupPath}");
            }
            
            // 写入新文件
            File.WriteAllText(filePath, json);
            fileSuccess = true;
            Debug.Log($"键位设置已保存到文件: {filePath}");
            
            // 2. 保存到PlayerPrefs作为备份
            PlayerPrefs.SetString(PLAYERPREFS_KEY, json);
            PlayerPrefs.Save();
            prefsSuccess = true;
            Debug.Log("键位设置已保存到PlayerPrefs");
            
        }
        catch (Exception e)
        {
            Debug.LogError($"保存键位设置失败: {e.Message}");
        }
        
        return fileSuccess || prefsSuccess; // 只要有一种方式成功就算成功
    }
    
    /// <summary>
    /// 从文件或PlayerPrefs加载键位设置
    /// </summary>
    /// <returns>加载的键位设置，如果失败则返回默认设置</returns>
    public static KeySettings LoadKeySettings()
    {
        KeySettings settings = null;
        
        // 1. 首先尝试从新的AppData路径加载
        settings = LoadFromFile();
        
        // 2. 如果AppData路径没有文件，尝试从旧的Documents路径迁移
        if (settings == null)
        {
            settings = MigrateFromOldPath();
        }
        
        // 3. 如果迁移失败，尝试从PlayerPrefs加载
        if (settings == null)
        {
            settings = LoadFromPlayerPrefs();
        }
        
        // 4. 如果都失败了，使用默认设置
        if (settings == null)
        {
            Debug.Log("使用默认键位设置");
            settings = new KeySettings();
            
            // 保存默认设置
            SaveKeySettings(settings);
        }
        
        return settings;
    }
    
    /// <summary>
    /// 从文件加载键位设置
    /// </summary>
    private static KeySettings LoadFromFile()
    {
        try
        {
            string filePath = GetSettingsPath();
            
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                KeySettings settings = JsonUtility.FromJson<KeySettings>(json);
                
                if (ValidateSettings(settings))
                {
                    Debug.Log($"从文件加载键位设置成功: {filePath}");
                    return settings;
                }
                else
                {
                    Debug.LogWarning("文件中的键位设置无效，尝试加载备份");
                    return LoadFromBackup();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"从文件加载键位设置失败: {e.Message}");
            return LoadFromBackup();
        }
        
        return null;
    }
    
    /// <summary>
    /// 从备份文件加载键位设置
    /// </summary>
    private static KeySettings LoadFromBackup()
    {
        try
        {
            string backupPath = GetBackupPath();
            
            if (File.Exists(backupPath))
            {
                string json = File.ReadAllText(backupPath);
                KeySettings settings = JsonUtility.FromJson<KeySettings>(json);
                
                if (ValidateSettings(settings))
                {
                    Debug.Log($"从备份文件加载键位设置成功: {backupPath}");
                    return settings;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"从备份文件加载键位设置失败: {e.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// 从旧的Documents路径迁移键位设置到新的AppData路径
    /// </summary>
    private static KeySettings MigrateFromOldPath()
    {
        try
        {
            // 构建旧路径
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string oldSettingsFolder = Path.Combine(documentsPath, SETTINGS_FOLDER);
            string oldFilePath = Path.Combine(oldSettingsFolder, SETTINGS_FILE);
            string oldBackupPath = Path.Combine(oldSettingsFolder, BACKUP_FILE);
            
            KeySettings settings = null;
            
            // 尝试从旧的主文件加载
            if (File.Exists(oldFilePath))
            {
                string json = File.ReadAllText(oldFilePath);
                settings = JsonUtility.FromJson<KeySettings>(json);
                
                if (ValidateSettings(settings))
                {
                    Debug.Log($"从旧路径加载键位设置成功: {oldFilePath}");
                    
                    // 保存到新路径
                    SaveKeySettings(settings);
                    Debug.Log("键位设置已迁移到AppData目录");
                    
                    // 可选：删除旧文件（注释掉以保留备份）
                    // File.Delete(oldFilePath);
                    // if (File.Exists(oldBackupPath)) File.Delete(oldBackupPath);
                    
                    return settings;
                }
            }
            
            // 尝试从旧的备份文件加载
            if (File.Exists(oldBackupPath))
            {
                string json = File.ReadAllText(oldBackupPath);
                settings = JsonUtility.FromJson<KeySettings>(json);
                
                if (ValidateSettings(settings))
                {
                    Debug.Log($"从旧备份路径加载键位设置成功: {oldBackupPath}");
                    
                    // 保存到新路径
                    SaveKeySettings(settings);
                    Debug.Log("键位设置已从备份迁移到AppData目录");
                    
                    return settings;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"从旧路径迁移键位设置失败: {e.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// 从PlayerPrefs加载键位设置
    /// </summary>
    private static KeySettings LoadFromPlayerPrefs()
    {
        try
        {
            if (PlayerPrefs.HasKey(PLAYERPREFS_KEY))
            {
                string json = PlayerPrefs.GetString(PLAYERPREFS_KEY);
                KeySettings settings = JsonUtility.FromJson<KeySettings>(json);
                
                if (ValidateSettings(settings))
                {
                    Debug.Log("从PlayerPrefs加载键位设置成功");
                    return settings;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"从PlayerPrefs加载键位设置失败: {e.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// 验证键位设置的有效性
    /// </summary>
    private static bool ValidateSettings(KeySettings settings)
    {
        if (settings == null)
            return false;
            
        if (settings.eightHoleKeys == null || settings.eightHoleKeys.Length != 10)
            return false;
            
        if (settings.tenHoleKeys == null || settings.tenHoleKeys.Length != 12)
            return false;
            
        return true;
    }
    
    /// <summary>
    /// 删除所有保存的键位设置
    /// </summary>
    public static void ClearAllSettings()
    {
        try
        {
            // 删除文件
            string filePath = GetSettingsPath();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log("删除键位设置文件");
            }
            
            // 删除备份文件
            string backupPath = GetBackupPath();
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
                Debug.Log("删除键位设置备份文件");
            }
            
            // 清除PlayerPrefs
            if (PlayerPrefs.HasKey(PLAYERPREFS_KEY))
            {
                PlayerPrefs.DeleteKey(PLAYERPREFS_KEY);
                PlayerPrefs.Save();
                Debug.Log("清除PlayerPrefs中的键位设置");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"清除键位设置失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 获取设置文件信息
    /// </summary>
    public static string GetSettingsInfo()
    {
        string info = "=== 键位设置文件信息 ===\n";
        
        string filePath = GetSettingsPath();
        string backupPath = GetBackupPath();
        
        info += $"设置文件路径: {filePath}\n";
        info += $"文件存在: {File.Exists(filePath)}\n";
        
        if (File.Exists(filePath))
        {
            FileInfo fileInfo = new FileInfo(filePath);
            info += $"文件大小: {fileInfo.Length} 字节\n";
            info += $"最后修改: {fileInfo.LastWriteTime}\n";
        }
        
        info += $"\n备份文件路径: {backupPath}\n";
        info += $"备份存在: {File.Exists(backupPath)}\n";
        
        info += $"\nPlayerPrefs存在: {PlayerPrefs.HasKey(PLAYERPREFS_KEY)}\n";
        
        return info;
    }
}