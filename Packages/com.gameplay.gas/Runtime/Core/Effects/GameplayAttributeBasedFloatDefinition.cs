using System;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public sealed class GameplayAttributeBasedFloatDefinition
    {
        [SerializeField]
        private GameplayEffectAttributeCaptureReference _capture = new();

        [SerializeField]
        private float _coefficient = 1f;

        [SerializeField]
        private float _preMultiplyAdditiveValue;

        [SerializeField]
        private float _postMultiplyAdditiveValue;

        public GameplayEffectAttributeCaptureReference Capture => _capture;
        public float Coefficient => _coefficient;
        public float PreMultiplyAdditiveValue => _preMultiplyAdditiveValue;
        public float PostMultiplyAdditiveValue => _postMultiplyAdditiveValue;

        public bool TryBuild(out GameplayAttributeBasedFloat attributeBasedFloat)
        {
            attributeBasedFloat = default;

            if (_capture == null || !_capture.TryBuild(out GameplayEffectAttributeCaptureDefinition capture))
                return false;

            attributeBasedFloat = new GameplayAttributeBasedFloat(
                capture,
                _coefficient,
                _preMultiplyAdditiveValue,
                _postMultiplyAdditiveValue);
            return true;
        }
    }
}
