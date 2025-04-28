using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreRecognition : MonoBehaviour
{
    public TextAsset textAsset;
    public string[] lines;

    public int bpm;

    public List<float> freqs;
    public List<float> times;

    void Start()
    {
        lines = textAsset.text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            Debug.LogError("乐谱文件为空");
            return;
        }
        bpm = Convert.ToInt32(lines[0]);
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] temp = line.Split(' ');
            if (temp.Length != 2)
            {
                Debug.LogError($"第{i+1}行格式错误：音符和拍数必须用空格分隔");
                continue;
            }

            try
            {
                freqs.Add(NameToFrequency(temp[0]));
                times.Add(Convert.ToSingle(temp[1]));
            }
            catch (Exception ex)
            {
                Debug.LogError($"第{i+1}行解析失败：{ex.Message}");
            }
        }
    }

    // 新的音名转频率方法
    public float NameToFrequency(string frequencyName)
    {
        if (frequencyName == "R")
        {
            return -1f;
        }

        // 分割音符和八度
        int splitIndex = frequencyName.Length - 1;
        while (splitIndex >= 0 && char.IsDigit(frequencyName[splitIndex]))
        {
            splitIndex--;
        }

        if (splitIndex == frequencyName.Length - 1) // 无八度信息
        {
            throw new ArgumentException($"音符名称缺少八度数字: {frequencyName}");
        }

        string notePart = frequencyName.Substring(0, splitIndex + 1).Trim();
        int octave = int.Parse(frequencyName.Substring(splitIndex + 1));

        // 确定基准键号
        int baseKey;
        switch (notePart)
        {
            case "A": baseKey = 9; break;
            case "A#": baseKey = 10; break;
            case "B": baseKey = 11; break;
            case "C": baseKey = 0; break;
            case "C#": baseKey = 1; break;
            case "D": baseKey = 2; break;
            case "D#": baseKey = 3; break;
            case "E": baseKey = 4; break;
            case "E#": baseKey = 5; break; // 处理E#的情况
            case "F": baseKey = 5; break;
            case "F#": baseKey = 6; break;
            case "G": baseKey = 7; break;
            case "G#": baseKey = 8; break;
            default:
                throw new ArgumentException($"未知的音符: {notePart}");
        }

        // 计算绝对键号
        int X = baseKey + (octave - 4) * 12;
        // 计算相对于A4的半音差
        float semitoneDiff = (X - 9);
        float frequency = 440f * (float)Mathf.Pow(2f, semitoneDiff / 12f);

        return frequency;
    }
}