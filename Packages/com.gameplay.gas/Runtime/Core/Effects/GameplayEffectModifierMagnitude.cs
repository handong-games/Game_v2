using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public sealed class GameplayEffectModifierMagnitude
    {
        [SerializeField]
        private GameplayModifierMagnitudeType _magnitudeType = GameplayModifierMagnitudeType.Fixed;

        [SerializeField]
        private float _fixedMagnitude;

        [SerializeField]
        private GameplayTag _setByCallerTag;

        [SerializeField]
        private GameplayAttributeBasedFloatDefinition _attributeBasedMagnitude = new();

        public GameplayModifierMagnitudeType MagnitudeType => _magnitudeType;
        public float FixedMagnitude => _fixedMagnitude;
        public GameplayTag SetByCallerTag => _setByCallerTag;

        public static GameplayEffectModifierMagnitude Fixed(float magnitude)
        {
            return new GameplayEffectModifierMagnitude
            {
                _magnitudeType = GameplayModifierMagnitudeType.Fixed,
                _fixedMagnitude = magnitude
            };
        }

        public static GameplayEffectModifierMagnitude SetByCaller(GameplayTag tag)
        {
            return new GameplayEffectModifierMagnitude
            {
                _magnitudeType = GameplayModifierMagnitudeType.SetByCaller,
                _setByCallerTag = tag
            };
        }

        public static GameplayEffectModifierMagnitude AttributeBased(GameplayAttributeBasedFloat magnitude)
        {
            return new GameplayEffectModifierMagnitude
            {
                _magnitudeType = GameplayModifierMagnitudeType.AttributeBased,
                RuntimeAttributeBasedMagnitude = magnitude
            };
        }

        public GameplayAttributeBasedFloat RuntimeAttributeBasedMagnitude { get; private set; }

        public bool TryGetAttributeBasedMagnitude(out GameplayAttributeBasedFloat magnitude)
        {
            if (RuntimeAttributeBasedMagnitude.IsValid)
            {
                magnitude = RuntimeAttributeBasedMagnitude;
                return true;
            }

            if (_attributeBasedMagnitude != null &&
                _attributeBasedMagnitude.TryBuild(out magnitude))
            {
                return true;
            }

            magnitude = default;
            return false;
        }

        public void GetAttributeCaptureDefinitions(
            List<GameplayEffectAttributeCaptureDefinition> definitions)
        {
            if (_magnitudeType != GameplayModifierMagnitudeType.AttributeBased)
                return;

            if (TryGetAttributeBasedMagnitude(out GameplayAttributeBasedFloat magnitude))
                definitions.Add(magnitude.CaptureDefinition);
        }

        public bool TryCalculateMagnitude(GameplayEffectSpec spec, out float magnitude)
        {
            switch (_magnitudeType)
            {
                case GameplayModifierMagnitudeType.SetByCaller:
                    magnitude = spec.GetSetByCallerMagnitude(_setByCallerTag);
                    return true;
                case GameplayModifierMagnitudeType.AttributeBased:
                    if (TryGetAttributeBasedMagnitude(out GameplayAttributeBasedFloat attributeBasedMagnitude))
                    {
                        return spec.AttemptCalculateAttributeBasedMagnitude(
                            attributeBasedMagnitude,
                            out magnitude);
                    }

                    magnitude = 0f;
                    return false;
                default:
                    magnitude = _fixedMagnitude;
                    return true;
            }
        }
    }
}
