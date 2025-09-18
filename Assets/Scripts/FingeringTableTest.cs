using System;
using System.Collections.Generic;
using UnityEngine;

public class FingeringTableTest : MonoBehaviour
{
    // 模拟按键状态
    private Dictionary<KeyCode, bool> _testKeyStates = new Dictionary<KeyCode, bool>();
    
    void Start()
    {
        Debug.Log("开始测试新指法表...");
        TestNewFingeringTable();
    }
    
    void TestNewFingeringTable()
    {
        // 测试新指法表的按键组合
        var testCases = new List<(string noteName, KeyCode[] keys, string expectedNote)>
        {
            ("低音5", new KeyCode[] { KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon }, "低音5"),
            ("低音5#", new KeyCode[] { KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.P }, "低音5#"),
            ("低音6", new KeyCode[] { KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I, KeyCode.O }, "低音6"),
            ("低音6#", new KeyCode[] { KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.O, KeyCode.Semicolon }, "低音6#"),
            ("低音7", new KeyCode[] { KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I, KeyCode.Semicolon }, "低音7"),
            ("中音1", new KeyCode[] { KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I }, "中音1"),
            ("中音1#", new KeyCode[] { KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.Semicolon }, "中音1#"),
            ("中音2", new KeyCode[] { KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J }, "中音2"),
            ("中音2#", new KeyCode[] { KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.Semicolon }, "中音2#"),
            ("中音3", new KeyCode[] { KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J }, "中音3"),
            ("中音4", new KeyCode[] { KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I, KeyCode.O }, "中音4"),
            ("中音4#", new KeyCode[] { KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I }, "中音4#"),
            ("中音5", new KeyCode[] { KeyCode.E, KeyCode.F, KeyCode.J }, "中音5"),
            ("中音5#", new KeyCode[] { KeyCode.F, KeyCode.J, KeyCode.I }, "中音5#"),
            ("中音6", new KeyCode[] { KeyCode.F, KeyCode.J }, "中音6"),
            ("中音6#", new KeyCode[] { KeyCode.J, KeyCode.I, KeyCode.O }, "中音6#"),
            ("中音7", new KeyCode[] { KeyCode.J }, "中音7"),
            ("高音1", new KeyCode[] { }, "高音1")
        };
        
        Debug.Log("=== 新指法表测试结果 ===");
        
        foreach (var testCase in testCases)
        {
            // 重置按键状态
            _testKeyStates.Clear();
            
            // 设置当前测试的按键状态
            foreach (var key in testCase.keys)
            {
                _testKeyStates[key] = true;
            }
            
            // 测试GetBaseFrequency方法的逻辑
            string detectedNote = GetDetectedNote();
            
            bool isCorrect = detectedNote == testCase.expectedNote;
            string result = isCorrect ? "✓ 正确" : "✗ 错误";
            
            Debug.Log($"{result} - {testCase.noteName}: 期望 '{testCase.expectedNote}', 检测到 '{detectedNote}'");
            
            if (!isCorrect)
            {
                Debug.LogError($"指法表错误: {testCase.noteName} 应该检测为 '{testCase.expectedNote}', 但检测为 '{detectedNote}'");
            }
        }
        
        Debug.Log("=== 指法表测试完成 ===");
    }
    
    // 模拟ToneGenerator中GetBaseFrequency的逻辑
    private string GetDetectedNote()
    {
        // 按照新指法表的优先级检查按键组合
        if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon))
            return "低音5";
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.P))
            return "低音5#";
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I, KeyCode.O))
            return "低音6";
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.O, KeyCode.Semicolon))
            return "低音6#";
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I, KeyCode.Semicolon))
            return "低音7";
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I))
            return "中音1";
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.Semicolon))
            return "中音1#";
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J))
            return "中音2";
        else if (CheckKeys(KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.Semicolon))
            return "中音2#";
        else if (CheckKeys(KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J))
            return "中音3";
        else if (CheckKeys(KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I, KeyCode.O))
            return "中音4";
        else if (CheckKeys(KeyCode.E, KeyCode.F, KeyCode.J, KeyCode.I))
            return "中音4#";
        else if (CheckKeys(KeyCode.E, KeyCode.F, KeyCode.J))
            return "中音5";
        else if (CheckKeys(KeyCode.F, KeyCode.J, KeyCode.I))
            return "中音5#";
        else if (CheckKeys(KeyCode.F, KeyCode.J))
            return "中音6";
        else if (CheckKeys(KeyCode.J, KeyCode.I, KeyCode.O))
            return "中音6#";
        else if (_testKeyStates.GetValueOrDefault(KeyCode.J))
            return "中音7";
        else
            return "高音1";
    }
    
    // 模拟CheckKeys方法
    private bool CheckKeys(params KeyCode[] keys)
    {
        foreach (var key in keys)
        {
            if (!_testKeyStates.GetValueOrDefault(key))
                return false;
        }
        return true;
    }
}