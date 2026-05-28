using System;
using UnityEngine;

namespace Game.AbilitySystem.Attributes
{
    [Serializable]
    public sealed class AttributeDefaultValueDefinition
    {
        public AttributeDefaultValueDefinition()
        {
        }

        public AttributeDefaultValueDefinition(string attributeFieldName, float value)
        {
            _attributeFieldName = attributeFieldName;
            _value = value;
        }

        [SerializeField]
        private string _attributeFieldName;

        [SerializeField]
        private float _value;

        public string AttributeFieldName => _attributeFieldName;
        public float Value => _value;
    }
}
