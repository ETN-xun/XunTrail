using System;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FileSelector : MonoBehaviour
{
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

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
            ofn.lpstrFilter = filter;
            ofn.lpstrFile = new string(new char[256]);
            ofn.nMaxFile = ofn.lpstrFile.Length;
            ofn.lpstrFileTitle = new string(new char[64]);
            ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
            ofn.lpstrTitle = title;
            ofn.Flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008; // OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_LONGNAMES | OFN_NOCHANGEDIR

            if (GetOpenFileName(ref ofn))
            {
                return ofn.lpstrFile;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("文件选择对话框错误: " + e.Message);
        }
        #else
        // 其他平台的备用方案
        Debug.LogWarning("当前平台不支持文件选择对话框，请将txt文件放入StreamingAssets文件夹");
        #endif
        
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
                // 返回第一个找到的txt文件
                return txtFiles[0];
            }
        }
        return null;
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