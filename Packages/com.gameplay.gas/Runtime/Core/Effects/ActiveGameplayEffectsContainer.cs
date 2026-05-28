using System.Collections.Generic;

namespace Gameplay.GAS
{
    internal sealed class ActiveGameplayEffectsContainer
    {
        private readonly AbilitySystemComponent _owner;
        private readonly List<ActiveGameplayEffect> _activeEffects = new();
        private int _nextEffectHandle = 1;

        public ActiveGameplayEffectsContainer(AbilitySystemComponent owner)
        {
            _owner = owner;
        }

        public IReadOnlyList<ActiveGameplayEffect> ActiveEffects => _activeEffects;

        public ActiveGameplayEffect ApplyGameplayEffectSpec(GameplayEffectSpec spec)
        {
            GameplayEffectContext context = _owner.CreateSelfApplicationContext(spec);
            GameplayEffectSpec runtimeSpec = ReferenceEquals(spec.Context, context)
                ? spec
                : spec.WithContext(context);

            runtimeSpec.CaptureDataFromSource();
            runtimeSpec.CaptureDataFromTarget();
            runtimeSpec.CalculateModifierMagnitudes();

            GameplayEffect definition = runtimeSpec.Definition;
            if (definition == null || !definition.CanApply(runtimeSpec, _owner))
            {
                return new ActiveGameplayEffect(
                    ActiveGameplayEffectHandle.Invalid,
                    runtimeSpec,
                    context);
            }

            if (definition.DurationPolicy == GameplayEffectDurationPolicy.Instant)
            {
                bool cuesHandledManually = ExecuteGameplayEffect(runtimeSpec);
                if (!cuesHandledManually)
                    _owner.InvokeGameplayCues(runtimeSpec, context, GameplayCueEvent.Executed);

                definition.OnGameplayEffectApplied(runtimeSpec, _owner);

                return new ActiveGameplayEffect(
                    new ActiveGameplayEffectHandle(_nextEffectHandle++),
                    runtimeSpec,
                    context);
            }

            if (TryApplyStack(runtimeSpec, context, out ActiveGameplayEffect stackedEffect))
                return stackedEffect;

            ActiveGameplayEffect activeEffect = new(
                new ActiveGameplayEffectHandle(_nextEffectHandle++),
                runtimeSpec,
                context);

            _owner.ApplyGrantedTags(definition.GrantedTags);
            _activeEffects.Add(activeEffect);
            definition.OnActiveGameplayEffectAdded(activeEffect, _owner);
            _owner.InvokeGameplayCues(runtimeSpec, context, GameplayCueEvent.OnActive);
            _owner.InvokeGameplayCues(runtimeSpec, context, GameplayCueEvent.WhileActive);

            if (definition.IsPeriodic)
            {
                if (definition.ExecutePeriodicEffectOnApplication)
                    ExecuteGameplayEffect(runtimeSpec);
            }
            else
            {
                RecalculateModifiedAttributes(runtimeSpec.Modifiers);
            }

            definition.OnGameplayEffectApplied(runtimeSpec, _owner);
            return activeEffect;
        }

        public bool RemoveActiveGameplayEffect(ActiveGameplayEffectHandle handle)
        {
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                ActiveGameplayEffect activeEffect = _activeEffects[i];
                if (!activeEffect.Handle.Equals(handle))
                    continue;

                GameplayEffect definition = activeEffect.Spec.Definition;
                _owner.RemoveGrantedTags(definition.GrantedTags);
                _activeEffects.RemoveAt(i);
                definition.OnGameplayEffectRemoved(activeEffect, _owner);
                _owner.InvokeGameplayCues(activeEffect.Spec, activeEffect.Context, GameplayCueEvent.Removed);

                if (!definition.IsPeriodic)
                    RecalculateModifiedAttributes(activeEffect.Spec.Modifiers);

                return true;
            }

            return false;
        }

        public void Tick(float deltaSeconds)
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveGameplayEffect activeEffect = _activeEffects[i];
                int periodTickCount = activeEffect.Tick(deltaSeconds);
                for (int j = 0; j < periodTickCount; j++)
                {
                    ExecuteGameplayEffect(activeEffect.Spec, activeEffect.StackCount);
                }

                if (activeEffect.IsExpired)
                    ExpireActiveGameplayEffect(activeEffect);
            }
        }

        public void RecalculateAttribute(GameplayAttribute attribute)
        {
            if (!_owner.TryGetAttributeStorage(attribute, out _, out GameplayAttributeData data))
                return;

            GameplayAttributeAggregator aggregator = new();
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                ActiveGameplayEffect activeEffect = _activeEffects[i];
                GameplayEffect definition = activeEffect.Spec.Definition;
                if (definition.IsPeriodic)
                    continue;

                activeEffect.Spec.CaptureDataFromSource();
                activeEffect.Spec.CaptureDataFromTarget();
                activeEffect.Spec.CalculateModifierMagnitudes();

                IReadOnlyList<GameplayModifierSpec> modifiers = activeEffect.Spec.Modifiers;
                for (int j = 0; j < modifiers.Count; j++)
                {
                    GameplayModifierSpec modifierSpec = modifiers[j];
                    GameplayModifier modifier = modifierSpec.Modifier;
                    if (modifier.Attribute.Equals(attribute))
                    {
                        aggregator.AddModifier(
                            modifier.Operation,
                            modifierSpec.EvaluatedMagnitude,
                            activeEffect.StackCount);
                    }
                }
            }

            _owner.InternalUpdateNumericalAttribute(attribute, aggregator.Evaluate(data.BaseValue));
        }

        private bool TryApplyStack(
            GameplayEffectSpec spec,
            GameplayEffectContext context,
            out ActiveGameplayEffect activeEffect)
        {
            activeEffect = null;

            GameplayEffect definition = spec.Definition;
            if (definition.StackingType == GameplayEffectStackingType.None)
                return false;

            if (!TryFindStackingActiveEffect(definition, context, out activeEffect))
                return false;

            int previousStackCount = activeEffect.StackCount;
            if (!activeEffect.TryAddStack())
                return true;

            activeEffect.Spec.SetStackCount(activeEffect.StackCount);

            if (definition.StackDurationRefreshPolicy ==
                GameplayEffectStackingDurationPolicy.RefreshOnSuccessfulApplication)
            {
                activeEffect.ResetDuration();
            }

            if (definition.StackPeriodResetPolicy ==
                GameplayEffectStackingPeriodPolicy.ResetOnSuccessfulApplication)
            {
                activeEffect.ResetPeriod();
            }

            if (definition.IsPeriodic)
            {
                if (definition.ExecutePeriodicEffectOnApplication)
                    ExecuteGameplayEffect(activeEffect.Spec, activeEffect.StackCount);
            }
            else if (activeEffect.StackCount != previousStackCount)
            {
                RecalculateModifiedAttributes(activeEffect.Spec.Modifiers);
            }

            definition.OnGameplayEffectApplied(spec, _owner);
            return true;
        }

        private bool TryFindStackingActiveEffect(
            GameplayEffect definition,
            GameplayEffectContext context,
            out ActiveGameplayEffect activeEffect)
        {
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                activeEffect = _activeEffects[i];
                if (!ReferenceEquals(activeEffect.Spec.Definition, definition))
                    continue;

                if (definition.StackingType == GameplayEffectStackingType.AggregateByTarget)
                    return true;

                if (definition.StackingType == GameplayEffectStackingType.AggregateBySource &&
                    ReferenceEquals(activeEffect.Context.Source, context.Source))
                {
                    return true;
                }
            }

            activeEffect = null;
            return false;
        }

        private void ExpireActiveGameplayEffect(ActiveGameplayEffect activeEffect)
        {
            GameplayEffect definition = activeEffect.Spec.Definition;
            if (activeEffect.StackCount > 1)
            {
                switch (definition.StackExpirationPolicy)
                {
                    case GameplayEffectStackingExpirationPolicy.RemoveSingleStackAndRefreshDuration:
                        activeEffect.RemoveStack();
                        activeEffect.Spec.SetStackCount(activeEffect.StackCount);
                        activeEffect.ResetDuration();
                        if (!definition.IsPeriodic)
                            RecalculateModifiedAttributes(activeEffect.Spec.Modifiers);
                        return;
                    case GameplayEffectStackingExpirationPolicy.RefreshDuration:
                        activeEffect.ResetDuration();
                        return;
                }
            }

            if (definition.StackExpirationPolicy == GameplayEffectStackingExpirationPolicy.RefreshDuration)
            {
                activeEffect.ResetDuration();
                return;
            }

            RemoveActiveGameplayEffect(activeEffect.Handle);
        }

        private bool ExecuteGameplayEffect(GameplayEffectSpec spec, int stackCount = 1)
        {
            bool cuesHandledManually = false;
            spec.ClearModifiedAttributes();
            spec.SetStackCount(stackCount);
            spec.CalculateModifierMagnitudes();

            for (int stackIndex = 0; stackIndex < stackCount; stackIndex++)
            {
                IReadOnlyList<GameplayModifierSpec> modifiers = spec.Modifiers;
                for (int i = 0; i < modifiers.Count; i++)
                {
                    ApplyEvaluatedModifier(spec, modifiers[i].EvaluatedData);
                }
            }

            IReadOnlyList<GameplayEffectExecutionDefinition> executionDefinitions =
                spec.Definition.ExecutionDefinitions;
            for (int i = 0; i < executionDefinitions.Count; i++)
            {
                GameplayEffectExecutionDefinition executionDefinition = executionDefinitions[i];
                GameplayEffectExecutionCalculation calculation = executionDefinition.Calculation;
                if (calculation == null)
                    continue;

                GameplayEffectExecutionParameters parameters = new(
                    spec,
                    executionDefinition,
                    stackCount);

                GameplayEffectExecutionOutput output = new();
                calculation.Execute(parameters, output);
                spec.Definition.OnGameplayEffectExecuted(spec, output, _owner);

                int outputStackCount = output.IsStackCountHandledManually ? 1 : stackCount;
                for (int stackIndex = 0; stackIndex < outputStackCount; stackIndex++)
                {
                    ApplyEvaluatedModifiers(spec, output.Modifiers);
                }

                if (output.ShouldTriggerConditionalGameplayEffects)
                    ApplyConditionalGameplayEffects(spec, executionDefinition);

                cuesHandledManually |= output.AreGameplayCuesHandledManually;
            }

            return cuesHandledManually;
        }

        private void ApplyConditionalGameplayEffects(
            GameplayEffectSpec spec,
            GameplayEffectExecutionDefinition executionDefinition)
        {
            IReadOnlyList<ConditionalGameplayEffect> conditionalEffects =
                executionDefinition.ConditionalGameplayEffects;
            if (conditionalEffects.Count == 0)
                return;

            AbilitySystemComponent source = spec.Context?.Source ?? _owner;
            for (int i = 0; i < conditionalEffects.Count; i++)
            {
                ConditionalGameplayEffect conditionalEffect = conditionalEffects[i];
                if (conditionalEffect == null ||
                    !conditionalEffect.RequirementsMet(source, _owner))
                {
                    continue;
                }

                GameplayEffectSpec conditionalSpec =
                    source.MakeOutgoingSpec(conditionalEffect.Effect, spec.Level);
                source.ApplyGameplayEffectSpecToTarget(conditionalSpec, _owner);
            }
        }

        private void ApplyEvaluatedModifiers(
            GameplayEffectSpec spec,
            IReadOnlyList<GameplayModifierEvaluatedData> modifiers)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                ApplyEvaluatedModifier(spec, modifiers[i]);
            }
        }

        private void ApplyEvaluatedModifier(
            GameplayEffectSpec spec,
            GameplayModifierEvaluatedData modifier)
        {
            if (!_owner.TryGetAttributeStorage(modifier.Attribute, out AttributeSet attributeSet, out _))
                return;

            GameplayEffectModCallbackData callbackData = new(spec, modifier, _owner);
            _owner.ApplyModToAttribute(modifier.Attribute, modifier.Operation, modifier.Magnitude);
            spec.AddOrAccumulateModifiedAttribute(modifier.Attribute, modifier.Magnitude);
            attributeSet.PostGameplayEffectExecute(callbackData);
        }

        private void RecalculateModifiedAttributes(IReadOnlyList<GameplayModifierSpec> modifiers)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                RecalculateAttribute(modifiers[i].Modifier.Attribute);
            }
        }
    }
}
