using UnityEditor;
using UnityEngine;

namespace UIToolkit.Timeline.Editor
{
    [CustomEditor(typeof(TimelineComposition))]
    public sealed class TimelineCompositionInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TimelineComposition composition = (TimelineComposition)target;

            if (GUILayout.Button("Open Timeline Composition Editor"))
                TimelineCompositionEditorWindow.Open(composition);

            EditorGUILayout.Space(8f);

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_previewDocument"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindings"), true);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Composition", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Bindings", composition.Bindings.Length.ToString());
            EditorGUILayout.LabelField("Duration", $"{composition.DurationMs} ms");
        }
    }
}
