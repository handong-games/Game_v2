using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GAS
{
    [CreateAssetMenu(fileName = "GameplayEffect", menuName = "Gameplay/GAS/Gameplay Effect")]
    public class GameplayEffect : ScriptableObject
    {
        [SerializeField]
        private List<GameplayModifierDefinition> _modifiers = new();

        [SerializeField]
        private List<GameplayEffectExecutionDefinition> _executionDefinitions = new();

        [SerializeField]
        private List<GameplayEffectComponent> _components = new();

        [SerializeField]
        private List<GameplayEffectCueDefinition> _gameplayCues = new();

        [SerializeField]
        private GameplayEffectDurationPolicy _durationPolicy = GameplayEffectDurationPolicy.Instant;

        [SerializeField]
        private float _durationSeconds;

        [SerializeField]
        private float _periodSeconds;

        [SerializeField]
        private bool _executePeriodicEffectOnApplication;

        [SerializeField]
        private GameplayEffectStackingType _stackingType = GameplayEffectStackingType.None;

        [SerializeField]
        private int _stackLimitCount;

        [SerializeField]
        private GameplayEffectStackingDurationPolicy _stackDurationRefreshPolicy =
            GameplayEffectStackingDurationPolicy.RefreshOnSuccessfulApplication;

        [SerializeField]
        private GameplayEffectStackingPeriodPolicy _stackPeriodResetPolicy =
            GameplayEffectStackingPeriodPolicy.ResetOnSuccessfulApplication;

        [SerializeField]
        private GameplayEffectStackingExpirationPolicy _stackExpirationPolicy =
            GameplayEffectStackingExpirationPolicy.ClearEntireStack;

        private readonly List<GameplayModifier> _runtimeModifiers = new();
        private readonly List<GameplayModifier> _resolvedModifiers = new();
        private readonly List<GameplayEffectExecutionDefinition> _runtimeExecutionDefinitions = new();
        private readonly List<GameplayEffectExecutionDefinition> _resolvedExecutionDefinitions = new();
        private readonly List<GameplayEffectComponent> _runtimeComponents = new();
        private readonly List<GameplayEffectComponent> _resolvedComponents = new();
        private readonly List<GameplayEffectCue> _runtimeGameplayCues = new();
        private readonly List<GameplayEffectCue> _resolvedGameplayCues = new();

        public IReadOnlyList<GameplayModifier> Modifiers
        {
            get
            {
                RebuildModifiers();
                return _resolvedModifiers;
            }
        }

        public IReadOnlyList<GameplayEffectExecutionDefinition> ExecutionDefinitions
        {
            get
            {
                RebuildExecutionDefinitions();
                return _resolvedExecutionDefinitions;
            }
        }

        public IReadOnlyList<GameplayEffectComponent> Components
        {
            get
            {
                RebuildComponents();
                return _resolvedComponents;
            }
        }

        public IReadOnlyList<GameplayEffectCue> GameplayCues
        {
            get
            {
                RebuildGameplayCues();
                return _resolvedGameplayCues;
            }
        }

        public GameplayTagContainer GrantedTags { get; } = new();
        public GameplayTagRequirements ApplicationTagRequirements { get; } = new();

        public GameplayEffectDurationPolicy DurationPolicy
        {
            get => _durationPolicy;
            set => _durationPolicy = value;
        }

        public float DurationSeconds
        {
            get => _durationSeconds;
            set => _durationSeconds = value;
        }

        public float PeriodSeconds
        {
            get => _periodSeconds;
            set => _periodSeconds = value;
        }

        public bool ExecutePeriodicEffectOnApplication
        {
            get => _executePeriodicEffectOnApplication;
            set => _executePeriodicEffectOnApplication = value;
        }

        public GameplayEffectStackingType StackingType
        {
            get => _stackingType;
            set => _stackingType = value;
        }

        public int StackLimitCount
        {
            get => _stackLimitCount;
            set => _stackLimitCount = value;
        }

        public GameplayEffectStackingDurationPolicy StackDurationRefreshPolicy
        {
            get => _stackDurationRefreshPolicy;
            set => _stackDurationRefreshPolicy = value;
        }

        public GameplayEffectStackingPeriodPolicy StackPeriodResetPolicy
        {
            get => _stackPeriodResetPolicy;
            set => _stackPeriodResetPolicy = value;
        }

        public GameplayEffectStackingExpirationPolicy StackExpirationPolicy
        {
            get => _stackExpirationPolicy;
            set => _stackExpirationPolicy = value;
        }

        public bool IsPeriodic => PeriodSeconds > 0f;

        public static GameplayEffect Create()
        {
            return CreateInstance<GameplayEffect>();
        }

        public void AddModifier(GameplayModifier modifier)
        {
            if (modifier.Attribute.IsValid)
                _runtimeModifiers.Add(modifier);
        }

        public void AddExecution(GameplayEffectExecutionDefinition executionDefinition)
        {
            if (executionDefinition?.Calculation != null)
                _runtimeExecutionDefinitions.Add(executionDefinition);
        }

        public void AddComponent(GameplayEffectComponent component)
        {
            if (component != null)
                _runtimeComponents.Add(component);
        }

        public void AddGameplayCue(GameplayEffectCue cue)
        {
            if (cue != null)
                _runtimeGameplayCues.Add(cue);
        }

        public bool CanApply(GameplayEffectSpec spec, AbilitySystemComponent target)
        {
            if (target == null)
                return false;

            if (!ApplicationTagRequirements.RequirementsMet(target.OwnedTags))
                return false;

            IReadOnlyList<GameplayEffectComponent> components = Components;
            for (int i = 0; i < components.Count; i++)
            {
                GameplayEffectComponent component = components[i];
                if (component != null && !component.CanGameplayEffectApply(spec, target))
                    return false;
            }

            return true;
        }

        public void OnActiveGameplayEffectAdded(
            ActiveGameplayEffect activeEffect,
            AbilitySystemComponent target)
        {
            IReadOnlyList<GameplayEffectComponent> components = Components;
            for (int i = 0; i < components.Count; i++)
            {
                components[i]?.OnActiveGameplayEffectAdded(activeEffect, target);
            }
        }

        public void OnGameplayEffectExecuted(
            GameplayEffectSpec spec,
            GameplayEffectExecutionOutput output,
            AbilitySystemComponent target)
        {
            IReadOnlyList<GameplayEffectComponent> components = Components;
            for (int i = 0; i < components.Count; i++)
            {
                components[i]?.OnGameplayEffectExecuted(spec, output, target);
            }
        }

        public void OnGameplayEffectApplied(
            GameplayEffectSpec spec,
            AbilitySystemComponent target)
        {
            IReadOnlyList<GameplayEffectComponent> components = Components;
            for (int i = 0; i < components.Count; i++)
            {
                components[i]?.OnGameplayEffectApplied(spec, target);
            }
        }

        public void OnGameplayEffectRemoved(
            ActiveGameplayEffect activeEffect,
            AbilitySystemComponent target)
        {
            IReadOnlyList<GameplayEffectComponent> components = Components;
            for (int i = 0; i < components.Count; i++)
            {
                components[i]?.OnGameplayEffectRemoved(activeEffect, target);
            }
        }

        private void RebuildModifiers()
        {
            _resolvedModifiers.Clear();

            for (int i = 0; i < _modifiers.Count; i++)
            {
                GameplayModifierDefinition definition = _modifiers[i];
                if (definition != null && definition.TryBuild(out GameplayModifier modifier))
                    _resolvedModifiers.Add(modifier);
            }

            _resolvedModifiers.AddRange(_runtimeModifiers);
        }

        private void RebuildExecutionDefinitions()
        {
            _resolvedExecutionDefinitions.Clear();

            for (int i = 0; i < _executionDefinitions.Count; i++)
            {
                GameplayEffectExecutionDefinition executionDefinition = _executionDefinitions[i];
                if (executionDefinition?.Calculation != null)
                    _resolvedExecutionDefinitions.Add(executionDefinition);
            }

            _resolvedExecutionDefinitions.AddRange(_runtimeExecutionDefinitions);
        }

        private void RebuildComponents()
        {
            _resolvedComponents.Clear();

            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] != null)
                    _resolvedComponents.Add(_components[i]);
            }

            _resolvedComponents.AddRange(_runtimeComponents);
        }

        private void RebuildGameplayCues()
        {
            _resolvedGameplayCues.Clear();

            for (int i = 0; i < _gameplayCues.Count; i++)
            {
                GameplayEffectCueDefinition definition = _gameplayCues[i];
                if (definition != null && definition.TryBuild(out GameplayEffectCue cue))
                    _resolvedGameplayCues.Add(cue);
            }

            _resolvedGameplayCues.AddRange(_runtimeGameplayCues);
        }
    }
}
