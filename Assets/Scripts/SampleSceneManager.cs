using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SampleSceneManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("SampleSceneManager: SampleScene已加载");
        
        // 检查是否有来自ChallengeDataManager的选中乐谱
        if (ChallengeDataManager.Instance != null)
        {
            MusicSheet selectedSheet = ChallengeDataManager.Instance.GetSelectedMusicSheet();
            if (selectedSheet != null)
            {
                Debug.Log($"SampleSceneManager: 检测到选中的乐谱 - {selectedSheet.name}，启动挑战模式");
                StartChallengeMode(selectedSheet);
            }
            else
            {
                Debug.Log("SampleSceneManager: 没有选中的乐谱，进入自由模式");
            }
        }
        else
        {
            Debug.Log("SampleSceneManager: ChallengeDataManager不存在，进入自由模式");
        }
    }
    
    private void StartChallengeMode(MusicSheet musicSheet)
    {
        // 启动挑战模式
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.StartChallenge(musicSheet);
            Debug.Log("SampleSceneManager: 挑战模式已启动");
        }
        else
        {
            Debug.LogWarning("SampleSceneManager: 未找到ChallengeManager实例，无法启动挑战模式");
        }
    }
    
    // 返回挑战选择界面
    public void BackToChallengeScene()
    {
        Debug.Log("SampleSceneManager: 返回挑战选择界面");
        
        // 清除选中的乐谱
        if (ChallengeDataManager.Instance != null)
        {
            ChallengeDataManager.Instance.SetSelectedMusicSheet(null);
        }
        
        SceneManager.LoadScene("ChallengeScene");
    }
    
    // 返回主菜单
    public void BackToMainMenu()
    {
        Debug.Log("SampleSceneManager: 返回主菜单");
        
        // 清除选中的乐谱
        if (ChallengeDataManager.Instance != null)
        {
            ChallengeDataManager.Instance.SetSelectedMusicSheet(null);
        }
        
        SceneManager.LoadScene("TitleScene");
    }
}