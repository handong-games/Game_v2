using System;
using System.Linq;
using System.Reflection;
using Gameplay.GAS;
using UnityEditor;
using UnityEngine;

namespace Game.AbilitySystem.Effects.Editor
{
    [CustomPropertyDrawer(typeof(GameplayModifierDefinition))]
    public sealed class GameplayModifierDefinitionDrawer : PropertyDrawer
    {
        private const float Spacing = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty attributeSetTypeNameProp =
                property.FindPropertyRelative("_attributeSetTypeName");

            SerializedProperty attributeFieldNameProp =
                property.FindPropertyRelative("_attributeFieldName");

            SerializedProperty operationProp =
                property.FindPropertyRelative("_operation");

            SerializedProperty fixedMagnitudeProp =
                property.FindPropertyRelative("_fixedMagnitude");

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float columnWidth = (position.width - Spacing * 3f) / 4f;

            Rect attributeSetRect = new(position.x, position.y, columnWidth, position.height);
            Rect attributeRect = new(attributeSetRect.xMax + Spacing, position.y, columnWidth, position.height);
            Rect operationRect = new(attributeRect.xMax + Spacing, position.y, columnWidth, position.height);
            Rect magnitudeRect = new(operationRect.xMax + Spacing, position.y, columnWidth, position.height);

            DrawAttributeSetPopup(attributeSetRect, attributeSetTypeNameProp);
            DrawAttributePopup(attributeRect, attributeSetTypeNameProp.stringValue, attributeFieldNameProp);
            EditorGUI.PropertyField(operationRect, operationProp, GUIContent.none);
            EditorGUI.PropertyField(magnitudeRect, fixedMagnitudeProp, GUIContent.none);

            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private static void DrawAttributeSetPopup(Rect rect, SerializedProperty property)
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
            {
                property.stringValue = attributeSetTypes[nextIndex].AssemblyQualifiedName;
            }
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
