using System;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FileSelector : MonoBehaviour
{
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrFilter;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrFile;
        public int nMaxFile;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrInitialDir;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }

    public static string OpenFileDialog(string title = "选择文件", string filter = "文本文件\0*.txt\0所有文件\0*.*\0")
    {
        #if UNITY_EDITOR
        // 在Unity编辑器中使用EditorUtility.OpenFilePanel
        string path = EditorUtility.OpenFilePanel(title, "", "txt");
        return string.IsNullOrEmpty(path) ? null : path;
        #elif UNITY_STANDALONE_WIN
        // 在Windows独立版本中使用原生文件对话框
        try
        {
            OpenFileName ofn = new OpenFileName();
            ofn.lStructSize = Marshal.SizeOf(ofn);
            
            // 改进的窗口句柄获取逻辑
            IntPtr unityWindow = GetUnityWindowHandle();
            ofn.hwndOwner = unityWindow;
            
            ofn.lpstrFilter = filter;
            
            // 使用StringBuilder来正确初始化字符串缓冲区
            StringBuilder fileBuffer = new StringBuilder(260); // MAX_PATH
            ofn.lpstrFile = fileBuffer.ToString();
            ofn.nMaxFile = 260;
            
            StringBuilder fileTitleBuffer = new StringBuilder(260);
            ofn.lpstrFileTitle = fileTitleBuffer.ToString();
            ofn.nMaxFileTitle = 260;
            
            ofn.lpstrTitle = title;
            ofn.lpstrInitialDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ofn.lpstrDefExt = "txt";
            
            // 设置标志位，确保对话框正常显示
            ofn.Flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008 | 0x00000004; 
            // OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_LONGNAMES | OFN_NOCHANGEDIR | OFN_HIDEREADONLY

            Debug.Log($"FileSelector: 尝试打开文件对话框，窗口句柄: {ofn.hwndOwner}");
            
            if (GetOpenFileName(ref ofn))
            {
                string selectedFile = ofn.lpstrFile.Trim('\0');
                Debug.Log($"FileSelector: 文件选择成功: {selectedFile}");
                return selectedFile;
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                Debug.LogWarning($"FileSelector: 文件选择对话框失败，错误代码: {error}");
                
                // 如果原生对话框失败，尝试备用方案
                return TryAlternativeFileSelection();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("文件选择对话框错误: " + e.Message);
            // 发生异常时使用备用方案
            return TryAlternativeFileSelection();
        }
        #else
        // 其他平台的备用方案
        Debug.LogWarning("当前平台不支持文件选择对话框，请将txt文件放入StreamingAssets文件夹");
        return SelectFileFromStreamingAssets();
        #endif
    }

    // 改进的窗口句柄获取方法
    private static IntPtr GetUnityWindowHandle()
    {
        // 尝试多种方法获取Unity窗口句柄
        IntPtr handle = IntPtr.Zero;
        
        // 方法1: 获取当前活动窗口
        handle = GetActiveWindow();
        if (handle != IntPtr.Zero)
        {
            Debug.Log($"FileSelector: 通过GetActiveWindow获取窗口句柄: {handle}");
            return handle;
        }
        
        // 方法2: 获取前台窗口
        handle = GetForegroundWindow();
        if (handle != IntPtr.Zero)
        {
            Debug.Log($"FileSelector: 通过GetForegroundWindow获取窗口句柄: {handle}");
            return handle;
        }
        
        // 方法3: 尝试查找Unity窗口
        string[] possibleTitles = { 
            Application.productName, 
            "Unity", 
            System.IO.Path.GetFileNameWithoutExtension(Application.dataPath)
        };
        
        foreach (string title in possibleTitles)
        {
            handle = FindWindow(null, title);
            if (handle != IntPtr.Zero)
            {
                Debug.Log($"FileSelector: 通过FindWindow找到窗口: {title}, 句柄: {handle}");
                return handle;
            }
        }
        
        // 方法4: 获取控制台窗口作为最后的备用
        handle = GetConsoleWindow();
        if (handle != IntPtr.Zero)
        {
            Debug.Log($"FileSelector: 使用控制台窗口句柄: {handle}");
            return handle;
        }
        
        Debug.LogWarning("FileSelector: 无法获取有效的窗口句柄，使用空句柄");
        return IntPtr.Zero;
    }

    // 备用文件选择方案
    private static string TryAlternativeFileSelection()
    {
        Debug.Log("FileSelector: 尝试备用文件选择方案");
        
        // 首先尝试从StreamingAssets加载
        string streamingFile = SelectFileFromStreamingAssets();
        if (!string.IsNullOrEmpty(streamingFile))
        {
            Debug.Log($"FileSelector: 从StreamingAssets选择文件: {streamingFile}");
            return streamingFile;
        }
        
        // 尝试从常见位置查找txt文件
        string[] searchPaths = {
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Application.dataPath + "/../"
        };
        
        foreach (string searchPath in searchPaths)
        {
            try
            {
                if (Directory.Exists(searchPath))
                {
                    string[] txtFiles = Directory.GetFiles(searchPath, "*.txt", SearchOption.TopDirectoryOnly);
                    if (txtFiles.Length > 0)
                    {
                        Debug.Log($"FileSelector: 在 {searchPath} 找到txt文件: {txtFiles[0]}");
                        return txtFiles[0];
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"FileSelector: 搜索路径 {searchPath} 时出错: {e.Message}");
            }
        }
        
        Debug.LogWarning("FileSelector: 所有备用方案都失败了");
        return null;
    }

    // 简化的文件选择方法，用于WebGL等不支持原生对话框的平台
    public static string SelectFileFromStreamingAssets()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        if (Directory.Exists(streamingAssetsPath))
        {
            string[] txtFiles = Directory.GetFiles(streamingAssetsPath, "*.txt");
            if (txtFiles.Length > 0)
            {
                Debug.Log($"FileSelector: 从StreamingAssets找到 {txtFiles.Length} 个txt文件");
                // 返回第一个找到的txt文件
                return txtFiles[0];
            }
            else
            {
                Debug.LogWarning("FileSelector: StreamingAssets文件夹中没有找到txt文件");
            }
        }
        else
        {
            Debug.LogWarning($"FileSelector: StreamingAssets路径不存在: {streamingAssetsPath}");
        }
        return null;
    }

    // 创建一个简单的文件选择UI（当原生对话框失败时使用）
    public static void ShowFileSelectionUI(System.Action<string> onFileSelected = null)
    {
        Debug.Log("FileSelector: 显示文件选择UI");
        
        // 创建FileSelectionUI对象
        GameObject uiObject = new GameObject("FileSelectionUI");
        FileSelectionUI ui = uiObject.AddComponent<FileSelectionUI>();
        ui.Initialize(onFileSelected);
    }

    // 检查文件是否为有效的乐谱文件
    public static bool IsValidMusicSheetFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return false;

        try
        {
            string content = File.ReadAllText(filePath);
            // 简单验证：检查是否包含音符标记
            return content.Contains("1") || content.Contains("2") || content.Contains("3") || 
                   content.Contains("4") || content.Contains("5") || content.Contains("6") || 
                   content.Contains("7");
        }
        catch
        {
            return false;
        }
    }
}