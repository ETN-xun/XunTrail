using UnityEngine;

/// <summary>
/// 编译验证脚本 - 测试ToneGenerator方法的访问权限
/// </summary>
public class CompileVerification : MonoBehaviour
{
    void Start()
    {
        // 测试ToneGenerator方法的访问权限
        ToneGenerator toneGenerator = FindObjectOfType<ToneGenerator>();
        
        if (toneGenerator != null)
        {
            // 测试GetCurrentNoteName方法访问
            string noteName = toneGenerator.GetCurrentNoteName();
            Debug.Log($"GetCurrentNoteName访问成功: {noteName}");
            
            // 测试GetFrequency方法访问
            float frequency = toneGenerator.GetFrequency();
            Debug.Log($"GetFrequency访问成功: {frequency}Hz");
            
            Debug.Log("✅ 所有方法访问权限正常！");
        }
        else
        {
            Debug.LogError("❌ 未找到ToneGenerator组件");
        }
    }
}