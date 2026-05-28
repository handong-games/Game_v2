using System.Collections.Generic;

namespace Gameplay.GAS
{
    public sealed class GameplayEffectSpec
    {
        private readonly Dictionary<GameplayTag, float> _setByCallerMagnitudes = new();
        private readonly Dictionary<GameplayEffectAttributeCaptureDefinition, GameplayEffectAttributeCaptureSpec>
            _capturedAttributes = new();
        private readonly List<GameplayEffectModifiedAttributeData> _modifiedAttributes = new();

        public GameplayEffectSpec(GameplayEffect effect, GameplayEffectContext context = null, int level = 1)
        {
            Effect = effect;
            Context = context;
            Level = level;
        }

        public GameplayEffect Effect { get; }
        public GameplayEffectContext Context { get; }
        public int Level { get; }
        public IReadOnlyList<GameplayEffectModifiedAttributeData> ModifiedAttributes => _modifiedAttributes;

        public void SetSetByCallerMagnitude(GameplayTag tag, float magnitude)
        {
            if (tag.IsValid)
                _setByCallerMagnitudes[tag] = magnitude;
        }

        public bool TryGetSetByCallerMagnitude(GameplayTag tag, out float magnitude)
        {
            return _setByCallerMagnitudes.TryGetValue(tag, out magnitude);
        }

        public float GetSetByCallerMagnitude(GameplayTag tag, float defaultValue = 0f)
        {
            return TryGetSetByCallerMagnitude(tag, out float magnitude) ? magnitude : defaultValue;
        }

        public void CaptureAttribute(
            GameplayEffectAttributeCaptureDefinition definition,
            AbilitySystemComponent component)
        {
            if (!definition.IsValid || !definition.Snapshot || component == null)
                return;

            if (_capturedAttributes.ContainsKey(definition))
                return;

            if (!component.TryGetAttributeData(definition.Attribute, out GameplayAttributeData data))
                return;

            _capturedAttributes.Add(definition, new GameplayEffectAttributeCaptureSpec(
                definition,
                data.BaseValue,
                data.CurrentValue));
        }

        public bool TryGetCapturedAttributeMagnitude(
            GameplayEffectAttributeCaptureDefinition definition,
            out float magnitude)
        {
            if (_capturedAttributes.TryGetValue(definition, out GameplayEffectAttributeCaptureSpec spec))
            {
                magnitude = spec.CurrentValue;
                return true;
            }

            magnitude = 0f;
            return false;
        }

        public bool AttemptCalculateAttributeBasedMagnitude(
            GameplayAttributeBasedFloat attributeBasedMagnitude,
            out float magnitude)
        {
            if (!attributeBasedMagnitude.IsValid)
            {
                magnitude = 0f;
                return false;
            }

            GameplayEffectAttributeCaptureDefinition definition =
                attributeBasedMagnitude.CaptureDefinition;

            if (definition.Snapshot)
            {
                if (!TryGetCapturedAttributeMagnitude(definition, out float capturedMagnitude))
                {
                    magnitude = 0f;
                    return false;
                }

                magnitude = attributeBasedMagnitude.Evaluate(capturedMagnitude);
                return true;
            }

            AbilitySystemComponent component = definition.Source == GameplayEffectAttributeCaptureSource.Source
                ? Context?.Source
                : Context?.Target;

            if (component == null ||
                !component.TryGetAttributeData(definition.Attribute, out GameplayAttributeData data))
            {
                magnitude = 0f;
                return false;
            }

            magnitude = attributeBasedMagnitude.Evaluate(data.CurrentValue);
            return true;
        }

        public GameplayEffectSpec WithContext(GameplayEffectContext context)
        {
            GameplayEffectSpec spec = new(Effect, context, Level);
            foreach (KeyValuePair<GameplayTag, float> setByCallerMagnitude in _setByCallerMagnitudes)
            {
                spec._setByCallerMagnitudes.Add(setByCallerMagnitude.Key, setByCallerMagnitude.Value);
            }

            foreach (KeyValuePair<GameplayEffectAttributeCaptureDefinition, GameplayEffectAttributeCaptureSpec>
                         capturedAttribute in _capturedAttributes)
            {
                spec._capturedAttributes.Add(capturedAttribute.Key, capturedAttribute.Value);
            }

            spec._modifiedAttributes.AddRange(_modifiedAttributes);

            return spec;
        }

        public GameplayEffectModifiedAttributeData? GetModifiedAttribute(GameplayAttribute attribute)
        {
            for (int i = 0; i < _modifiedAttributes.Count; i++)
            {
                GameplayEffectModifiedAttributeData modifiedAttribute = _modifiedAttributes[i];
                if (modifiedAttribute.Attribute.Equals(attribute))
                    return modifiedAttribute;
            }

            return null;
        }

        public void AddOrAccumulateModifiedAttribute(GameplayAttribute attribute, float magnitude)
        {
            for (int i = 0; i < _modifiedAttributes.Count; i++)
            {
                GameplayEffectModifiedAttributeData modifiedAttribute = _modifiedAttributes[i];
                if (!modifiedAttribute.Attribute.Equals(attribute))
                    continue;

                _modifiedAttributes[i] = new GameplayEffectModifiedAttributeData(
                    modifiedAttribute.Attribute,
                    modifiedAttribute.TotalMagnitude + magnitude);
                return;
            }

            _modifiedAttributes.Add(new GameplayEffectModifiedAttributeData(attribute, magnitude));
        }

        public void ClearModifiedAttributes()
        {
            _modifiedAttributes.Clear();
        }
    }
}
