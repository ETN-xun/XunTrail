using UnityEngine;
using UnityEngine.SceneManagement;

public class DirectSceneTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("DirectSceneTest: 开始直接测试场景切换");
        
        // 等待一帧后执行测试
        Invoke("TestSceneLoad", 1f);
    }
    
void TestSceneLoad()
    {
        Debug.Log("DirectSceneTest: 尝试通过buildIndex加载ChallengeScene");
        
        try
        {
            // 通过buildIndex加载场景
            SceneManager.LoadScene(2);
            Debug.Log("DirectSceneTest: 场景加载命令已发送 (buildIndex: 2)");
        }
        catch (System.Exception e)
        {
            Debug.LogError("DirectSceneTest: 场景加载失败 - " + e.Message);
        }
    }
}