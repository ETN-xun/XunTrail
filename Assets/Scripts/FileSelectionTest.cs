using UnityEngine;
using UnityEngine.UI;

public class FileSelectionTest : MonoBehaviour
{
    [Header("UI References")]
    public Button testButton;
    public Text resultText;
    
    void Start()
    {
        if (testButton != null)
        {
            testButton.onClick.AddListener(TestFileSelection);
        }
        
        if (resultText != null)
        {
            resultText.text = "点击按钮测试文件选择器";
        }
    }
    
    public void TestFileSelection()
    {
        Debug.Log("FileSelectionTest: 开始测试文件选择器");
        
        if (resultText != null)
        {
            resultText.text = "正在打开文件选择器...";
        }
        
        // 测试原生文件对话框
        string selectedFile = FileSelector.OpenFileDialog("测试文件选择", "文本文件\0*.txt\0所有文件\0*.*\0");
        
        if (!string.IsNullOrEmpty(selectedFile))
        {
            // 原生对话框成功
            Debug.Log($"FileSelectionTest: 原生对话框选择了文件: {selectedFile}");
            
            if (resultText != null)
            {
                resultText.text = $"原生对话框成功: {System.IO.Path.GetFileName(selectedFile)}";
            }
        }
        else
        {
            // 原生对话框失败，测试备用UI
            Debug.Log("FileSelectionTest: 原生对话框失败，测试备用UI");
            
            if (resultText != null)
            {
                resultText.text = "原生对话框失败，使用备用UI...";
            }
            
            FileSelector.ShowFileSelectionUI(OnFileSelectedFromUI);
        }
    }
    
    private void OnFileSelectedFromUI(string selectedFile)
    {
        if (!string.IsNullOrEmpty(selectedFile))
        {
            Debug.Log($"FileSelectionTest: 备用UI选择了文件: {selectedFile}");
            
            if (resultText != null)
            {
                resultText.text = $"备用UI成功: {System.IO.Path.GetFileName(selectedFile)}";
            }
        }
        else
        {
            Debug.Log("FileSelectionTest: 用户取消了文件选择");
            
            if (resultText != null)
            {
                resultText.text = "用户取消了文件选择";
            }
        }
    }
    
    // 用于在Inspector中快速测试
    [ContextMenu("Test File Selection")]
    public void TestFileSelectionFromMenu()
    {
        TestFileSelection();
    }
}