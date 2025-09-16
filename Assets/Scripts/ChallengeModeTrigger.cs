using UnityEngine;

public class ChallengeModeTrigger : MonoBehaviour
{
    void Start()
    {
        // 延迟1秒后自动启动挑战模式
        Invoke("TriggerChallengeMode", 1f);
    }
    
    void TriggerChallengeMode()
    {
        if (ChallengeManager.Instance != null)
        {
            Debug.Log("自动触发挑战模式");
            ChallengeManager.Instance.StartChallenge();
        }
        else
        {
            Debug.LogError("ChallengeManager.Instance 未找到！");
        }
    }
}