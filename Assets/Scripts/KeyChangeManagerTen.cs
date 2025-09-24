using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class KeyChangeManagerTen : MonoBehaviour
{
    private KeyCode[] tenHole;
    public List<Button> tenHoleButtons=new List<Button>();
    public Button nowButton=null;
    
    [Header("UI元素")]
    public Button saveButton;
    public Button resetButton;
    public Text statusText;
    
    // Start is called before the first frame update
    void Start()
    {
        // 从KeySettingsManager加载当前键位设置
        LoadCurrentKeySettings();
        
        for(int i=0;i<tenHoleButtons.Count;i++)
        {
            int index = i; // 创建局部变量避免闭包问题
            tenHoleButtons[i].onClick.AddListener(() => OnTargetButtonClicked(index));
        }
        
        // 设置保存和重置按钮的事件
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveKeySettings);
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetToDefault);
            
        UpdateStatusText("十孔键位设置已加载");
    }
    
    private void LoadCurrentKeySettings()
    {
        tenHole = KeySettingsManager.Instance.GetTenHoleKeys();
        Debug.Log("十孔键位设置已从KeySettingsManager加载");
    }

    // Update is called once per frame
    void Update()
    {
        if(nowButton==null)
        {
            for (int i = 0; i < tenHoleButtons.Count; i++)
            {
                tenHoleButtons[i].transform.GetChild(0).GetComponent<Text>().text = tenHole[i].ToString();
            }
        }
        else
        {
            nowButton.transform.GetChild(0).GetComponent<Text>().text = "输入或交换";
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                // 检测当前按键是否被按下
                if (Input.GetKeyDown(keyCode)&&keyCode!=KeyCode.None&&keyCode!=KeyCode.Mouse0&&keyCode!=KeyCode.Mouse1
                    &&keyCode!=KeyCode.Mouse2&&keyCode!=KeyCode.Mouse3&&keyCode!=KeyCode.Mouse4&&keyCode!=KeyCode.Mouse5&&keyCode!=KeyCode.Mouse6)
                {
                    int t = 0;
                    for (int i = 0; i < tenHoleButtons.Count; i++)
                    {
                        if (tenHoleButtons[i] == nowButton)
                        {
                            t = i;
                            break;
                        }
                    }
                    tenHole[t]=keyCode;
                    nowButton=null;
                    
                    // 检查是否有键位冲突
                    CheckForConflicts();
                }
            }
        }
    }
    
    private void CheckForConflicts()
    {
        if (KeySettingsManager.Instance.HasKeyConflict(tenHole))
        {
            var conflicts = KeySettingsManager.Instance.GetConflictingKeys(tenHole);
            UpdateStatusText($"警告：检测到键位冲突 - {string.Join(", ", conflicts)}");
        }
        else
        {
            UpdateStatusText("键位设置正常，无冲突");
        }
    }
    
    private void SaveKeySettings()
    {
        if (KeySettingsManager.Instance.HasKeyConflict(tenHole))
        {
            UpdateStatusText("无法保存：存在键位冲突，请先解决冲突");
            return;
        }
        
        KeySettingsManager.Instance.SetTenHoleKeys(tenHole);
        
        // 通知ToneGenerator重新加载键位设置
        ToneGenerator toneGenerator = FindObjectOfType<ToneGenerator>();
        if (toneGenerator != null)
        {
            toneGenerator.LoadDynamicKeySettings();
        }
        
        UpdateStatusText("十孔键位设置已保存！");
    }
    
    private void ResetToDefault()
    {
        KeySettingsManager.Instance.ResetToDefault();
        LoadCurrentKeySettings();
        UpdateStatusText("十孔键位已重置为默认设置");
    }
    
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[十孔键位管理器] {message}");
    }

    public void OnTargetButtonClicked(int n)
    {
        if (Input.GetKey(KeyCode.Joystick1Button0))
        {
            return;
        }
        Debug.Log(n);
        
        // 添加边界检查
        if (n < 0 || n >= tenHoleButtons.Count || n >= tenHole.Length)
        {
            Debug.LogError($"索引越界: n={n}, tenHoleButtons.Count={tenHoleButtons.Count}, tenHole.Length={tenHole.Length}");
            return;
        }
        
        if (nowButton == null)
        {
            nowButton = tenHoleButtons[n];
        }
        else
        {
            int t = 0;
            for (int i = 0; i < tenHoleButtons.Count; i++)
            {
                if (tenHoleButtons[i] == nowButton)
                {
                    t = i;
                    break;
                }
            }

            KeyCode temp = tenHole[t];
            tenHole[t] = tenHole[n];
            tenHole[n] = temp;
            nowButton = null;
        }
    }
    
    public void ToEight()
    {
        SceneManager.LoadScene("KeyChangeEight");
    }
}
