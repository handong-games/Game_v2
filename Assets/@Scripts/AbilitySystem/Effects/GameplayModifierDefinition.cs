using System;
using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Effects
{
    [Serializable]
    public sealed class GameplayModifierDefinition
    {
        [SerializeField]
        private string _attributeSetTypeName;

        [SerializeField]
        private string _attributeFieldName;

        [SerializeField]
        private GameplayModifierOperation _operation = GameplayModifierOperation.Override;

        [SerializeField]
        private float _fixedMagnitude;

        public string AttributeSetTypeName => _attributeSetTypeName;
        public string AttributeFieldName => _attributeFieldName;
        public GameplayModifierOperation Operation => _operation;
        public float FixedMagnitude => _fixedMagnitude;

        public bool TryBuild(out GameplayModifier modifier)
        {
            modifier = default;

            if (string.IsNullOrWhiteSpace(_attributeSetTypeName) ||
                string.IsNullOrWhiteSpace(_attributeFieldName))
            {
                return false;
            }

            Type attributeSetType = Type.GetType(_attributeSetTypeName);
            if (attributeSetType == null)
                return false;

            if (!typeof(AttributeSet).IsAssignableFrom(attributeSetType))
                return false;

            GameplayAttribute attribute = GameplayAttribute.Create(attributeSetType, _attributeFieldName);
            if (!attribute.IsValid)
                return false;

            modifier = new GameplayModifier(attribute, _operation, _fixedMagnitude);
            return true;
        }
    }
}
