using System;
using System.Linq;
using Gameplay.GAS;
using UnityEditor;
using UnityEngine;

namespace Game.AbilitySystem.Attributes.Editor
{
    [CustomEditor(typeof(AttributeSetDefaultsDefinition))]
    public sealed class AttributeSetDefaultsDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty attributeSetTypeNameProp =
                serializedObject.FindProperty("_attributeSetTypeName");

            SerializedProperty valuesProp =
                serializedObject.FindProperty("_values");

            DrawAttributeSetPopup(attributeSetTypeNameProp);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Default Values", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            for (int i = 0; i < valuesProp.arraySize; i++)
            {
                SerializedProperty rowProp = valuesProp.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(
                    rowProp,
                    new GUIContent($"Value {i + 1}"),
                    true);
            }

            EditorGUILayout.Space(8f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Value"))
                {
                    valuesProp.InsertArrayElementAtIndex(valuesProp.arraySize);
                }

                using (new EditorGUI.DisabledScope(valuesProp.arraySize == 0))
                {
                    if (GUILayout.Button("Remove Last"))
                    {
                        valuesProp.DeleteArrayElementAtIndex(valuesProp.arraySize - 1);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawAttributeSetPopup(SerializedProperty property)
        {
            Type[] attributeSetTypes = TypeCache.GetTypesDerivedFrom<AttributeSet>()
                .Where(type => !type.IsAbstract)
                .OrderBy(type => type.Name)
                .ToArray();

            string[] options = attributeSetTypes
                .Select(type => type.Name)
                .ToArray();

            if (options.Length == 0)
            {
                EditorGUILayout.LabelField("Attribute Set", "-");
                return;
            }

            int currentIndex = Array.FindIndex(
                attributeSetTypes,
                type => type.AssemblyQualifiedName == property.stringValue);

            if (currentIndex < 0)
                currentIndex = 0;

            int nextIndex = EditorGUILayout.Popup("Attribute Set", currentIndex, options);

            if (nextIndex >= 0 && nextIndex < attributeSetTypes.Length)
            {
                property.stringValue = attributeSetTypes[nextIndex].AssemblyQualifiedName;
            }
        }
    }
}
