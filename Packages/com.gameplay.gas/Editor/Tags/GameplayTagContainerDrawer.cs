using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.GAS.Editor
{
    [CustomPropertyDrawer(typeof(GameplayTagContainer))]
    public sealed class GameplayTagContainerDrawer : PropertyDrawer
    {
        private const float Spacing = 4f;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty tagsProperty = property.FindPropertyRelative("_tags");
            GameplayTagFilterAttribute filter = GetFilterAttribute();
            string rootPath = filter?.RootPath;

            VisualElement root = new();
            root.style.flexDirection = FlexDirection.Column;

            Label titleLabel = new(property.displayName);
            root.Add(titleLabel);

            Label summaryLabel = new();
            summaryLabel.style.marginTop = 2f;
            summaryLabel.style.marginBottom = 4f;
            UpdateSummary(summaryLabel, tagsProperty);
            root.Add(summaryLabel);

            Button selectButton = new() { text = "Select Tags" };
            selectButton.clicked += () =>
            {
                GameplayTagContainerPickerWindow.Show(
                    ElementToScreenRect(selectButton),
                    rootPath,
                    ReadSelectedPaths(tagsProperty),
                    selectedTags =>
                    {
                        tagsProperty.ClearArray();

                        for (int i = 0; i < selectedTags.Count; i++)
                        {
                            tagsProperty.InsertArrayElementAtIndex(i);
                            SerializedProperty element = tagsProperty.GetArrayElementAtIndex(i);
                            element.FindPropertyRelative("_path").stringValue = selectedTags[i].Path;
                        }

                        property.serializedObject.ApplyModifiedProperties();
                        UpdateSummary(summaryLabel, tagsProperty);
                        RebuildSelectedTags(root, tagsProperty, property, summaryLabel);
                    });
            };
            root.Add(selectButton);

            RebuildSelectedTags(root, tagsProperty, property, summaryLabel);
            return root;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty tagsProperty = property.FindPropertyRelative("_tags");
            GameplayTagFilterAttribute filter = GetFilterAttribute();
            string rootPath = filter?.RootPath;

            EditorGUI.BeginProperty(position, label, property);

            Rect prefixRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(prefixRect, GUIUtility.GetControlID(FocusType.Passive), label);

            Rect summaryRect = new(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + Spacing,
                position.width,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(summaryRect, string.IsNullOrWhiteSpace(BuildSummary(tagsProperty)) ? "None" : BuildSummary(tagsProperty));

            Rect selectRect = new(
                position.x,
                summaryRect.y + EditorGUIUtility.singleLineHeight + Spacing,
                position.width,
                EditorGUIUtility.singleLineHeight);

            if (GUI.Button(selectRect, "Select Tags", EditorStyles.popup))
            {
                GameplayTagContainerPickerWindow.Show(
                    GUIRectToScreen(selectRect),
                    rootPath,
                    ReadSelectedPaths(tagsProperty),
                    selectedTags =>
                    {
                        tagsProperty.ClearArray();

                        for (int i = 0; i < selectedTags.Count; i++)
                        {
                            tagsProperty.InsertArrayElementAtIndex(i);
                            SerializedProperty element = tagsProperty.GetArrayElementAtIndex(i);
                            element.FindPropertyRelative("_path").stringValue = selectedTags[i].Path;
                        }

                        property.serializedObject.ApplyModifiedProperties();
                    });
            }

            Rect listRect = new(
                position.x,
                selectRect.y + EditorGUIUtility.singleLineHeight + Spacing,
                position.width,
                GetListHeight(tagsProperty));
            DrawSelectedTagsList(listRect, tagsProperty);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty tagsProperty = property.FindPropertyRelative("_tags");
            float height = EditorGUIUtility.singleLineHeight;
            height += Spacing + EditorGUIUtility.singleLineHeight;
            height += Spacing + EditorGUIUtility.singleLineHeight;
            height += Spacing + GetListHeight(tagsProperty);
            return height;
        }

        private GameplayTagFilterAttribute GetFilterAttribute()
        {
            object[] attributes = fieldInfo.GetCustomAttributes(typeof(GameplayTagFilterAttribute), false);
            return attributes.Length == 0 ? null : (GameplayTagFilterAttribute)attributes[0];
        }

        private static HashSet<string> ReadSelectedPaths(SerializedProperty tagsProperty)
        {
            HashSet<string> paths = new(StringComparer.Ordinal);

            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                SerializedProperty element = tagsProperty.GetArrayElementAtIndex(i);
                string path = element.FindPropertyRelative("_path").stringValue;
                if (!string.IsNullOrWhiteSpace(path))
                    paths.Add(path);
            }

            return paths;
        }

        private static string BuildSummary(SerializedProperty tagsProperty)
        {
            if (tagsProperty.arraySize == 0)
                return string.Empty;

            System.Text.StringBuilder builder = new();

            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(tagsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_path").stringValue);
            }

            return builder.ToString();
        }

        private static float GetListHeight(SerializedProperty tagsProperty)
        {
            int rowCount = Math.Max(tagsProperty.arraySize, 1);
            return rowCount * (EditorGUIUtility.singleLineHeight + Spacing) - Spacing;
        }

        private static void DrawSelectedTagsList(Rect rect, SerializedProperty tagsProperty)
        {
            if (tagsProperty.arraySize == 0)
            {
                EditorGUI.LabelField(rect, "No Tags");
                return;
            }

            Rect row = new(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                SerializedProperty element = tagsProperty.GetArrayElementAtIndex(i);
                SerializedProperty pathProperty = element.FindPropertyRelative("_path");

                Rect pathRect = new(row.x, row.y, row.width - 60f, row.height);
                Rect removeRect = new(pathRect.xMax + Spacing, row.y, 56f, row.height);

                EditorGUI.LabelField(pathRect, pathProperty.stringValue);
                if (GUI.Button(removeRect, "Remove"))
                {
                    tagsProperty.DeleteArrayElementAtIndex(i);
                    return;
                }

                row.y += EditorGUIUtility.singleLineHeight + Spacing;
            }
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

        private static void UpdateSummary(Label summaryLabel, SerializedProperty tagsProperty)
        {
            string summary = BuildSummary(tagsProperty);
            summaryLabel.text = string.IsNullOrWhiteSpace(summary) ? "None" : summary;
        }

        private static void RebuildSelectedTags(
            VisualElement root,
            SerializedProperty tagsProperty,
            SerializedProperty property,
            Label summaryLabel)
        {
            const string selectedTagsContainerName = "selected-tags-container";

            VisualElement existing = root.Q<VisualElement>(selectedTagsContainerName);
            if (existing != null)
                root.Remove(existing);

            VisualElement container = new() { name = selectedTagsContainerName };
            container.style.flexDirection = FlexDirection.Column;
            container.style.marginTop = 4f;

            if (tagsProperty.arraySize == 0)
            {
                container.Add(new Label("No Tags"));
                root.Add(container);
                return;
            }

            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                int index = i;
                SerializedProperty element = tagsProperty.GetArrayElementAtIndex(i);
                string path = element.FindPropertyRelative("_path").stringValue;

                VisualElement row = new();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 2f;

                Label pathLabel = new(path);
                pathLabel.style.flexGrow = 1f;

                Button removeButton = new(() =>
                {
                    tagsProperty.DeleteArrayElementAtIndex(index);
                    property.serializedObject.ApplyModifiedProperties();
                    UpdateSummary(summaryLabel, tagsProperty);
                    RebuildSelectedTags(root, tagsProperty, property, summaryLabel);
                })
                {
                    text = "Remove"
                };

                row.Add(pathLabel);
                row.Add(removeButton);
                container.Add(row);
            }

            root.Add(container);
        }
    }
}
