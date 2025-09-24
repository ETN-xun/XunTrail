using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class KeyChangeManagerEight : MonoBehaviour
{
    private KeyCode[] eightHole;
    public List<Button> eightHoleButtons=new List<Button>();
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
        
        for(int i=0;i<eightHoleButtons.Count;i++)
        {
            int index = i; // 创建局部变量避免闭包问题
            eightHoleButtons[i].onClick.AddListener(() => OnTargetButtonClicked(index));
        }
        
        // 设置保存和重置按钮的事件
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveKeySettings);
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetToDefault);
            
        UpdateStatusText("八孔键位设置已加载");
    }
    
    private void LoadCurrentKeySettings()
    {
        eightHole = KeySettingsManager.Instance.GetEightHoleKeys();
        Debug.Log("八孔键位设置已从KeySettingsManager加载");
    }

    // Update is called once per frame
    void Update()
    {
        if(nowButton==null)
        {
            for (int i = 0; i < eightHoleButtons.Count; i++)
            {
                eightHoleButtons[i].transform.GetChild(0).GetComponent<Text>().text = eightHole[i].ToString();
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
                    for (int i = 0; i < eightHoleButtons.Count; i++)
                    {
                        if (eightHoleButtons[i] == nowButton)
                        {
                            t = i;
                            break;
                        }
                    }
                    eightHole[t]=keyCode;
                    nowButton=null;
                    
                    // 检查是否有键位冲突
                    CheckForConflicts();
                }
            }
        }
    }
    
    private void CheckForConflicts()
    {
        if (KeySettingsManager.Instance.HasKeyConflict(eightHole))
        {
            var conflicts = KeySettingsManager.Instance.GetConflictingKeys(eightHole);
            UpdateStatusText($"警告：检测到键位冲突 - {string.Join(", ", conflicts)}");
        }
        else
        {
            UpdateStatusText("键位设置正常，无冲突");
        }
    }
    
    private void SaveKeySettings()
    {
        if (KeySettingsManager.Instance.HasKeyConflict(eightHole))
        {
            UpdateStatusText("无法保存：存在键位冲突，请先解决冲突");
            return;
        }
        
        KeySettingsManager.Instance.SetEightHoleKeys(eightHole);
        
        // 通知ToneGenerator重新加载键位设置
        ToneGenerator toneGenerator = FindObjectOfType<ToneGenerator>();
        if (toneGenerator != null)
        {
            toneGenerator.LoadDynamicKeySettings();
        }
        
        UpdateStatusText("八孔键位设置已保存！");
    }
    
    private void ResetToDefault()
    {
        KeySettingsManager.Instance.ResetToDefault();
        LoadCurrentKeySettings();
        UpdateStatusText("八孔键位已重置为默认设置");
    }
    
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[八孔键位管理器] {message}");
    }

    public void OnTargetButtonClicked(int n)
    {
        if (Input.GetKey(KeyCode.Joystick1Button0))
        {
            return;
        }
        Debug.Log(n);
        
        // 添加边界检查
        if (n < 0 || n >= eightHoleButtons.Count || n >= eightHole.Length)
        {
            Debug.LogError($"索引越界: n={n}, eightHoleButtons.Count={eightHoleButtons.Count}, eightHole.Length={eightHole.Length}");
            return;
        }
        
        if (nowButton == null)
        {
            nowButton = eightHoleButtons[n];
        }
        else
        {
            int t = 0;
            for (int i = 0; i < eightHoleButtons.Count; i++)
            {
                if (eightHoleButtons[i] == nowButton)
                {
                    t = i;
                    break;
                }
            }

            KeyCode temp = eightHole[t];
            eightHole[t] = eightHole[n];
            eightHole[n] = temp;
            nowButton = null;
        }
    }

    public void ToTen()
    {
        SceneManager.LoadScene("KeyChangeTen");
    }
}
