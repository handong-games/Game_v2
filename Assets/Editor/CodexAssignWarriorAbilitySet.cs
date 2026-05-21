using Game.Data;
using UnityEditor;
using UnityEngine;

public static class CodexAssignWarriorAbilitySet
{
    public static void Assign()
    {
        const string warriorPath = "Assets/@Resources/Characters/Data/Character_Warrior.asset";
        const string abilitySetPath = "Assets/@Resources/Abilities/Sets/Warrior_AbilitySet.asset";

        CharacterModel warrior = AssetDatabase.LoadAssetAtPath<CharacterModel>(warriorPath);
        AbilitySetModel abilitySet = AssetDatabase.LoadAssetAtPath<AbilitySetModel>(abilitySetPath);

        if (warrior == null)
        {
            Debug.LogError($"WARRIOR_ASSET_MISSING:{warriorPath}");
            EditorApplication.Exit(1);
            return;
        }

        if (abilitySet == null)
        {
            Debug.LogError($"ABILITY_SET_ASSET_MISSING:{abilitySetPath}");
            EditorApplication.Exit(1);
            return;
        }

        SerializedObject serializedObject = new(warrior);
        SerializedProperty abilitySetProperty = serializedObject.FindProperty("_abilitySet");
        if (abilitySetProperty == null)
        {
            Debug.LogError("ABILITY_SET_PROPERTY_MISSING:_abilitySet");
            EditorApplication.Exit(1);
            return;
        }

        abilitySetProperty.objectReferenceValue = abilitySet;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(warrior);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"WARRIOR_ABILITY_SET_ASSIGNED:{abilitySetPath}");
        EditorApplication.Exit(0);
    }
}
