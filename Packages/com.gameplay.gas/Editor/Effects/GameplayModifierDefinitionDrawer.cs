using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Gameplay.GAS.Editor
{
    [CustomPropertyDrawer(typeof(GameplayModifierDefinition))]
    public sealed class GameplayModifierDefinitionDrawer : PropertyDrawer
    {
        private const float Spacing = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty attributeSetTypeNameProperty =
                property.FindPropertyRelative("_attributeSetTypeName");

            SerializedProperty attributeFieldNameProperty =
                property.FindPropertyRelative("_attributeFieldName");

            SerializedProperty operationProperty =
                property.FindPropertyRelative("_operation");

            SerializedProperty magnitudeTypeProperty =
                property.FindPropertyRelative("_magnitudeType");

            EditorGUI.BeginProperty(position, label, property);

            Rect firstLine = new(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);
            Rect secondLine = new(
                position.x,
                firstLine.yMax + Spacing,
                position.width,
                GetMagnitudeHeight(property, magnitudeTypeProperty));

            firstLine = EditorGUI.PrefixLabel(
                firstLine,
                GUIUtility.GetControlID(FocusType.Passive),
                label);

            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float columnWidth = (firstLine.width - Spacing * 3f) / 4f;

            Rect attributeSetRect = new(firstLine.x, firstLine.y, columnWidth, firstLine.height);
            Rect attributeRect = new(attributeSetRect.xMax + Spacing, firstLine.y, columnWidth, firstLine.height);
            Rect operationRect = new(attributeRect.xMax + Spacing, firstLine.y, columnWidth, firstLine.height);
            Rect magnitudeTypeRect = new(operationRect.xMax + Spacing, firstLine.y, columnWidth, firstLine.height);

            DrawAttributeSetPopup(attributeSetRect, attributeSetTypeNameProperty);
            DrawAttributePopup(attributeRect, attributeSetTypeNameProperty.stringValue, attributeFieldNameProperty);
            EditorGUI.PropertyField(operationRect, operationProperty, GUIContent.none);
            EditorGUI.PropertyField(magnitudeTypeRect, magnitudeTypeProperty, GUIContent.none);
            DrawMagnitude(secondLine, property, magnitudeTypeProperty);

            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty magnitudeTypeProperty =
                property.FindPropertyRelative("_magnitudeType");

            return EditorGUIUtility.singleLineHeight +
                   Spacing +
                   GetMagnitudeHeight(property, magnitudeTypeProperty);
        }

        private static void DrawMagnitude(
            Rect rect,
            SerializedProperty property,
            SerializedProperty magnitudeTypeProperty)
        {
            GameplayModifierMagnitudeType magnitudeType =
                (GameplayModifierMagnitudeType)magnitudeTypeProperty.enumValueIndex;

            switch (magnitudeType)
            {
                case GameplayModifierMagnitudeType.SetByCaller:
                    EditorGUI.PropertyField(
                        rect,
                        property.FindPropertyRelative("_setByCallerTag"),
                        GUIContent.none);
                    break;
                case GameplayModifierMagnitudeType.AttributeBased:
                    EditorGUI.PropertyField(
                        rect,
                        property.FindPropertyRelative("_attributeBasedMagnitude"),
                        GUIContent.none);
                    break;
                default:
                    EditorGUI.PropertyField(
                        rect,
                        property.FindPropertyRelative("_fixedMagnitude"),
                        GUIContent.none);
                    break;
            }
        }

        private static float GetMagnitudeHeight(
            SerializedProperty property,
            SerializedProperty magnitudeTypeProperty)
        {
            GameplayModifierMagnitudeType magnitudeType =
                (GameplayModifierMagnitudeType)magnitudeTypeProperty.enumValueIndex;

            return magnitudeType == GameplayModifierMagnitudeType.AttributeBased
                ? EditorGUI.GetPropertyHeight(
                    property.FindPropertyRelative("_attributeBasedMagnitude"),
                    includeChildren: true)
                : EditorGUIUtility.singleLineHeight;
        }

        private static void DrawAttributeSetPopup(Rect rect, SerializedProperty property)
        {
            Type[] attributeSetTypes = TypeCache.GetTypesDerivedFrom<AttributeSet>()
                .Where(type => !type.IsAbstract)
                .OrderBy(type => type.FullName)
                .ToArray();

            string[] options = attributeSetTypes
                .Select(type => type.Name)
                .ToArray();

            if (options.Length == 0)
            {
                EditorGUI.LabelField(rect, "-");
                return;
            }

            int currentIndex = Array.FindIndex(
                attributeSetTypes,
                type => type.AssemblyQualifiedName == property.stringValue);

            if (currentIndex < 0)
                currentIndex = 0;

            int nextIndex = EditorGUI.Popup(rect, currentIndex, options);

            if (nextIndex >= 0 && nextIndex < attributeSetTypes.Length)
                property.stringValue = attributeSetTypes[nextIndex].AssemblyQualifiedName;
        }

        private static void DrawAttributePopup(
            Rect rect,
            string attributeSetTypeName,
            SerializedProperty property)
        {
            Type attributeSetType = Type.GetType(attributeSetTypeName);
            if (attributeSetType == null)
            {
                EditorGUI.LabelField(rect, "-");
                return;
            }

            FieldInfo[] fields = attributeSetType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            string[] attributeNames = fields
                .Where(field => field.FieldType == typeof(GameplayAttributeData))
                .Select(field => field.Name)
                .ToArray();

            if (attributeNames.Length == 0)
            {
                EditorGUI.LabelField(rect, "-");
                return;
            }

            int currentIndex = Array.IndexOf(attributeNames, property.stringValue);
            if (currentIndex < 0)
                currentIndex = 0;

            int nextIndex = EditorGUI.Popup(rect, currentIndex, attributeNames);
            property.stringValue = attributeNames[nextIndex];
        }
    }
}
