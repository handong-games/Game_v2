using System.Collections.Generic;

namespace Gameplay.GAS
{
    public sealed class GameplayEffectSpec
    {
        private readonly Dictionary<GameplayTag, float> _setByCallerMagnitudes = new();
        private readonly Dictionary<GameplayEffectAttributeCaptureDefinition, GameplayEffectAttributeCaptureSpec>
            _capturedAttributes = new();
        private readonly List<GameplayEffectModifiedAttributeData> _modifiedAttributes = new();
        private readonly List<GameplayModifierSpec> _modifierSpecs = new();

        public GameplayEffectSpec(GameplayEffect effect, GameplayEffectContext context = null, int level = 1)
        {
            Definition = effect;
            Context = context;
            Level = level;
        }

        public GameplayEffect Definition { get; }
        public GameplayEffect Effect => Definition;
        public GameplayEffectContext Context { get; }
        public int Level { get; }
        public int StackCount { get; private set; } = 1;
        public IReadOnlyList<GameplayEffectModifiedAttributeData> ModifiedAttributes => _modifiedAttributes;
        public IReadOnlyList<GameplayModifierSpec> Modifiers => _modifierSpecs;

        public void SetStackCount(int stackCount)
        {
            StackCount = stackCount < 1 ? 1 : stackCount;
        }

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

        public void SetupAttributeCaptureDefinitions(
            List<GameplayEffectAttributeCaptureDefinition> definitions)
        {
            if (definitions == null || Definition == null)
                return;

            IReadOnlyList<GameplayModifier> modifiers = Definition.Modifiers;
            for (int i = 0; i < modifiers.Count; i++)
            {
                modifiers[i].ModifierMagnitude?.GetAttributeCaptureDefinitions(definitions);
            }

            IReadOnlyList<GameplayEffectExecutionDefinition> executionDefinitions =
                Definition.ExecutionDefinitions;
            for (int i = 0; i < executionDefinitions.Count; i++)
            {
                executionDefinitions[i].GetAttributeCaptureDefinitions(definitions);
            }
        }

        public void CaptureDataFromSource()
        {
            CaptureAttributeData(GameplayEffectAttributeCaptureSource.Source);
        }

        public void CaptureDataFromTarget()
        {
            CaptureAttributeData(GameplayEffectAttributeCaptureSource.Target);
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

        public void CalculateModifierMagnitudes()
        {
            _modifierSpecs.Clear();

            if (Definition == null)
                return;

            IReadOnlyList<GameplayModifier> modifiers = Definition.Modifiers;
            for (int i = 0; i < modifiers.Count; i++)
            {
                GameplayModifier modifier = modifiers[i];
                if (!modifier.Attribute.IsValid)
                    continue;

                float magnitude = TryCalculateModifierMagnitude(modifier, out float calculatedMagnitude)
                    ? calculatedMagnitude
                    : 0f;

                _modifierSpecs.Add(new GameplayModifierSpec(modifier, magnitude));
            }
        }

        public bool TryCalculateModifierMagnitude(
            GameplayModifier modifier,
            out float magnitude)
        {
            if (modifier.ModifierMagnitude != null)
                return modifier.ModifierMagnitude.TryCalculateMagnitude(this, out magnitude);

            switch (modifier.MagnitudeType)
            {
                case GameplayModifierMagnitudeType.SetByCaller:
                    magnitude = GetSetByCallerMagnitude(modifier.SetByCallerTag);
                    return true;
                case GameplayModifierMagnitudeType.AttributeBased:
                    return AttemptCalculateAttributeBasedMagnitude(
                        modifier.AttributeBasedMagnitude,
                        out magnitude);
                default:
                    magnitude = modifier.Magnitude;
                    return true;
            }
        }

        public GameplayEffectSpec WithContext(GameplayEffectContext context)
        {
            GameplayEffectSpec spec = new(Definition, context, Level);
            spec.SetStackCount(StackCount);

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

        private void CaptureAttributeData(GameplayEffectAttributeCaptureSource captureSource)
        {
            AbilitySystemComponent component = captureSource == GameplayEffectAttributeCaptureSource.Source
                ? Context?.Source
                : Context?.Target;

            if (component == null)
                return;

            List<GameplayEffectAttributeCaptureDefinition> definitions = new();
            SetupAttributeCaptureDefinitions(definitions);

            for (int i = 0; i < definitions.Count; i++)
            {
                GameplayEffectAttributeCaptureDefinition definition = definitions[i];
                if (definition.Source == captureSource && definition.Snapshot)
                    CaptureAttribute(definition, component);
            }
        }
    }
}
