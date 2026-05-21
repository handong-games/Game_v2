using System.IO;
using Game.AbilitySystem.Abilities.Warrior;
using UnityEditor;
using UnityEngine;

public static class CodexCreateWarriorBasicAttackAsset
{
    public static void Create()
    {
        const string assetPath = "Assets/@Resources/Abilities/Warrior/GA_Skill_BasicAttack.asset";

        Object existing = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (existing != null)
        {
            Debug.Log($"ASSET_EXISTS:{assetPath}");
            EditorApplication.Exit(0);
            return;
        }

        string directory = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
        if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
        {
            Debug.LogError($"ASSET_DIRECTORY_MISSING:{directory}");
            EditorApplication.Exit(1);
            return;
        }

        GA_Skill_BasicAttack asset = ScriptableObject.CreateInstance<GA_Skill_BasicAttack>();
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"ASSET_CREATED:{assetPath}");
        EditorApplication.Exit(0);
    }
}
