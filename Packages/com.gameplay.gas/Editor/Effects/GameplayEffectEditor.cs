using UnityEditor;

namespace Gameplay.GAS.Editor
{
    [CustomEditor(typeof(GameplayEffect))]
    public sealed class GameplayEffectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDuration();
            EditorGUILayout.Space(8f);
            DrawStacking();
            EditorGUILayout.Space(8f);
            DrawList("_modifiers", "Modifiers", "Add Modifier", "Remove Last Modifier");
            EditorGUILayout.Space(8f);
            DrawList("_gameplayCues", "Gameplay Cues", "Add Gameplay Cue", "Remove Last Gameplay Cue");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDuration()
        {
            EditorGUILayout.LabelField("Duration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_durationPolicy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_durationSeconds"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_periodSeconds"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_executePeriodicEffectOnApplication"));
        }

        private void DrawStacking()
        {
            EditorGUILayout.LabelField("Stacking", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_stackingType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_stackLimitCount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_stackDurationRefreshPolicy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_stackPeriodResetPolicy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_stackExpirationPolicy"));
        }

        private void DrawList(
            string propertyName,
            string label,
            string addLabel,
            string removeLabel)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(property, true);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (EditorGUILayout.LinkButton(addLabel))
                    property.InsertArrayElementAtIndex(property.arraySize);

                using (new EditorGUI.DisabledScope(property.arraySize == 0))
                {
                    if (EditorGUILayout.LinkButton(removeLabel))
                        property.DeleteArrayElementAtIndex(property.arraySize - 1);
                }
            }
        }
    }
}
