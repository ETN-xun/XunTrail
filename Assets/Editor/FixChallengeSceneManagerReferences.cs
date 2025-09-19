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

        // 首先检查当前场景是否是 ChallengeScene
        var currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (!currentScene.name.Contains("Challenge"))
        {
            // 如果不是挑战场景，静默返回，不显示警告
            return;
        }

        // 再次确认场景中是否存在 ChallengeSceneManager 组件
        ChallengeSceneManager[] allManagers = Object.FindObjectsOfType<ChallengeSceneManager>();
        if (allManagers.Length == 0)
        {
            // 如果挑战场景中没有 ChallengeSceneManager 组件，也静默返回
            return;
        }

        // 查找SceneController
        GameObject sceneController = GameObject.Find("SceneController");
        if (sceneController == null)
        {
            // 尝试查找任何包含ChallengeSceneManager组件的GameObject
            ChallengeSceneManager[] managers = Object.FindObjectsOfType<ChallengeSceneManager>();
            if (managers.Length == 0)
            {
                // 如果没有找到ChallengeSceneManager，说明当前场景不需要修复引用
                return;
            }
            
            // 使用第一个找到的ChallengeSceneManager
            sceneController = managers[0].gameObject;
            Debug.Log($"Using GameObject '{sceneController.name}' with ChallengeSceneManager component");
        }

        // 获取ChallengeSceneManager组件
        ChallengeSceneManager manager = sceneController.GetComponent<ChallengeSceneManager>();
        if (manager == null)
        {
            Debug.LogWarning("ChallengeSceneManager component not found in Challenge scene");
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