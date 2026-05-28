namespace Gameplay.GAS
{
    using System.Collections.Generic;

    public sealed class GameplayEffectExecutionParameters
    {
        private readonly Dictionary<GameplayEffectAttributeCaptureDefinition, float>
            _scopedCapturedAttributeMagnitudes = new();
        private readonly Dictionary<GameplayTag, float> _transientAggregatorMagnitudes = new();

        public GameplayEffectExecutionParameters(GameplayEffectSpec spec, int stackCount = 1)
            : this(spec, (GameplayTagContainer)null, stackCount)
        {
        }

        public GameplayEffectExecutionParameters(
            GameplayEffectSpec spec,
            GameplayTagContainer passedInTags,
            int stackCount = 1)
            : this(spec, null, passedInTags, stackCount)
        {
        }

        public GameplayEffectExecutionParameters(
            GameplayEffectSpec spec,
            GameplayEffectExecutionDefinition executionDefinition,
            int stackCount = 1)
            : this(spec, executionDefinition, executionDefinition?.PassedInTags, stackCount)
        {
        }

        public GameplayEffectExecutionParameters(
            GameplayEffectSpec spec,
            GameplayEffectExecutionDefinition executionDefinition,
            GameplayTagContainer passedInTags,
            int stackCount = 1)
        {
            Spec = spec;
            ExecutionDefinition = executionDefinition;
            PassedInTags = passedInTags ?? new GameplayTagContainer();
            StackCount = stackCount;

            BuildScopedModifierAggregators();
        }

        public GameplayEffectSpec Spec { get; }
        public GameplayEffectExecutionDefinition ExecutionDefinition { get; }
        public GameplayEffectContext Context => Spec.Context;
        public AbilitySystemComponent Source => Context?.Source;
        public AbilitySystemComponent Target => Context?.Target;
        public GameplayTagContainer PassedInTags { get; }
        public int StackCount { get; }

        public float GetSetByCallerMagnitude(GameplayTag tag, float defaultValue = 0f)
        {
            return Spec.GetSetByCallerMagnitude(tag, defaultValue);
        }

        public bool TryGetSourceAttribute(GameplayAttribute attribute, out GameplayAttributeData data)
        {
            if (Source != null)
                return Source.TryGetAttributeData(attribute, out data);

            data = null;
            return false;
        }

        public bool TryGetTargetAttribute(GameplayAttribute attribute, out GameplayAttributeData data)
        {
            if (Target != null)
                return Target.TryGetAttributeData(attribute, out data);

            data = null;
            return false;
        }

        public bool AttemptCalculateCapturedAttributeMagnitude(
            GameplayEffectAttributeCaptureDefinition definition,
            out float magnitude)
        {
            if (!definition.IsValid)
            {
                magnitude = 0f;
                return false;
            }

            if (_scopedCapturedAttributeMagnitudes.TryGetValue(definition, out magnitude))
                return true;

            return TryCalculateCapturedAttributeMagnitudeRaw(definition, out magnitude);
        }

        public bool AttemptCalculateTransientAggregatorMagnitude(
            GameplayTag tag,
            out float magnitude)
        {
            if (tag.IsValid &&
                _transientAggregatorMagnitudes.TryGetValue(tag, out magnitude))
            {
                return true;
            }

            magnitude = 0f;
            return false;
        }

        private bool TryCalculateCapturedAttributeMagnitudeRaw(
            GameplayEffectAttributeCaptureDefinition definition,
            out float magnitude)
        {
            if (definition.Snapshot)
                return Spec.TryGetCapturedAttributeMagnitude(definition, out magnitude);

            AbilitySystemComponent component =
                definition.Source == GameplayEffectAttributeCaptureSource.Source ? Source : Target;

            if (component != null &&
                component.TryGetAttributeData(definition.Attribute, out GameplayAttributeData data))
            {
                magnitude = data.CurrentValue;
                return true;
            }

            magnitude = 0f;
            return false;
        }

        private void BuildScopedModifierAggregators()
        {
            if (ExecutionDefinition == null)
                return;

            IReadOnlyList<GameplayEffectExecutionScopedModifierInfo> scopedModifiers =
                ExecutionDefinition.CalculationModifiers;
            if (scopedModifiers.Count == 0)
                return;

            Dictionary<GameplayEffectAttributeCaptureDefinition, GameplayAttributeAggregator> capturedAggregators =
                new();
            Dictionary<GameplayEffectAttributeCaptureDefinition, float> capturedBaseMagnitudes = new();
            Dictionary<GameplayTag, GameplayAttributeAggregator> transientAggregators = new();

            for (int i = 0; i < scopedModifiers.Count; i++)
            {
                GameplayEffectExecutionScopedModifierInfo scopedModifier = scopedModifiers[i];
                if (scopedModifier == null ||
                    !scopedModifier.RequirementsMet(Source, Target) ||
                    !scopedModifier.TryCalculateModifierMagnitude(this, out float modifierMagnitude))
                {
                    continue;
                }

                if (scopedModifier.AggregatorType ==
                    GameplayEffectScopedModifierAggregatorType.CapturedAttributeBacked)
                {
                    if (!scopedModifier.TryGetCapturedAttribute(
                            out GameplayEffectAttributeCaptureDefinition definition))
                    {
                        continue;
                    }

                    if (!capturedBaseMagnitudes.TryGetValue(definition, out float baseMagnitude) &&
                        !TryCalculateCapturedAttributeMagnitudeRaw(definition, out baseMagnitude))
                    {
                        continue;
                    }

                    if (!capturedBaseMagnitudes.ContainsKey(definition))
                        capturedBaseMagnitudes.Add(definition, baseMagnitude);

                    if (!capturedAggregators.TryGetValue(
                            definition,
                            out GameplayAttributeAggregator aggregator))
                    {
                        aggregator = new GameplayAttributeAggregator();
                        capturedAggregators.Add(definition, aggregator);
                    }

                    aggregator.AddModifier(scopedModifier.Operation, modifierMagnitude);
                    continue;
                }

                GameplayTag transientIdentifier = scopedModifier.TransientAggregatorIdentifier;
                if (!transientIdentifier.IsValid)
                    continue;

                if (!transientAggregators.TryGetValue(
                        transientIdentifier,
                        out GameplayAttributeAggregator transientAggregator))
                {
                    transientAggregator = new GameplayAttributeAggregator();
                    transientAggregators.Add(transientIdentifier, transientAggregator);
                }

                transientAggregator.AddModifier(scopedModifier.Operation, modifierMagnitude);
            }

            foreach (KeyValuePair<GameplayEffectAttributeCaptureDefinition, GameplayAttributeAggregator> pair in
                     capturedAggregators)
            {
                _scopedCapturedAttributeMagnitudes[pair.Key] =
                    pair.Value.Evaluate(capturedBaseMagnitudes[pair.Key]);
            }

            foreach (KeyValuePair<GameplayTag, GameplayAttributeAggregator> pair in transientAggregators)
            {
                _transientAggregatorMagnitudes[pair.Key] = pair.Value.Evaluate(0f);
            }
        }
    }
}
