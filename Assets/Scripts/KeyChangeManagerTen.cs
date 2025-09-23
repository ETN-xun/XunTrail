using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class KeyChangeManagerTen : MonoBehaviour
{
    public KeyCode[] tenHole=new KeyCode[]{KeyCode.Q, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.R, 
        KeyCode.I, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.LeftBracket, KeyCode.C, KeyCode.M,KeyCode.Alpha1,KeyCode.Space};
    public List<Button> tenHoleButtons=new List<Button>();
    public Button nowButton=null;
    // Start is called before the first frame update
    void Start()
    {
        for(int i=0;i<tenHoleButtons.Count;i++)
        {
            int index = i; // 创建局部变量避免闭包问题
            tenHoleButtons[i].onClick.AddListener(() => OnTargetButtonClicked(index));
        }
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
                }
            }
        }
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
