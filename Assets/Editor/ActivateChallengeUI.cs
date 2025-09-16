using UnityEngine;
using UnityEditor;

public class ActivateChallengeUI : EditorWindow
{
    [MenuItem("Tools/Activate ChallengeUI")]
    static void ActivateUI()
    {
        // 检查是否在播放模式下，如果是则跳过
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Cannot activate UI during play mode");
            return;
        }

        // 查找所有GameObject，包括非激活的
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "ChallengeUI" && obj.scene.name != null)
            {
                obj.SetActive(true);
                Debug.Log("ChallengeUI已激活");
                
                // 标记场景为已修改
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(obj.scene);
                return;
            }
        }
        
        Debug.LogError("未找到ChallengeUI对象");
    }
}