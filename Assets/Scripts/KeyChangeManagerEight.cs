using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class KeyChangeManagerEight : MonoBehaviour
{
    public KeyCode[] eightHole=new KeyCode[]{KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F,
        KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon,KeyCode.P,KeyCode.Space};
    public List<Button> eightHoleButtons=new List<Button>();
    public Button nowButton=null;
    // Start is called before the first frame update
    void Start()
    {
        for(int i=0;i<eightHoleButtons.Count;i++)
        {
            int index = i; // 创建局部变量避免闭包问题
            eightHoleButtons[i].onClick.AddListener(() => OnTargetButtonClicked(index));
        }
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
