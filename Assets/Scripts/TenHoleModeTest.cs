using UnityEngine;
using System.Collections.Generic;

public class TenHoleModeTest : MonoBehaviour
{
    private ToneGenerator toneGenerator;
    private bool lastTenHoleMode = false;
    
    void Start()
    {
        toneGenerator = FindObjectOfType<ToneGenerator>();
        if (toneGenerator == null)
        {
            Debug.LogError("ToneGenerator not found!");
        }
    }
    
    void Update()
    {
        if (toneGenerator == null) return;
        
        // 检测模式切换
        bool currentTenHoleMode = toneGenerator.isTenHoleMode;
        if (currentTenHoleMode != lastTenHoleMode)
        {
            Debug.Log($"模式切换: {(currentTenHoleMode ? "十孔模式" : "八孔模式")}");
            lastTenHoleMode = currentTenHoleMode;
        }
        
        // 在十孔模式下测试按键组合
        if (currentTenHoleMode)
        {
            TestTenHoleKeyCombinations();
        }
    }
    
    void TestTenHoleKeyCombinations()
    {
        // 测试一些关键的按键组合
        var testCombinations = new Dictionary<string, KeyCode[]>
        {
            {"低音5", new KeyCode[] {KeyCode.Q, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.R, KeyCode.I, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.LeftBracket, KeyCode.C, KeyCode.M}},
            {"中音5", new KeyCode[] {KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.R, KeyCode.C, KeyCode.M}},
            {"高音1", new KeyCode[] {KeyCode.C, KeyCode.M}},
            {"高音2", new KeyCode[] {KeyCode.M}},
            {"高音3", new KeyCode[] {KeyCode.Space}}
        };
        
        foreach (var combination in testCombinations)
        {
            bool allPressed = true;
            foreach (var key in combination.Value)
            {
                if (!Input.GetKey(key))
                {
                    allPressed = false;
                    break;
                }
            }
            
            if (allPressed)
            {
                Debug.Log($"检测到按键组合: {combination.Key}");
                
                // 检查是否有额外按键被按下
                var tenHoleKeys = new KeyCode[] 
                {
                    KeyCode.Q, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, 
                    KeyCode.R, KeyCode.I, KeyCode.Alpha9, KeyCode.Alpha0, 
                    KeyCode.LeftBracket, KeyCode.C, KeyCode.M, KeyCode.Space
                };
                
                int extraKeys = 0;
                foreach (var key in tenHoleKeys)
                {
                    if (Input.GetKey(key))
                    {
                        bool isInCombination = false;
                        foreach (var combKey in combination.Value)
                        {
                            if (key == combKey)
                            {
                                isInCombination = true;
                                break;
                            }
                        }
                        if (!isInCombination)
                        {
                            extraKeys++;
                        }
                    }
                }
                
                if (extraKeys > 0)
                {
                    Debug.LogWarning($"检测到额外按键: {extraKeys}个");
                }
            }
        }
    }
}