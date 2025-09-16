using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Note
{
    public string noteName;     // 音符名称，如 "C4", "A#3", "R"(休止符)
    public float duration;      // 持续拍数
    public float frequency;     // 频率（Hz）
    
    public int octave;          // 八度
public bool isRest;         // 是否为休止符
    
public Note(string name, float dur)
    {
        noteName = name;
        duration = dur;
        isRest = name == "R";
        
        // 提取八度信息
        if (!isRest && !string.IsNullOrEmpty(name) && name.Length >= 2)
        {
            string octaveStr = name.Substring(1);
            // 处理升号
            if (octaveStr.StartsWith("#"))
            {
                octaveStr = octaveStr.Substring(1);
            }
            
            if (int.TryParse(octaveStr, out int parsedOctave))
            {
                octave = parsedOctave;
            }
            else
            {
                octave = 4; // 默认八度
            }
        }
        else
        {
            octave = 4; // 默认八度
        }
        
        frequency = isRest ? 0f : NoteToFrequency(name);
    }
    
    // 将音符名称转换为频率
    private float NoteToFrequency(string noteName)
    {
        // 音符到半音数的映射（相对于C）
        Dictionary<char, int> noteToSemitone = new Dictionary<char, int>
        {
            {'C', 0}, {'D', 2}, {'E', 4}, {'F', 5}, {'G', 7}, {'A', 9}, {'B', 11}
        };
        
        if (string.IsNullOrEmpty(noteName) || noteName.Length < 2)
            return 440f; // 默认A4
            
        char note = noteName[0];
        string octaveStr = noteName.Substring(1);
        
        // 处理升号
        int sharpOffset = 0;
        if (octaveStr.StartsWith("#"))
        {
            sharpOffset = 1;
            octaveStr = octaveStr.Substring(1);
        }
        
        if (!int.TryParse(octaveStr, out int octave))
            octave = 4; // 默认第4八度
            
        if (!noteToSemitone.ContainsKey(note))
            return 440f; // 默认A4
            
        // 计算相对于A4的半音数差
        int semitoneFromC = noteToSemitone[note] + sharpOffset;
        int totalSemitones = (octave - 4) * 12 + semitoneFromC - 9; // A4是参考点
        
        // A4 = 440Hz，每个半音是2^(1/12)倍频率
        return 440f * Mathf.Pow(2f, totalSemitones / 12f);
    }
}

[System.Serializable]
public class MusicSheet
{
    public float bpm;                    // 每分钟拍数
    public List<Note> notes;             // 音符列表
    public float totalDuration;          // 总时长（秒）
    
    public string name;                  // 乐谱名称
public string fileName;              // 文件名
    
    public MusicSheet()
    {
        notes = new List<Note>();
    }
    
    // 获取指定时间点应该播放的音符
    public Note GetNoteAtTime(float timeInSeconds)
    {
        float beatDuration = 60f / bpm; // 每拍的秒数
        float currentTime = 0f;
        
        foreach (Note note in notes)
        {
            float noteDuration = note.duration * beatDuration;
            if (timeInSeconds >= currentTime && timeInSeconds < currentTime + noteDuration)
            {
                return note;
            }
            currentTime += noteDuration;
        }
        
        return null; // 超出乐谱范围
    }
    
    // 获取接下来的N个音符
    public List<Note> GetUpcomingNotes(float currentTimeInSeconds, int count = 3)
    {
        List<Note> upcomingNotes = new List<Note>();
        float beatDuration = 60f / bpm;
        float currentTime = 0f;
        bool foundCurrent = false;
        
        for (int i = 0; i < notes.Count; i++)
        {
            float noteDuration = notes[i].duration * beatDuration;
            
            if (!foundCurrent)
            {
                if (currentTimeInSeconds >= currentTime && currentTimeInSeconds < currentTime + noteDuration)
                {
                    foundCurrent = true;
                    // 从当前音符开始添加
                    for (int j = i; j < notes.Count && upcomingNotes.Count < count; j++)
                    {
                        upcomingNotes.Add(notes[j]);
                    }
                    break;
                }
            }
            
            currentTime += noteDuration;
        }
        
        return upcomingNotes;
    }
    
    // 计算总时长
    public void CalculateTotalDuration()
    {
        float beatDuration = 60f / bpm;
        totalDuration = 0f;
        
        foreach (Note note in notes)
        {
            totalDuration += note.duration * beatDuration;
        }
    }
}

public class MusicSheetParser : MonoBehaviour
{
    private static MusicSheetParser _instance;
    public static MusicSheetParser Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("MusicSheetParser");
                _instance = go.AddComponent<MusicSheetParser>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // 私有构造函数，确保单例模式
    private MusicSheetParser() { }
    
    // 解析乐谱文件
    public MusicSheet ParseMusicSheet(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"乐谱文件不存在: {filePath}");
                return null;
            }
            
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2)
            {
                Debug.LogError("乐谱文件格式错误：至少需要BPM和一个音符");
                return null;
            }
            
            MusicSheet sheet = new MusicSheet();
            sheet.fileName = Path.GetFileName(filePath);
            
            // 第一行是BPM
            if (!float.TryParse(lines[0].Trim(), out sheet.bpm))
            {
                Debug.LogError("BPM格式错误");
                return null;
            }
            
            // 解析音符
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                    continue; // 跳过空行和注释
                    
                string[] parts = line.Split(' ');
                if (parts.Length != 2)
                {
                    Debug.LogWarning($"第{i+1}行格式错误，跳过: {line}");
                    continue;
                }
                
                string noteName = parts[0];
                if (!float.TryParse(parts[1], out float duration))
                {
                    Debug.LogWarning($"第{i+1}行时长格式错误，跳过: {line}");
                    continue;
                }
                
                Note note = new Note(noteName, duration);
                sheet.notes.Add(note);
            }
            
            sheet.CalculateTotalDuration();
            Debug.Log($"成功解析乐谱: {sheet.fileName}, BPM: {sheet.bpm}, 音符数: {sheet.notes.Count}, 总时长: {sheet.totalDuration:F2}秒");
            
            return sheet;
        }
        catch (Exception e)
        {
            Debug.LogError($"解析乐谱文件时出错: {e.Message}");
            return null;
        }
    }
    
    // 从Resources文件夹加载乐谱
    public MusicSheet LoadMusicSheetFromResources(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);
        if (textAsset == null)
        {
            Debug.LogError($"无法从Resources加载乐谱: {fileName}");
            return null;
        }
        
        return ParseMusicSheetFromText(textAsset.text, fileName);
    }
    
    // 从文本内容解析乐谱
    public MusicSheet ParseMusicSheetFromText(string content, string fileName = "Unknown")
    {
        try
        {
            string[] lines = content.Split('\n');
            if (lines.Length < 2)
            {
                Debug.LogError("乐谱内容格式错误：至少需要BPM和一个音符");
                return null;
            }
            
            MusicSheet sheet = new MusicSheet();
            sheet.fileName = fileName;
            
            // 第一行是BPM
            if (!float.TryParse(lines[0].Trim(), out sheet.bpm))
            {
                Debug.LogError("BPM格式错误");
                return null;
            }
            
            // 解析音符
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                    continue;
                    
                string[] parts = line.Split(' ');
                if (parts.Length != 2)
                {
                    Debug.LogWarning($"第{i+1}行格式错误，跳过: {line}");
                    continue;
                }
                
                string noteName = parts[0];
                if (!float.TryParse(parts[1], out float duration))
                {
                    Debug.LogWarning($"第{i+1}行时长格式错误，跳过: {line}");
                    continue;
                }
                
                Note note = new Note(noteName, duration);
                sheet.notes.Add(note);
            }
            
            sheet.CalculateTotalDuration();
            return sheet;
        }
        catch (Exception e)
        {
            Debug.LogError($"解析乐谱内容时出错: {e.Message}");
            return null;
        }
    }
}