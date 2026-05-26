using System;
using UnityEngine;

namespace Game.AbilitySystem.Attributes
{
    [Serializable]
    public sealed class AttributeDefaultValueDefinition
    {
        [SerializeField]
        private string _attributeFieldName;

        [SerializeField]
        private float _value;

        public string AttributeFieldName => _attributeFieldName;
        public float Value => _value;
    }
}
