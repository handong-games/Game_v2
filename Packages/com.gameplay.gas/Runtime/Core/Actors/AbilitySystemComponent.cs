using System;
using System.Collections.Generic;

namespace Gameplay.GAS
{
    public sealed class AbilitySystemComponent
    {
        private readonly Dictionary<GameplayAbilitySpecHandle, GameplayAbilitySpec> _abilities = new();
        private readonly Dictionary<GameplayTag, List<GameplayAbilitySpecHandle>> _gameplayEventTriggeredAbilities =
            new();
        private readonly List<AttributeSet> _attributeSets = new();
        private readonly List<ActiveGameplayEffect> _activeEffects = new();
        private int _nextAbilityHandle = 1;
        private int _nextEffectHandle = 1;

        public AbilitySystemComponent(GameplayActor owner)
        {
            Owner = owner;
            ActorInfo = new GameplayAbilityActorInfo(owner, this);
        }

        public GameplayActor Owner { get; }
        public GameplayAbilityActorInfo ActorInfo { get; }
        public GameplayTagCountContainer OwnedTags { get; } = new();
        public IReadOnlyList<ActiveGameplayEffect> ActiveEffects => _activeEffects;
        public event Action<GameplayEventData> GameplayEventReceived;
        public event Action<GameplayCueEventData> GameplayCueReceived;

        public GameplayAbilitySpecHandle GiveAbility(GameplayAbility ability, int level = 1)
        {
            GameplayAbilitySpecHandle handle = new(_nextAbilityHandle++);
            GameplayAbilitySpec spec = new(handle, ability, level);
            _abilities.Add(handle, spec);
            RegisterAbilityTriggers(spec);
            return handle;
        }

        public bool TryGetAbilitySpec(GameplayAbilitySpecHandle handle, out GameplayAbilitySpec spec)
        {
            return _abilities.TryGetValue(handle, out spec);
        }

        public void FindAllAbilitiesWithTags(
            List<GameplayAbilitySpecHandle> outAbilityHandles,
            GameplayTagContainer tags,
            bool exactMatch = true)
        {
            if (outAbilityHandles == null)
                throw new ArgumentNullException(nameof(outAbilityHandles));

            outAbilityHandles.Clear();
            if (tags == null)
                return;

            foreach (KeyValuePair<GameplayAbilitySpecHandle, GameplayAbilitySpec> pair in _abilities)
            {
                GameplayAbilitySpecHandle handle = pair.Key;
                GameplayAbilitySpec spec = pair.Value;
                GameplayAbility ability = spec.Ability;
                if (ability == null)
                    continue;

                bool matches = exactMatch
                    ? ability.AbilityTags.HasAnyExact(tags)
                    : ability.AbilityTags.HasAny(tags);

                if (matches)
                    outAbilityHandles.Add(handle);
            }
        }

        public bool TryActivateAbility(GameplayAbilitySpecHandle handle)
        {
            if (!_abilities.TryGetValue(handle, out GameplayAbilitySpec spec))
                return false;

            GameplayAbilityActivationInfo activationInfo = GameplayAbilityActivationInfo.Default;
            if (!spec.Ability.CanActivateAbility(handle, ActorInfo))
                return false;

            if (!spec.Ability.CommitAbility(handle, ActorInfo, activationInfo))
                return false;

            spec.Ability.ActivateAbility(handle, ActorInfo, activationInfo, null);
            return true;
        }

        public int HandleGameplayEvent(GameplayEventData eventData)
        {
            int triggeredCount = TriggerAbilitiesFromGameplayEvent(eventData);
            GameplayEventReceived?.Invoke(eventData);
            return triggeredCount;
        }

        public void ExecuteGameplayCue(GameplayTag cueTag, GameplayEffectContext context = null)
        {
            ExecuteGameplayCue(cueTag, new GameplayCueParameters(context));
        }

        public void ExecuteGameplayCue(GameplayTag cueTag, GameplayCueParameters parameters)
        {
            InvokeGameplayCueEvent(cueTag, GameplayCueEvent.Executed, parameters);
        }

        public void AddGameplayCue(GameplayTag cueTag, GameplayEffectContext context = null)
        {
            AddGameplayCue(cueTag, new GameplayCueParameters(context));
        }

        public void AddGameplayCue(GameplayTag cueTag, GameplayCueParameters parameters)
        {
            InvokeGameplayCueEvent(cueTag, GameplayCueEvent.OnActive, parameters);
        }

        public void RemoveGameplayCue(GameplayTag cueTag)
        {
            InvokeGameplayCueEvent(
                cueTag,
                GameplayCueEvent.Removed,
                new GameplayCueParameters(new GameplayEffectContext(this, this)));
        }

        public void InvokeGameplayCueEvent(
            GameplayTag cueTag,
            GameplayCueEvent eventType,
            GameplayCueParameters parameters)
        {
            if (!cueTag.IsValid)
                return;

            GameplayCueReceived?.Invoke(new GameplayCueEventData(this, cueTag, eventType, parameters));
        }

        public void AddAttributeSet(AttributeSet attributeSet)
        {
            if (attributeSet != null)
                _attributeSets.Add(attributeSet);
        }

        public bool TryGetAttributeData(GameplayAttribute attribute, out GameplayAttributeData data)
        {
            for (int i = 0; i < _attributeSets.Count; i++)
            {
                if (_attributeSets[i].TryGetAttributeData(attribute, out data))
                    return true;
            }

            data = null;
            return false;
        }

        public GameplayEffectSpec MakeOutgoingSpec(GameplayEffect effect, int level = 1)
        {
            GameplayEffectContext context = new(this, null);
            GameplayEffectSpec spec = new(effect, context, level);
            CaptureSnapshotAttributes(spec, GameplayEffectAttributeCaptureSource.Source);
            return spec;
        }

        public ActiveGameplayEffect ApplyGameplayEffectSpecToTarget(
            GameplayEffectSpec spec,
            AbilitySystemComponent target)
        {
            GameplayEffectContext context = spec.Context ?? new GameplayEffectContext(this, target);
            GameplayEffectSpec targetSpec = spec.WithContext(context.WithTarget(target));
            return target.ApplyGameplayEffectSpecToSelf(targetSpec);
        }

        public ActiveGameplayEffect ApplyGameplayEffectSpecToSelf(GameplayEffectSpec spec)
        {
            GameplayEffectContext context = CreateSelfApplicationContext(spec);
            GameplayEffectSpec runtimeSpec = ReferenceEquals(spec.Context, context) ? spec : spec.WithContext(context);
            CaptureSnapshotAttributes(runtimeSpec, GameplayEffectAttributeCaptureSource.Source);
            CaptureSnapshotAttributes(runtimeSpec, GameplayEffectAttributeCaptureSource.Target);

            if (!CanApplyGameplayEffect(runtimeSpec.Effect))
            {
                return new ActiveGameplayEffect(
                    ActiveGameplayEffectHandle.Invalid,
                    runtimeSpec,
                    context);
            }

            if (runtimeSpec.Effect.DurationPolicy == GameplayEffectDurationPolicy.Instant)
            {
                ApplyInstantModifiers(runtimeSpec);
                InvokeGameplayCues(runtimeSpec, context, GameplayCueEvent.Executed);
                return new ActiveGameplayEffect(
                    new ActiveGameplayEffectHandle(_nextEffectHandle++),
                    runtimeSpec,
                    context);
            }

            if (TryApplyStack(runtimeSpec, context, out ActiveGameplayEffect stackedEffect))
                return stackedEffect;

            ActiveGameplayEffect activeEffect = new(new ActiveGameplayEffectHandle(_nextEffectHandle++), runtimeSpec, context);

            ApplyGrantedTags(runtimeSpec.Effect.GrantedTags);
            _activeEffects.Add(activeEffect);
            InvokeGameplayCues(runtimeSpec, context, GameplayCueEvent.OnActive);
            InvokeGameplayCues(runtimeSpec, context, GameplayCueEvent.WhileActive);

            if (runtimeSpec.Effect.IsPeriodic)
            {
                if (runtimeSpec.Effect.ExecutePeriodicEffectOnApplication)
                    ApplyInstantModifiers(runtimeSpec);
            }
            else
            {
                RecalculateModifiedAttributes(runtimeSpec.Effect.Modifiers);
            }

            return activeEffect;
        }

        private GameplayEffectContext CreateSelfApplicationContext(GameplayEffectSpec spec)
        {
            if (spec.Context == null)
                return new GameplayEffectContext(this, this);

            if (spec.Context.Source != null && spec.Context.Target != null)
                return spec.Context;

            return new GameplayEffectContext(
                spec.Context.Source ?? this,
                spec.Context.Target ?? this);
        }

        public bool TriggerAbilityFromGameplayEvent(
            GameplayAbilitySpecHandle handle,
            GameplayEventData eventData)
        {
            if (!_abilities.TryGetValue(handle, out GameplayAbilitySpec spec))
                return false;

            GameplayAbilityActivationInfo activationInfo = GameplayAbilityActivationInfo.Default;
            if (!spec.Ability.ShouldAbilityRespondToEvent(ActorInfo, eventData))
                return false;

            if (!spec.Ability.CanActivateAbility(handle, ActorInfo))
                return false;

            if (!spec.Ability.CommitAbility(handle, ActorInfo, activationInfo))
                return false;

            spec.Ability.ActivateAbility(handle, ActorInfo, activationInfo, eventData);
            return true;
        }

        public bool CanApplyGameplayEffect(GameplayEffect effect)
        {
            return effect.ApplicationTagRequirements.RequirementsMet(OwnedTags);
        }

        public bool HasAnyMatchingGameplayTags(GameplayTagContainer tags)
        {
            return OwnedTags.HasAny(tags);
        }

        public bool CanApplyAttributeModifiers(GameplayEffect effect, int level = 1)
        {
            if (effect == null)
                return true;

            if (!CanApplyGameplayEffect(effect))
                return false;

            GameplayEffectSpec spec = MakeOutgoingSpec(effect, level);
            IReadOnlyList<GameplayModifier> modifiers = effect.Modifiers;
            for (int i = 0; i < modifiers.Count; i++)
            {
                GameplayModifier modifier = modifiers[i];
                if (!TryGetAttributeData(modifier.Attribute, out GameplayAttributeData data))
                    return false;

                float magnitude = ResolveModifierMagnitude(modifier, spec);
                float nextValue = modifier.Operation switch
                {
                    GameplayModifierOperation.Add => data.CurrentValue + magnitude,
                    GameplayModifierOperation.Multiply => data.CurrentValue * magnitude,
                    GameplayModifierOperation.Override => magnitude,
                    _ => data.CurrentValue
                };

                if (nextValue < 0f)
                    return false;
            }

            return true;
        }

        public bool RemoveActiveGameplayEffect(ActiveGameplayEffectHandle handle)
        {
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                ActiveGameplayEffect activeEffect = _activeEffects[i];
                if (!activeEffect.Handle.Equals(handle))
                    continue;

                RemoveGrantedTags(activeEffect.Spec.Effect.GrantedTags);
                _activeEffects.RemoveAt(i);
                InvokeGameplayCues(activeEffect.Spec, activeEffect.Context, GameplayCueEvent.Removed);

                if (!activeEffect.Spec.Effect.IsPeriodic)
                    RecalculateModifiedAttributes(activeEffect.Spec.Effect.Modifiers);

                return true;
            }

            return false;
        }

        public void TickActiveGameplayEffects(float deltaSeconds)
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveGameplayEffect activeEffect = _activeEffects[i];
                int periodTickCount = activeEffect.Tick(deltaSeconds);
                for (int j = 0; j < periodTickCount; j++)
                {
                    ApplyInstantModifiers(activeEffect.Spec, activeEffect.StackCount);
                }

                if (activeEffect.IsExpired)
                    ExpireActiveGameplayEffect(activeEffect);
            }
        }

        private bool TryApplyStack(
            GameplayEffectSpec spec,
            GameplayEffectContext context,
            out ActiveGameplayEffect activeEffect)
        {
            activeEffect = null;

            if (spec.Effect.StackingType == GameplayEffectStackingType.None)
                return false;

            if (!TryFindStackingActiveEffect(spec.Effect, context, out activeEffect))
                return false;

            int previousStackCount = activeEffect.StackCount;
            if (!activeEffect.TryAddStack())
                return true;

            if (spec.Effect.StackDurationRefreshPolicy ==
                GameplayEffectStackingDurationPolicy.RefreshOnSuccessfulApplication)
            {
                activeEffect.ResetDuration();
            }

            if (spec.Effect.StackPeriodResetPolicy ==
                GameplayEffectStackingPeriodPolicy.ResetOnSuccessfulApplication)
            {
                activeEffect.ResetPeriod();
            }

            if (spec.Effect.IsPeriodic)
            {
                if (spec.Effect.ExecutePeriodicEffectOnApplication)
                    ApplyInstantModifiers(activeEffect.Spec, activeEffect.StackCount);
            }
            else if (activeEffect.StackCount != previousStackCount)
            {
                RecalculateModifiedAttributes(spec.Effect.Modifiers);
            }

            return true;
        }

        private bool TryFindStackingActiveEffect(
            GameplayEffect effect,
            GameplayEffectContext context,
            out ActiveGameplayEffect activeEffect)
        {
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                activeEffect = _activeEffects[i];
                if (!ReferenceEquals(activeEffect.Spec.Effect, effect))
                    continue;

                if (effect.StackingType == GameplayEffectStackingType.AggregateByTarget)
                    return true;

                if (effect.StackingType == GameplayEffectStackingType.AggregateBySource &&
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
            GameplayEffect effect = activeEffect.Spec.Effect;
            if (activeEffect.StackCount > 1)
            {
                switch (effect.StackExpirationPolicy)
                {
                    case GameplayEffectStackingExpirationPolicy.RemoveSingleStackAndRefreshDuration:
                        activeEffect.RemoveStack();
                        activeEffect.ResetDuration();
                        if (!effect.IsPeriodic)
                            RecalculateModifiedAttributes(effect.Modifiers);
                        return;
                    case GameplayEffectStackingExpirationPolicy.RefreshDuration:
                        activeEffect.ResetDuration();
                        return;
                }
            }

            if (effect.StackExpirationPolicy == GameplayEffectStackingExpirationPolicy.RefreshDuration)
            {
                activeEffect.ResetDuration();
                return;
            }

            RemoveActiveGameplayEffect(activeEffect.Handle);
        }

        private void ApplyInstantModifiers(GameplayEffectSpec spec, int stackCount = 1)
        {
            for (int stackIndex = 0; stackIndex < stackCount; stackIndex++)
            {
                IReadOnlyList<GameplayModifier> modifiers = spec.Effect.Modifiers;
                for (int i = 0; i < modifiers.Count; i++)
                {
                    GameplayModifier modifier = modifiers[i];
                    if (!TryGetAttributeData(modifier.Attribute, out GameplayAttributeData data))
                        continue;

                    float magnitude = ResolveModifierMagnitude(modifier, spec);
                    float baseValue = data.BaseValue;
                    float nextValue = modifier.Operation switch
                    {
                        GameplayModifierOperation.Add => baseValue + magnitude,
                        GameplayModifierOperation.Multiply => baseValue * magnitude,
                        GameplayModifierOperation.Override => magnitude,
                        _ => baseValue
                    };

                    data.SetBaseValue(nextValue);
                    RecalculateAttribute(modifier.Attribute);
                }
            }

            ApplyExecutions(spec, stackCount);
        }

        private void ApplyExecutions(GameplayEffectSpec spec, int stackCount)
        {
            IReadOnlyList<GameplayEffectExecution> executions = spec.Effect.Executions;
            for (int i = 0; i < executions.Count; i++)
            {
                GameplayEffectExecutionParameters parameters = new(spec, stackCount);
                GameplayEffectExecutionOutput output = new();
                executions[i].Execute(parameters, output);
                ApplyEvaluatedModifiers(output.Modifiers);
            }
        }

        private void CaptureSnapshotAttributes(
            GameplayEffectSpec spec,
            GameplayEffectAttributeCaptureSource captureSource)
        {
            AbilitySystemComponent component = captureSource == GameplayEffectAttributeCaptureSource.Source
                ? spec.Context?.Source
                : spec.Context?.Target;

            if (component == null)
                return;

            List<GameplayEffectAttributeCaptureDefinition> definitions = new();
            IReadOnlyList<GameplayModifier> modifiers = spec.Effect.Modifiers;
            for (int i = 0; i < modifiers.Count; i++)
            {
                GameplayModifier modifier = modifiers[i];
                if (modifier.MagnitudeType == GameplayModifierMagnitudeType.AttributeBased &&
                    modifier.AttributeBasedMagnitude.IsValid)
                {
                    definitions.Add(modifier.AttributeBasedMagnitude.CaptureDefinition);
                }
            }

            IReadOnlyList<GameplayEffectExecution> executions = spec.Effect.Executions;
            for (int i = 0; i < executions.Count; i++)
            {
                executions[i].GetAttributeCaptureDefinitions(definitions);
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                GameplayEffectAttributeCaptureDefinition definition = definitions[i];
                if (definition.Source == captureSource && definition.Snapshot)
                    spec.CaptureAttribute(definition, component);
            }
        }

        private void ApplyEvaluatedModifiers(IReadOnlyList<GameplayModifierEvaluatedData> modifiers)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                GameplayModifierEvaluatedData modifier = modifiers[i];
                if (!TryGetAttributeData(modifier.Attribute, out GameplayAttributeData data))
                    continue;

                float baseValue = data.BaseValue;
                float nextValue = modifier.Operation switch
                {
                    GameplayModifierOperation.Add => baseValue + modifier.Magnitude,
                    GameplayModifierOperation.Multiply => baseValue * modifier.Magnitude,
                    GameplayModifierOperation.Override => modifier.Magnitude,
                    _ => baseValue
                };

                data.SetBaseValue(nextValue);
                RecalculateAttribute(modifier.Attribute);
            }
        }

        private void RecalculateModifiedAttributes(IReadOnlyList<GameplayModifier> modifiers)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                RecalculateAttribute(modifiers[i].Attribute);
            }
        }

        private void RecalculateAttribute(GameplayAttribute attribute)
        {
            if (!TryGetAttributeData(attribute, out GameplayAttributeData data))
                return;

            GameplayAttributeAggregator aggregator = new();
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                if (_activeEffects[i].Spec.Effect.IsPeriodic)
                    continue;

                IReadOnlyList<GameplayModifier> modifiers = _activeEffects[i].Spec.Effect.Modifiers;
                for (int j = 0; j < modifiers.Count; j++)
                {
                    GameplayModifier modifier = modifiers[j];
                    if (modifier.Attribute.Equals(attribute))
                    {
                        float magnitude = ResolveModifierMagnitude(modifier, _activeEffects[i].Spec);
                        aggregator.AddModifier(modifier, magnitude, _activeEffects[i].StackCount);
                    }
                }
            }

            data.SetCurrentValue(aggregator.Evaluate(data.BaseValue));
        }

        private static float ResolveModifierMagnitude(GameplayModifier modifier, GameplayEffectSpec spec)
        {
            switch (modifier.MagnitudeType)
            {
                case GameplayModifierMagnitudeType.SetByCaller:
                    return spec.GetSetByCallerMagnitude(modifier.SetByCallerTag);
                case GameplayModifierMagnitudeType.AttributeBased:
                    return spec.AttemptCalculateAttributeBasedMagnitude(
                        modifier.AttributeBasedMagnitude,
                        out float magnitude)
                        ? magnitude
                        : 0f;
                default:
                    return modifier.Magnitude;
            }
        }

        private void InvokeGameplayCues(
            GameplayEffectSpec spec,
            GameplayEffectContext context,
            GameplayCueEvent eventType)
        {
            IReadOnlyList<GameplayEffectCue> cues = spec.Effect.GameplayCues;
            for (int i = 0; i < cues.Count; i++)
            {
                GameplayEffectCue cue = cues[i];
                GameplayCueParameters parameters = new(
                    context,
                    cue.NormalizeLevel(spec.Level),
                    spec.Level,
                    spec.Level);

                foreach (GameplayTag tag in cue.GameplayCueTags)
                {
                    InvokeGameplayCueEvent(tag, eventType, parameters);
                }
            }
        }

        private void ApplyGrantedTags(GameplayTagContainer grantedTags)
        {
            foreach (GameplayTag tag in grantedTags)
            {
                OwnedTags.AddTag(tag);
            }
        }

        private void RemoveGrantedTags(GameplayTagContainer grantedTags)
        {
            foreach (GameplayTag tag in grantedTags)
            {
                OwnedTags.RemoveTag(tag);
            }
        }

        private void RegisterAbilityTriggers(GameplayAbilitySpec spec)
        {
            IReadOnlyList<GameplayAbilityTriggerData> triggers = spec.Ability.AbilityTriggers;
            for (int i = 0; i < triggers.Count; i++)
            {
                GameplayAbilityTriggerData trigger = triggers[i];
                if (trigger.TriggerSource != GameplayAbilityTriggerSource.GameplayEvent)
                    continue;

                if (!_gameplayEventTriggeredAbilities.TryGetValue(
                        trigger.TriggerTag,
                        out List<GameplayAbilitySpecHandle> handles))
                {
                    handles = new List<GameplayAbilitySpecHandle>();
                    _gameplayEventTriggeredAbilities.Add(trigger.TriggerTag, handles);
                }

                handles.Add(spec.Handle);
            }
        }

        private int TriggerAbilitiesFromGameplayEvent(GameplayEventData eventData)
        {
            int triggeredCount = 0;
            GameplayTag currentTag = eventData.EventTag;
            while (currentTag.IsValid)
            {
                if (_gameplayEventTriggeredAbilities.TryGetValue(
                        currentTag,
                        out List<GameplayAbilitySpecHandle> handles))
                {
                    GameplayAbilitySpecHandle[] snapshot = handles.ToArray();
                    for (int i = 0; i < snapshot.Length; i++)
                    {
                        if (TriggerAbilityFromGameplayEvent(snapshot[i], eventData))
                            triggeredCount++;
                    }
                }

                currentTag = GetDirectParentTag(currentTag);
            }

            return triggeredCount;
        }

        private static GameplayTag GetDirectParentTag(GameplayTag tag)
        {
            return tag.RequestDirectParent();
        }
    }
}
