using System;
using System.Collections.Generic;
using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Attributes
{
    [CreateAssetMenu(
        fileName = "AttributeSetDefaults",
        menuName = "Game/AbilitySystem/Attributes/Attribute Set Defaults")]
    public sealed class AttributeSetDefaultsDefinition : ScriptableObject
    {
        [SerializeField]
        private string _attributeSetTypeName;

        [SerializeField]
        private List<AttributeDefaultValueDefinition> _values = new();

        public string AttributeSetTypeName => _attributeSetTypeName;
        public IReadOnlyList<AttributeDefaultValueDefinition> Values => _values;

        public Type GetAttributeSetType()
        {
            if (string.IsNullOrWhiteSpace(_attributeSetTypeName))
                return null;

            return Type.GetType(_attributeSetTypeName);
        }

        public bool TryGetValue(GameplayAttribute attribute, out float value)
        {
            value = 0f;

            Type setType = GetAttributeSetType();
            if (setType == null || setType != attribute.AttributeSetType)
                return false;

            for (int i = 0; i < _values.Count; i++)
            {
                AttributeDefaultValueDefinition row = _values[i];
                if (!string.Equals(row.AttributeFieldName, attribute.FieldName, StringComparison.Ordinal))
                    continue;

                value = row.Value;
                return true;
            }

            return false;
        }

        public void ApplyTo(AttributeSet attributeSet)
        {
            if (attributeSet == null)
                return;

            Type setType = GetAttributeSetType();
            if (setType == null || attributeSet.GetType() != setType)
                return;

            for (int i = 0; i < _values.Count; i++)
            {
                AttributeDefaultValueDefinition row = _values[i];
                GameplayAttribute attribute = GameplayAttribute.Create(setType, row.AttributeFieldName);
                if (!attribute.IsValid)
                    continue;

                if (!attributeSet.TryGetAttributeData(attribute, out GameplayAttributeData data))
                    continue;

                data.SetBaseValue(row.Value);
                data.SetCurrentValue(row.Value);
            }
        }
    }
}
