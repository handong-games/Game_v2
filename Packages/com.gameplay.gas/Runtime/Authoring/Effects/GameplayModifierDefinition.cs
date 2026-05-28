using System;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public sealed class GameplayModifierDefinition
    {
        [SerializeField]
        private string _attributeSetTypeName;

        [SerializeField]
        private string _attributeFieldName;

        [SerializeField]
        private GameplayModifierOperation _operation = GameplayModifierOperation.Add;

        [SerializeField]
        private float _fixedMagnitude;

        [SerializeField]
        private GameplayModifierMagnitudeType _magnitudeType = GameplayModifierMagnitudeType.Fixed;

        [SerializeField]
        private GameplayTag _setByCallerTag;

        [SerializeField]
        private GameplayAttributeBasedFloatDefinition _attributeBasedMagnitude = new();

        public string AttributeSetTypeName => _attributeSetTypeName;
        public string AttributeFieldName => _attributeFieldName;
        public GameplayModifierOperation Operation => _operation;
        public float FixedMagnitude => _fixedMagnitude;
        public GameplayModifierMagnitudeType MagnitudeType => _magnitudeType;
        public GameplayTag SetByCallerTag => _setByCallerTag;
        public GameplayAttributeBasedFloatDefinition AttributeBasedMagnitude => _attributeBasedMagnitude;

        public bool TryBuild(out GameplayModifier modifier)
        {
            modifier = default;

            if (string.IsNullOrWhiteSpace(_attributeSetTypeName) ||
                string.IsNullOrWhiteSpace(_attributeFieldName))
            {
                return false;
            }

            Type attributeSetType = Type.GetType(_attributeSetTypeName);
            if (attributeSetType == null ||
                !typeof(AttributeSet).IsAssignableFrom(attributeSetType))
            {
                return false;
            }

            GameplayAttribute attribute = GameplayAttribute.Create(attributeSetType, _attributeFieldName);
            if (!attribute.IsValid)
                return false;

            GameplayEffectModifierMagnitude magnitude = BuildMagnitude();
            modifier = new GameplayModifier(attribute, _operation, magnitude);
            return true;
        }

        private GameplayEffectModifierMagnitude BuildMagnitude()
        {
            switch (_magnitudeType)
            {
                case GameplayModifierMagnitudeType.SetByCaller:
                    return GameplayEffectModifierMagnitude.SetByCaller(_setByCallerTag);
                case GameplayModifierMagnitudeType.AttributeBased:
                    return _attributeBasedMagnitude != null &&
                           _attributeBasedMagnitude.TryBuild(out GameplayAttributeBasedFloat attributeBasedMagnitude)
                        ? GameplayEffectModifierMagnitude.AttributeBased(attributeBasedMagnitude)
                        : GameplayEffectModifierMagnitude.Fixed(0f);
                default:
                    return GameplayEffectModifierMagnitude.Fixed(_fixedMagnitude);
            }
        }
    }
}
