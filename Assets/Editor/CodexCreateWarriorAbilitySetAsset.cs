using Game.Data;
using UnityEditor;
using UnityEngine;

public static class CodexCreateWarriorAbilitySetAsset
{
    public static void Create()
    {
        const string abilitySetPath = "Assets/@Resources/Abilities/Sets/Warrior_AbilitySet.asset";
        const string basicAttackPath = "Assets/@Resources/Abilities/Warrior/GA_Skill_BasicAttack.asset";

        Object basicAttackAsset = AssetDatabase.LoadAssetAtPath<Object>(basicAttackPath);
        if (basicAttackAsset == null)
        {
            Debug.LogError($"ABILITY_ASSET_MISSING:{basicAttackPath}");
            EditorApplication.Exit(1);
            return;
        }

        AbilitySetModel abilitySet = AssetDatabase.LoadAssetAtPath<AbilitySetModel>(abilitySetPath);
        if (abilitySet == null)
        {
            abilitySet = ScriptableObject.CreateInstance<AbilitySetModel>();
            AssetDatabase.CreateAsset(abilitySet, abilitySetPath);
        }

        SerializedObject serializedObject = new(abilitySet);
        SerializedProperty abilities = serializedObject.FindProperty("_abilities");
        abilities.arraySize = 1;
        abilities.GetArrayElementAtIndex(0).objectReferenceValue = basicAttackAsset;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(abilitySet);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"ABILITY_SET_READY:{abilitySetPath}");
        EditorApplication.Exit(0);
    }
}
