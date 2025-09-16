using UnityEngine;

public class MusicSheetParserTest : MonoBehaviour
{
    void Start()
    {
        TestMusicSheetParser();
    }
    
    void TestMusicSheetParser()
    {
        Debug.Log("开始测试MusicSheetParser...");
        
        try
        {
            // 测试单例模式
            var parser1 = MusicSheetParser.Instance;
            var parser2 = MusicSheetParser.Instance;
            
            if (parser1 == parser2)
            {
                Debug.Log("✓ 单例模式工作正常");
            }
            else
            {
                Debug.LogError("✗ 单例模式失败");
                return;
            }
            
            // 测试解析示例乐谱
            string filePath = Application.dataPath + "/乐谱/示例乐谱.txt";
            if (System.IO.File.Exists(filePath))
            {
                Debug.Log($"找到示例乐谱文件: {filePath}");
                
                var musicSheet = MusicSheetParser.Instance.ParseMusicSheet(filePath);
                if (musicSheet != null && musicSheet.notes.Count > 0)
                {
                    Debug.Log($"✓ 乐谱解析成功! 文件名: {musicSheet.fileName}, BPM: {musicSheet.bpm}, 音符数: {musicSheet.notes.Count}");
                    
                    // 显示前几个音符
                    for (int i = 0; i < Mathf.Min(3, musicSheet.notes.Count); i++)
                    {
                        var note = musicSheet.notes[i];
                        Debug.Log($"  音符 {i+1}: {note.noteName}, 时长: {note.duration}, 频率: {note.frequency}Hz");
                    }
                }
                else
                {
                    Debug.LogError("✗ 乐谱解析失败");
                }
            }
            else
            {
                Debug.LogWarning($"示例乐谱文件不存在: {filePath}");
            }
            
            Debug.Log("MusicSheetParser测试完成!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"测试过程中发生错误: {e.Message}");
        }
    }
}