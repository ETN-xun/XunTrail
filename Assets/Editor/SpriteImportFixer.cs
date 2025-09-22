using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteImportFixer : EditorWindow
{
    [MenuItem("Tools/Fix Sprite Mesh Types")]
    public static void ShowWindow()
    {
        GetWindow<SpriteImportFixer>("Sprite Import Fixer");
    }

    [MenuItem("Tools/Fix All Sprite Mesh Types to Full Rect")]
public static void FixAllSpriteMeshTypes()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Spites" });
        int fixedCount = 0;
        int totalCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null && importer.textureType == TextureImporterType.Sprite)
            {
                totalCount++;
                
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                
                if (settings.spriteMeshType != SpriteMeshType.FullRect)
                {
                    Debug.Log($"Fixing sprite mesh type for: {path} (was {settings.spriteMeshType}, changing to FullRect)");
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    importer.SetTextureSettings(settings);
                    importer.SaveAndReimport();
                    fixedCount++;
                }
                else
                {
                    Debug.Log($"Sprite already has FullRect mesh type: {path}");
                }
            }
        }

        Debug.Log($"Sprite mesh type fix complete! Fixed {fixedCount} out of {totalCount} sprites.");
        AssetDatabase.Refresh();
    }

    void OnGUI()
    {
        GUILayout.Label("Sprite Import Settings Fixer", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This tool will fix sprite tiling issues by setting Mesh Type to Full Rect.");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Check Current Sprite Settings"))
        {
            CheckCurrentSettings();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Fix All Sprite Mesh Types"))
        {
            FixAllSpriteMeshTypes();
        }
    }

private void CheckCurrentSettings()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Spites" });
        int needsFixing = 0;
        int totalSprites = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null && importer.textureType == TextureImporterType.Sprite)
            {
                totalSprites++;
                
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                
                if (settings.spriteMeshType != SpriteMeshType.FullRect)
                {
                    needsFixing++;
                    Debug.Log($"Sprite needs fixing: {path} (current mesh type: {settings.spriteMeshType})");
                }
            }
        }

        Debug.Log($"Found {totalSprites} sprites total. {needsFixing} sprites need mesh type changed to FullRect.");
    }
}