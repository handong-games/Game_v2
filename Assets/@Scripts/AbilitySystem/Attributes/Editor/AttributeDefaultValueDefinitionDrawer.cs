using System;
using System.Linq;
using System.Reflection;
using Gameplay.GAS;
using UnityEditor;
using UnityEngine;

namespace Game.AbilitySystem.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(AttributeDefaultValueDefinition))]
    public sealed class AttributeDefaultValueDefinitionDrawer : PropertyDrawer
    {
        private const float Spacing = 6f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty fieldNameProp = property.FindPropertyRelative("_attributeFieldName");
            SerializedProperty valueProp = property.FindPropertyRelative("_value");
            SerializedProperty attributeSetTypeNameProp =
                property.serializedObject.FindProperty("_attributeSetTypeName");

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float fieldWidth = position.width * 0.58f;
            Rect fieldRect = new(position.x, position.y, fieldWidth, position.height);
            Rect valueRect = new(fieldRect.xMax + Spacing, position.y, position.width - fieldWidth - Spacing, position.height);

            DrawAttributeFieldPopup(fieldRect, attributeSetTypeNameProp?.stringValue, fieldNameProp);
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private static void DrawAttributeFieldPopup(
            Rect rect,
            string attributeSetTypeName,
            SerializedProperty fieldNameProp)
        {
            Type attributeSetType = Type.GetType(attributeSetTypeName);
            if (attributeSetType == null)
            {
                EditorGUI.LabelField(rect, "-");
                return;
            }

            string[] attributeNames = attributeSetType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(IsInitializableAttributeField)
                .Select(field => field.Name)
                .ToArray();

            if (attributeNames.Length == 0)
            {
                EditorGUI.LabelField(rect, "-");
                return;
            }

            int currentIndex = Array.IndexOf(attributeNames, fieldNameProp.stringValue);
            if (currentIndex < 0)
                currentIndex = 0;

            int nextIndex = EditorGUI.Popup(rect, currentIndex, attributeNames);
            fieldNameProp.stringValue = attributeNames[nextIndex];
        }

        private static bool IsInitializableAttributeField(FieldInfo field)
        {
            return field.FieldType == typeof(GameplayAttributeData) &&
                   field.IsDefined(typeof(AttributeDefaultValueAttribute), inherit: false);
        }
    }
}
