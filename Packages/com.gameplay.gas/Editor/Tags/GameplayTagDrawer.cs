using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.GAS.Editor
{
    [CustomPropertyDrawer(typeof(GameplayTag))]
    public sealed class GameplayTagDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty pathProperty = property.FindPropertyRelative("_path");
            GameplayTagFilterAttribute filter = GetFilterAttribute();
            string rootPath = filter?.RootPath;

            VisualElement root = new();
            root.style.flexDirection = FlexDirection.Row;
            root.style.alignItems = Align.Center;

            Label titleLabel = new(property.displayName);
            titleLabel.style.minWidth = 150f;
            titleLabel.style.marginRight = 4f;
            root.Add(titleLabel);

            Button button = new();
            button.style.flexGrow = 1f;
            UpdateButtonText(button, pathProperty.stringValue);
            button.clicked += () =>
            {
                GameplayTagPickerWindow.Show(
                    ElementToScreenRect(button),
                    rootPath,
                    tag =>
                    {
                        pathProperty.stringValue = tag.Path;
                        property.serializedObject.ApplyModifiedProperties();
                        UpdateButtonText(button, tag.Path);
                    });
            };
            root.Add(button);

            return root;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty pathProperty = property.FindPropertyRelative("_path");
            GameplayTagFilterAttribute filter = GetFilterAttribute();
            string rootPath = filter?.RootPath;
            string currentPath = pathProperty.stringValue;

            Rect valueRect = EditorGUI.PrefixLabel(
                position,
                GUIUtility.GetControlID(FocusType.Passive),
                label);

            if (GUI.Button(
                    valueRect,
                    string.IsNullOrWhiteSpace(currentPath) ? "None" : currentPath,
                    EditorStyles.popup))
            {
                GameplayTagPickerWindow.Show(
                    GUIRectToScreen(valueRect),
                    rootPath,
                    tag =>
                    {
                        pathProperty.stringValue = tag.Path;
                        property.serializedObject.ApplyModifiedProperties();
                    });
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private GameplayTagFilterAttribute GetFilterAttribute()
        {
            object[] attributes = fieldInfo.GetCustomAttributes(typeof(GameplayTagFilterAttribute), false);
            return attributes.Length == 0 ? null : (GameplayTagFilterAttribute)attributes[0];
        }

        private static Rect GUIRectToScreen(Rect rect)
        {
            Vector2 topLeft = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
            return new Rect(topLeft.x, topLeft.y, rect.width, rect.height);
        }

        private static Rect ElementToScreenRect(VisualElement element)
        {
            Rect rect = element.worldBound;
            EditorWindow focusedWindow = EditorWindow.focusedWindow;
            if (focusedWindow != null)
                rect.position += focusedWindow.position.position;

            return rect;
        }

        private static void UpdateButtonText(Button button, string path)
        {
            button.text = string.IsNullOrWhiteSpace(path) ? "None" : path;
        }
    }
}
