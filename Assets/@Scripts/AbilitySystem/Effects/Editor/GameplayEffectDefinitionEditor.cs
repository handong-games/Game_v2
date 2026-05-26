using UnityEditor;
using UnityEngine;

namespace Game.AbilitySystem.Effects.Editor
{
    [CustomEditor(typeof(GameplayEffectDefinition))]
    public sealed class GameplayEffectDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty modifiersProp = serializedObject.FindProperty("_modifiers");

            EditorGUILayout.LabelField("Modifiers", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            for (int i = 0; i < modifiersProp.arraySize; i++)
            {
                SerializedProperty modifierProp = modifiersProp.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(
                    modifierProp,
                    new GUIContent($"Modifier {i + 1}"),
                    true);
            }

            EditorGUILayout.Space(8f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Modifier"))
                {
                    modifiersProp.InsertArrayElementAtIndex(modifiersProp.arraySize);
                }

                using (new EditorGUI.DisabledScope(modifiersProp.arraySize == 0))
                {
                    if (GUILayout.Button("Remove Last"))
                    {
                        modifiersProp.DeleteArrayElementAtIndex(modifiersProp.arraySize - 1);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
