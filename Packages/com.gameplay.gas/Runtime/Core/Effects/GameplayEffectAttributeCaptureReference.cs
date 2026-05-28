using System;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public sealed class GameplayEffectAttributeCaptureReference
    {
        [SerializeField]
        private string _attributeSetTypeName;

        [SerializeField]
        private string _attributeFieldName;

        [SerializeField]
        private GameplayEffectAttributeCaptureSource _source = GameplayEffectAttributeCaptureSource.Source;

        [SerializeField]
        private bool _snapshot;

        public string AttributeSetTypeName => _attributeSetTypeName;
        public string AttributeFieldName => _attributeFieldName;
        public GameplayEffectAttributeCaptureSource Source => _source;
        public bool Snapshot => _snapshot;

        public bool TryBuild(out GameplayEffectAttributeCaptureDefinition definition)
        {
            definition = default;

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

            definition = new GameplayEffectAttributeCaptureDefinition(attribute, _source, _snapshot);
            return true;
        }
    }
}
