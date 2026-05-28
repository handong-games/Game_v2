using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameplay.GAS.Editor
{
    [CustomEditor(typeof(GameplayCueSet))]
    public sealed class GameplayCueSetEditor : UnityEditor.Editor
    {
        private const string GameplayCuePrefix = "GameplayCue.";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            DrawValidationMessages();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidationMessages()
        {
            List<string> errors = ValidateCueSet(serializedObject);

            if (errors.Count == 0)
            {
                EditorGUILayout.HelpBox("GameplayCueSet is valid.", MessageType.Info);
                return;
            }

            for (int i = 0; i < errors.Count; i++)
                EditorGUILayout.HelpBox(errors[i], MessageType.Error);
        }

        private static List<string> ValidateCueSet(SerializedObject serializedObject)
        {
            List<string> errors = new();
            HashSet<string> seenTags = new(StringComparer.Ordinal);

            SerializedProperty cuesProperty = serializedObject.FindProperty("_cues");
            if (cuesProperty == null || !cuesProperty.isArray)
            {
                errors.Add("Cues property is missing or invalid.");
                return errors;
            }

            for (int i = 0; i < cuesProperty.arraySize; i++)
            {
                SerializedProperty cueProperty = cuesProperty.GetArrayElementAtIndex(i);
                SerializedProperty tagProperty = cueProperty.FindPropertyRelative("_cueTag");
                SerializedProperty notifyProperty = cueProperty.FindPropertyRelative("_notify");
                string tagName = tagProperty?.FindPropertyRelative("_path")?.stringValue;

                if (string.IsNullOrWhiteSpace(tagName))
                {
                    errors.Add($"Element {i}: Cue tag is empty.");
                    continue;
                }

                if (!tagName.StartsWith(GameplayCuePrefix, StringComparison.Ordinal))
                    errors.Add($"Element {i}: Cue tag must start with '{GameplayCuePrefix}'.");

                if (!seenTags.Add(tagName))
                    errors.Add($"Element {i}: Duplicate cue tag '{tagName}'.");

                if (notifyProperty == null || notifyProperty.managedReferenceValue == null)
                    errors.Add($"Element {i}: Notify is not assigned.");
            }

            return errors;
        }
    }
}
