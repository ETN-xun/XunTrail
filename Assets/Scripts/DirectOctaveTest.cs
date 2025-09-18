using UnityEngine;

public class DirectOctaveTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 直接八度测试 ===");
        
        // 测试C4在1=C调号下
        string c4Result = ChallengeManager.FrequencyToSolfege(261.63f, 0);
        Debug.Log($"C4 在 1=C 调号下: {c4Result} (期望: 中音1)");
        Debug.Log($"C4测试通过: {c4Result == "中音1"}");
        
        // 测试F4在1=F调号下
        string f4Result = ChallengeManager.FrequencyToSolfege(349.23f, 5);
        Debug.Log($"F4 在 1=F 调号下: {f4Result} (期望: 中音1)");
        Debug.Log($"F4测试通过: {f4Result == "中音1"}");
        
        // 测试边界情况
        string c3Result = ChallengeManager.FrequencyToSolfege(130.81f, 0);
        Debug.Log($"C3 在 1=C 调号下: {c3Result} (期望: 低音1)");
        
        string c5Result = ChallengeManager.FrequencyToSolfege(523.25f, 0);
        Debug.Log($"C5 在 1=C 调号下: {c5Result} (期望: 高音1)");
        
        if (c4Result == "中音1" && f4Result == "中音1")
        {
            Debug.Log("✅ 八度显示修复成功！");
        }
        else
        {
            Debug.Log("❌ 八度显示仍有问题");
        }
    }
}