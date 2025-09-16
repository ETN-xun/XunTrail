using UnityEngine;
using UnityEditor;
using System.Linq;

[InitializeOnLoad]
public class AddChallengeSceneToBuild
{
    static AddChallengeSceneToBuild()
    {
        AddSceneToBuildSettings();
    }

    [MenuItem("Tools/Add ChallengeScene to Build Settings")]
    static void AddSceneToBuildSettings()
    {
        string scenePath = "Assets/Scenes/ChallengeScene.unity";
        
        // 检查场景是否已经在构建设置中
        var buildScenes = EditorBuildSettings.scenes.ToList();
        bool sceneExists = buildScenes.Any(scene => scene.path == scenePath);
        
        if (!sceneExists)
        {
            // 添加场景到构建设置
            buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log($"已添加 {scenePath} 到构建设置");
        }
        else
        {
            Debug.Log($"{scenePath} 已经在构建设置中");
        }
    }
}