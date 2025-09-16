using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[InitializeOnLoad]
public class FixChallengeSceneManagerReferences
{
    static FixChallengeSceneManagerReferences()
    {
        EditorApplication.delayCall += FixReferences;
    }

    [MenuItem("Tools/Fix Challenge Scene Manager References")]
    static void FixReferences()
    {
        // 检查是否在播放模式下，如果是则跳过
        if (EditorApplication.isPlaying)
        {
            return;
        }

        // 查找SceneController
        GameObject sceneController = GameObject.Find("SceneController");
        if (sceneController == null)
        {
            Debug.LogWarning("SceneController not found");
            return;
        }

        // 获取ChallengeSceneManager组件
        ChallengeSceneManager manager = sceneController.GetComponent<ChallengeSceneManager>();
        if (manager == null)
        {
            Debug.LogWarning("ChallengeSceneManager component not found");
            return;
        }

        // 查找UI元素
        GameObject selectFileButton = GameObject.Find("SelectFileButton");
        GameObject startButton = GameObject.Find("StartButton");
        GameObject backButton = GameObject.Find("BackButton");
        GameObject selectFileText = GameObject.Find("SelectFileText");

        // 使用反射来设置字段
        var type = typeof(ChallengeSceneManager);
        
        if (selectFileButton != null)
        {
            var selectFileButtonField = type.GetField("selectFileButton");
            if (selectFileButtonField != null)
            {
                selectFileButtonField.SetValue(manager, selectFileButton.GetComponent<Button>());
                Debug.Log("Set selectFileButton reference");
            }
        }

        if (startButton != null)
        {
            var startButtonField = type.GetField("startButton");
            if (startButtonField != null)
            {
                startButtonField.SetValue(manager, startButton.GetComponent<Button>());
                Debug.Log("Set startButton reference");
            }
        }

        if (backButton != null)
        {
            var backButtonField = type.GetField("backButton");
            if (backButtonField != null)
            {
                backButtonField.SetValue(manager, backButton.GetComponent<Button>());
                Debug.Log("Set backButton reference");
            }
        }

        if (selectFileText != null)
        {
            var selectFileTextField = type.GetField("selectFileText");
            if (selectFileTextField != null)
            {
                selectFileTextField.SetValue(manager, selectFileText.GetComponent<Text>());
                Debug.Log("Set selectFileText reference");
            }
        }

        // 标记场景为已修改
        EditorUtility.SetDirty(manager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("ChallengeSceneManager references fixed!");
    }
}