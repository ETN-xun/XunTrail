using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class Score : MonoBehaviour
{
    public float LastTime=0f;
    private int num;
    public ToneGenerator ToneGenerator;
    public ScoreRecognition ScoreRecognition;
    public string keyInput;   // 调号输入框（如"C", "F#"）
    public string noteInput;  // 音名输入框（如"A4", "F#3"）
    public Text resultText;       // 结果显示文本
    // 自然音阶对应关系（大调音阶的半音差）
    private int[] intervals = { 0, 2, 4, 5, 7, 9, 11 };
    private string[] solfegeNumbers = { "1", "2", "3", "4", "5", "6", "7" };
    // Start is called before the first frame update
    void Start()
    {
        num = -1;
        resultText.text=ConvertToSolfege(keyInput,noteInput);
    }

    // Update is called once per frame
    void Update()
    {
        if (LastTime <= 0)
        {
            num++;
            LastTime = ScoreRecognition.times[num] * 60f / ScoreRecognition.bpm;
        }
    }
    
        public string ConvertToSolfege(string keyInput, string noteInput)
    {
        try
        {
            int keyMidi = ParseKey(keyInput);    // 解析调号为MIDI编号
            int noteMidi = ParseNote(noteInput); // 解析音名为MIDI编号
            int semitoneDiff = noteMidi - keyMidi;   // 计算相对半音差

            // 计算音级并匹配唱名
            int relativeSemitone = (semitoneDiff % 12 + 12) % 12;
            int index = Array.IndexOf(intervals, relativeSemitone);
            if (index == -1) throw new Exception("音名不在调式自然音阶中");

            // 获取音区描述（低音/中音/高音）
            string register = GetRegister(noteMidi);
            return $"{register}{solfegeNumbers[index]}";
        }
        catch (Exception e)
        {
            return $"错误：{e.Message}";
        }
    }

    // 解析调号为MIDI编号（默认中央C八度）
    private int ParseKey(string key)
    {
        Match match = Regex.Match(key.ToUpper(), @"^([A-G])(#|B)?$");
        if (!match.Success) throw new Exception("调号格式错误");

        // 基础音名与半音偏移
        Dictionary<string, int> noteValues = new Dictionary<string, int> {
            {"C",0}, {"D",2}, {"E",4}, {"F",5}, {"G",7}, {"A",9}, {"B",11}
        };

        string note = match.Groups[1].Value;
        int baseValue = noteValues[note];
        baseValue += match.Groups[2].Value switch { "#" => 1, "B" => -1, _ => 0 };

        return baseValue + 4 * 12 + 12; // 中央C八度（C4=60）
    }

    // 解析音名为MIDI编号（如"A#4"）
    private int ParseNote(string note)
    {
        Match match = Regex.Match(note.ToUpper(), @"^([A-G])(#|B)?(\d+)$");
        if (!match.Success) throw new Exception("音名格式错误");

        // 解析音高和八度
        Dictionary<string, int> noteValues = new Dictionary<string, int> {
            {"C",0}, {"D",2}, {"E",4}, {"F",5}, {"G",7}, {"A",9}, {"B",11}
        };

        string notePart = match.Groups[1].Value;
        int octave = int.Parse(match.Groups[3].Value);
        int baseValue = noteValues[notePart];
        baseValue += match.Groups[2].Value switch { "#" => 1, "B" => -1, _ => 0 };

        return baseValue + octave * 12 + 12; // MIDI编号公式
    }

    // 根据MIDI编号判断音区
    private string GetRegister(int midi)
    {
        int octave = (midi - 12) / 12;
        return octave switch {
            <=3 => "低音",
            4 => "中音",
            >=5 => "高音"
        };
    }
}
