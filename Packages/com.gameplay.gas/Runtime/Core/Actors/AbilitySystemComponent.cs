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
        private readonly ActiveGameplayEffectsContainer _activeGameplayEffects;
        private int _nextAbilityHandle = 1;

        public AbilitySystemComponent(GameplayActor owner)
        {
            Owner = owner;
            ActorInfo = new GameplayAbilityActorInfo(owner, this);
            _activeGameplayEffects = new ActiveGameplayEffectsContainer(this);
        }

        public GameplayActor Owner { get; }
        public GameplayAbilityActorInfo ActorInfo { get; }
        public GameplayTagCountContainer OwnedTags { get; } = new();
        public IReadOnlyList<ActiveGameplayEffect> ActiveEffects => _activeGameplayEffects.ActiveEffects;
        public event Action<GameplayEventData> GameplayEventReceived;
        public event Action<GameplayCueEventData> GameplayCueReceived;

        public GameplayAbilitySpecHandle GiveAbility(GameplayAbility ability, int level = 1)
        {
            if (ability == null)
                throw new ArgumentNullException(nameof(ability));

            GameplayAbility runtimeAbility = UnityEngine.Object.Instantiate(ability);
            runtimeAbility.name = ability.name;
            runtimeAbility.InitializeRuntimeInstanceFrom(ability);

            GameplayAbilitySpecHandle handle = new(_nextAbilityHandle++);
            GameplayAbilitySpec spec = new(handle, runtimeAbility, level);
            _abilities.Add(handle, spec);
            RegisterAbilityTriggers(spec);
            runtimeAbility.OnGiveAbility(ActorInfo, spec);
            return handle;
        }

        public bool ClearAbility(GameplayAbilitySpecHandle handle)
        {
            if (!_abilities.TryGetValue(handle, out GameplayAbilitySpec spec))
                return false;

            UnregisterAbilityTriggers(spec);
            spec.Ability.OnRemoveAbility(ActorInfo, spec);
            _abilities.Remove(handle);
            DestroyAbilityInstance(spec.Ability);
            return true;
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
                    ? ability.AbilityTags.HasAll(tags)
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

            GameplayCueEventData eventData = new(this, cueTag, eventType, parameters);
            GameplayCueManager.Instance.HandleGameplayCue(eventData);
            GameplayCueReceived?.Invoke(eventData);
        }

        public void AddAttributeSet(AttributeSet attributeSet)
        {
            if (attributeSet == null)
                return;

            attributeSet.Initialize(this);
            _attributeSets.Add(attributeSet);
        }

        public T GetSet<T>() where T : AttributeSet
        {
            for (int i = 0; i < _attributeSets.Count; i++)
            {
                if (_attributeSets[i] is T set)
                    return set;
            }

            return null;
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
            spec.CaptureDataFromSource();
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

        public ActiveGameplayEffect ApplyGameplayEffectToSelf(
            GameplayEffect effect,
            int level = 1)
        {
            GameplayEffectSpec spec = MakeOutgoingSpec(effect, level);
            return ApplyGameplayEffectSpecToSelf(spec);
        }

        public ActiveGameplayEffect ApplyGameplayEffectToTarget(
            GameplayEffect effect,
            AbilitySystemComponent target,
            int level = 1)
        {
            GameplayEffectSpec spec = MakeOutgoingSpec(effect, level);
            return ApplyGameplayEffectSpecToTarget(spec, target);
        }

        public ActiveGameplayEffect ApplyGameplayEffectSpecToSelf(GameplayEffectSpec spec)
        {
            return _activeGameplayEffects.ApplyGameplayEffectSpec(spec);
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

            spec.Ability.ActivateAbility(handle, ActorInfo, activationInfo, eventData);
            return true;
        }

        public bool CanApplyGameplayEffect(GameplayEffect effect)
        {
            if (effect == null)
                return true;

            GameplayEffectSpec spec = new(effect, new GameplayEffectContext(this, this));
            return effect.CanApply(spec, this);
        }

        public bool HasAnyMatchingGameplayTags(GameplayTagContainer tags)
        {
            return OwnedTags.HasAny(tags);
        }

        public bool CanApplyAttributeModifiers(GameplayEffect effect, int level = 1)
        {
            if (effect == null)
                return true;

            GameplayEffectSpec spec = MakeOutgoingSpec(effect, level)
                .WithContext(new GameplayEffectContext(this, this));
            spec.CaptureDataFromTarget();
            spec.CalculateModifierMagnitudes();

            if (!effect.CanApply(spec, this))
                return false;

            IReadOnlyList<GameplayModifierSpec> modifiers = spec.Modifiers;
            for (int i = 0; i < modifiers.Count; i++)
            {
                GameplayModifierSpec modifierSpec = modifiers[i];
                GameplayModifier modifier = modifierSpec.Modifier;
                if (!TryGetAttributeData(modifier.Attribute, out GameplayAttributeData data))
                    return false;

                float nextValue = modifier.Operation switch
                {
                    GameplayModifierOperation.Add => data.CurrentValue + modifierSpec.EvaluatedMagnitude,
                    GameplayModifierOperation.Multiply => data.CurrentValue * modifierSpec.EvaluatedMagnitude,
                    GameplayModifierOperation.Override => modifierSpec.EvaluatedMagnitude,
                    _ => data.CurrentValue
                };

                if (nextValue < 0f)
                    return false;
            }

            return true;
        }

        public bool RemoveActiveGameplayEffect(ActiveGameplayEffectHandle handle)
        {
            return _activeGameplayEffects.RemoveActiveGameplayEffect(handle);
        }

        public void TickActiveGameplayEffects(float deltaSeconds)
        {
            _activeGameplayEffects.Tick(deltaSeconds);
        }

        internal GameplayEffectContext CreateSelfApplicationContext(GameplayEffectSpec spec)
        {
            if (spec.Context == null)
                return new GameplayEffectContext(this, this);

            if (spec.Context.Source != null && spec.Context.Target != null)
                return spec.Context;

            return new GameplayEffectContext(
                spec.Context.Source ?? this,
                spec.Context.Target ?? this);
        }

        internal void InvokeGameplayCues(
            GameplayEffectSpec spec,
            GameplayEffectContext context,
            GameplayCueEvent eventType)
        {
            GameplayEffectContext cueContext = BuildCueContext(spec, context);
            IReadOnlyList<GameplayEffectCue> cues = spec.Definition.GameplayCues;
            for (int i = 0; i < cues.Count; i++)
            {
                GameplayEffectCue cue = cues[i];
                GameplayCueParameters parameters = new(
                    cueContext,
                    cue.NormalizeLevel(spec.Level),
                    spec.Level,
                    spec.Level,
                    spec.Level);

                foreach (GameplayTag tag in cue.GameplayCueTags)
                {
                    InvokeGameplayCueEvent(tag, eventType, parameters);
                }
            }
        }

        internal void ApplyGrantedTags(GameplayTagContainer grantedTags)
        {
            foreach (GameplayTag tag in grantedTags)
            {
                OwnedTags.AddTag(tag);
            }
        }

        internal void RemoveGrantedTags(GameplayTagContainer grantedTags)
        {
            foreach (GameplayTag tag in grantedTags)
            {
                OwnedTags.RemoveTag(tag);
            }
        }

        internal void ApplyModToAttribute(
            GameplayAttribute attribute,
            GameplayModifierOperation modifierOperation,
            float modifierMagnitude)
        {
            if (!TryGetAttributeStorage(attribute, out _, out GameplayAttributeData data))
                return;

            float currentBaseValue = data.BaseValue;
            float newBaseValue = modifierOperation switch
            {
                GameplayModifierOperation.Add => currentBaseValue + modifierMagnitude,
                GameplayModifierOperation.Multiply => currentBaseValue * modifierMagnitude,
                GameplayModifierOperation.Override => modifierMagnitude,
                _ => currentBaseValue
            };

            SetAttributeBaseValue(attribute, newBaseValue);
        }

        internal void InternalUpdateNumericalAttribute(
            GameplayAttribute attribute,
            float newCurrentValue)
        {
            if (!TryGetAttributeStorage(attribute, out AttributeSet attributeSet, out GameplayAttributeData data))
                return;

            float oldCurrentValue = data.CurrentValue;
            attributeSet.PreAttributeChange(attribute, ref newCurrentValue);
            data.SetCurrentValue(newCurrentValue);
            attributeSet.PostAttributeChange(attribute, oldCurrentValue, data.CurrentValue);
        }

        internal bool TryGetAttributeStorage(
            GameplayAttribute attribute,
            out AttributeSet attributeSet,
            out GameplayAttributeData data)
        {
            for (int i = 0; i < _attributeSets.Count; i++)
            {
                AttributeSet currentSet = _attributeSets[i];
                if (!currentSet.TryGetAttributeData(attribute, out data))
                    continue;

                attributeSet = currentSet;
                return true;
            }

            attributeSet = null;
            data = null;
            return false;
        }

        private void SetAttributeBaseValue(
            GameplayAttribute attribute,
            float newBaseValue)
        {
            if (!TryGetAttributeStorage(attribute, out AttributeSet attributeSet, out GameplayAttributeData data))
                return;

            float oldBaseValue = data.BaseValue;
            attributeSet.PreAttributeBaseChange(attribute, ref newBaseValue);
            data.SetBaseValue(newBaseValue);
            _activeGameplayEffects.RecalculateAttribute(attribute);
            attributeSet.PostAttributeBaseChange(attribute, oldBaseValue, data.BaseValue);
        }

        private GameplayEffectContext BuildCueContext(
            GameplayEffectSpec spec,
            GameplayEffectContext context)
        {
            GameplayEffectContext baseContext = context ?? new GameplayEffectContext(this, this);
            if (spec.ModifiedAttributes.Count == 0)
                return baseContext;

            return new GameplayEffectContext(
                baseContext.Source ?? this,
                baseContext.Target ?? this,
                spec.ModifiedAttributes);
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

        private void UnregisterAbilityTriggers(GameplayAbilitySpec spec)
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
                    continue;

                handles.Remove(spec.Handle);
                if (handles.Count == 0)
                    _gameplayEventTriggeredAbilities.Remove(trigger.TriggerTag);
            }
        }

        private static void DestroyAbilityInstance(GameplayAbility ability)
        {
            if (ability == null)
                return;

            if (UnityEngine.Application.isPlaying)
                UnityEngine.Object.Destroy(ability);
            else
                UnityEngine.Object.DestroyImmediate(ability);
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
            return tag.GetDirectParent();
        }
    }
}
