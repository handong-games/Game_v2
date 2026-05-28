using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gameplay.GAS.Editor
{
    [CustomPropertyDrawer(typeof(GameplayCueNotifyData))]
    public sealed class GameplayCueNotifyDataDrawer : PropertyDrawer
    {
        private static readonly List<Type> NotifyTypes = TypeCache.GetTypesDerivedFrom<GameplayCueNotify>()
            .Where(type => !type.IsAbstract && !type.IsGenericType)
            .OrderBy(type => type.FullName)
            .ToList();

        private static readonly string[] NotifyTypeOptions = BuildNotifyTypeOptions();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty notifyProperty = property.FindPropertyRelative("_notify");

            float height = EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            height += notifyProperty.managedReferenceValue != null
                ? EditorGUI.GetPropertyHeight(notifyProperty, includeChildren: true)
                : EditorGUIUtility.singleLineHeight;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty cueTagProperty = property.FindPropertyRelative("_cueTag");
            SerializedProperty notifyProperty = property.FindPropertyRelative("_notify");

            Rect row = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(row, cueTagProperty, new GUIContent("Cue Tag"));

            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            DrawNotifyTypePopup(row, notifyProperty);

            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float notifyHeight = notifyProperty.managedReferenceValue != null
                ? EditorGUI.GetPropertyHeight(notifyProperty, includeChildren: true)
                : EditorGUIUtility.singleLineHeight;
            Rect notifyRect = new(position.x, row.y, position.width, notifyHeight);

            if (notifyProperty.managedReferenceValue != null)
            {
                EditorGUI.PropertyField(notifyRect, notifyProperty, new GUIContent("Notify"), includeChildren: true);
            }
            else
            {
                EditorGUI.LabelField(notifyRect, "Notify", "None");
            }

            EditorGUI.EndProperty();
        }

        private static void DrawNotifyTypePopup(Rect rect, SerializedProperty notifyProperty)
        {
            int currentIndex = 0;
            string currentTypeName = notifyProperty.managedReferenceValue?.GetType().FullName;

            for (int i = 0; i < NotifyTypes.Count; i++)
            {
                Type notifyType = NotifyTypes[i];
                if (notifyType.FullName == currentTypeName)
                    currentIndex = i + 1;
            }

            EditorGUI.BeginChangeCheck();
            int nextIndex = EditorGUI.Popup(rect, "Notify Type", currentIndex, NotifyTypeOptions);
            if (!EditorGUI.EndChangeCheck())
                return;

            if (nextIndex == 0)
            {
                notifyProperty.managedReferenceValue = null;
                return;
            }

            Type nextType = NotifyTypes[nextIndex - 1];
            notifyProperty.managedReferenceValue = Activator.CreateInstance(nextType);
        }

        private static string[] BuildNotifyTypeOptions()
        {
            string[] options = new string[NotifyTypes.Count + 1];
            options[0] = "None";

            for (int i = 0; i < NotifyTypes.Count; i++)
                options[i + 1] = NotifyTypes[i].FullName;

            return options;
        }
    }
}
