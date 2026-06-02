using UnityEditor;
using UnityEngine;

namespace UIToolkit.Timeline.Editor
{
    [CustomEditor(typeof(TimelineAsset))]
    public sealed class TimelineAssetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TimelineAsset timeline = (TimelineAsset)target;

            if (GUILayout.Button("Open Timeline Asset Editor"))
                TimelineAssetEditorWindow.Open(timeline);

            EditorGUILayout.Space(8f);

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            SerializedProperty authoring = serializedObject.FindProperty("_authoring");
            if (authoring != null)
                EditorGUILayout.PropertyField(authoring, true);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                timeline.CompileRuntimeData();
                EditorUtility.SetDirty(timeline);
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space(8f);
            DrawRuntimeSummary(timeline);
            DrawValidationMessages(timeline);
        }

        private static void DrawRuntimeSummary(TimelineAsset timeline)
        {
            TimelineRuntimeData runtime = timeline.RuntimeData;

            EditorGUILayout.LabelField("Runtime Tape", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Value Clips", runtime.ValueClipCount.ToString());
            EditorGUILayout.LabelField("Notifies", runtime.NotifyCount.ToString());
            EditorGUILayout.LabelField("Events", runtime.EventCount.ToString());
            EditorGUILayout.LabelField("Duration", $"{runtime.DurationMs} ms");
        }

        private static void DrawValidationMessages(TimelineAsset timeline)
        {
            string[] messages = timeline.ValidationMessages;
            if (messages.Length == 0)
                return;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            for (int i = 0; i < messages.Length; i++)
                EditorGUILayout.HelpBox(messages[i], MessageType.Error);
        }
    }
}
