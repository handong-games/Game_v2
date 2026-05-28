using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GAS
{
    public abstract class GameplayAbility : ScriptableObject
    {
        private readonly List<GameplayAbilityTask> _activeTasks = new();
        private readonly List<GameplayAbilityTriggerData> _abilityTriggers = new();

        [SerializeField]
        private GameplayEffect _costGameplayEffect;

        [SerializeField]
        private GameplayEffect _cooldownGameplayEffect;

        public GameplayTagContainer AbilityTags { get; } = new();
        public GameplayTagContainer ActivationOwnedTags { get; } = new();
        public GameplayEffect CostGameplayEffect
        {
            get => _costGameplayEffect;
            set => _costGameplayEffect = value;
        }

        public GameplayEffect CooldownGameplayEffect
        {
            get => _cooldownGameplayEffect;
            set => _cooldownGameplayEffect = value;
        }

        public IReadOnlyList<GameplayAbilityTask> ActiveTasks => _activeTasks;
        public IReadOnlyList<GameplayAbilityTriggerData> AbilityTriggers => _abilityTriggers;

        public virtual bool CanActivateAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo)
        {
            return true;
        }

        public virtual bool ShouldAbilityRespondToEvent(
            GameplayAbilityActorInfo actorInfo,
            GameplayEventData eventData)
        {
            return true;
        }

        public virtual bool CommitAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo)
        {
            if (!CommitCheck(handle, actorInfo, activationInfo))
                return false;

            CommitExecute(handle, actorInfo, activationInfo);
            return true;
        }

        public virtual bool CommitAbilityCost(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo)
        {
            if (!CheckCost(handle, actorInfo))
                return false;

            ApplyCost(handle, actorInfo, activationInfo);
            return true;
        }

        public virtual bool CommitAbilityCooldown(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            bool forceCooldown = false)
        {
            if (!forceCooldown && !CheckCooldown(handle, actorInfo))
                return false;

            ApplyCooldown(handle, actorInfo, activationInfo);
            return true;
        }

        public abstract void ActivateAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            GameplayEventData triggerEventData);

        public virtual void EndAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            bool wasCancelled)
        {
            EndActiveTasks();
        }

        protected virtual bool CommitCheck(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo)
        {
            return CheckCost(handle, actorInfo) &&
                   CheckCooldown(handle, actorInfo);
        }

        protected virtual void CommitExecute(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo)
        {
            ApplyCost(handle, actorInfo, activationInfo);
            ApplyCooldown(handle, actorInfo, activationInfo);
        }

        public virtual GameplayEffect GetCostGameplayEffect()
        {
            return _costGameplayEffect;
        }

        public virtual GameplayEffect GetCooldownGameplayEffect()
        {
            return _cooldownGameplayEffect;
        }

        public virtual bool CheckCost(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo)
        {
            GameplayEffect costGameplayEffect = GetCostGameplayEffect();
            return costGameplayEffect == null ||
                   actorInfo.AbilitySystem.CanApplyAttributeModifiers(
                       costGameplayEffect,
                       GetAbilityLevel(actorInfo, handle));
        }

        public virtual void ApplyCost(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo)
        {
            GameplayEffect costGameplayEffect = GetCostGameplayEffect();
            if (costGameplayEffect == null)
                return;

            actorInfo.AbilitySystem.ApplyGameplayEffectToSelf(
                costGameplayEffect,
                GetAbilityLevel(actorInfo, handle));
        }

        public virtual bool CheckCooldown(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo)
        {
            GameplayEffect cooldownGameplayEffect = GetCooldownGameplayEffect();
            return cooldownGameplayEffect == null ||
                   !actorInfo.AbilitySystem.HasAnyMatchingGameplayTags(cooldownGameplayEffect.GrantedTags);
        }

        public virtual void ApplyCooldown(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo)
        {
            GameplayEffect cooldownGameplayEffect = GetCooldownGameplayEffect();
            if (cooldownGameplayEffect == null)
                return;

            actorInfo.AbilitySystem.ApplyGameplayEffectToSelf(
                cooldownGameplayEffect,
                GetAbilityLevel(actorInfo, handle));
        }

        protected GameplayEffectSpec MakeOutgoingGameplayEffectSpec(
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilitySpecHandle handle,
            GameplayEffect effect)
        {
            return actorInfo.AbilitySystem.MakeOutgoingSpec(effect, GetAbilityLevel(actorInfo, handle));
        }

        protected GameplayEffectSpec MakeOutgoingGameplayEffectSpec(
            GameplayAbilityActorInfo actorInfo,
            GameplayEffect effect,
            int level)
        {
            return actorInfo.AbilitySystem.MakeOutgoingSpec(effect, level);
        }

        protected ActiveGameplayEffect ApplyGameplayEffectSpecToOwner(
            GameplayAbilityActorInfo actorInfo,
            GameplayEffectSpec spec)
        {
            return actorInfo.AbilitySystem.ApplyGameplayEffectSpecToSelf(spec);
        }

        protected ActiveGameplayEffect ApplyGameplayEffectSpecToTarget(
            GameplayAbilityActorInfo actorInfo,
            GameplayEffectSpec spec,
            AbilitySystemComponent target)
        {
            return actorInfo.AbilitySystem.ApplyGameplayEffectSpecToTarget(spec, target);
        }

        protected ActiveGameplayEffect ApplyGameplayEffectToTarget(
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilitySpecHandle handle,
            GameplayEffect effect,
            AbilitySystemComponent target)
        {
            GameplayEffectSpec spec = MakeOutgoingGameplayEffectSpec(actorInfo, handle, effect);
            return ApplyGameplayEffectSpecToTarget(actorInfo, spec, target);
        }

        public void AddAbilityTrigger(GameplayAbilityTriggerData triggerData)
        {
            if (triggerData.IsValid)
                _abilityTriggers.Add(triggerData);
        }

        public void AddGameplayEventTrigger(GameplayTag eventTag)
        {
            AddAbilityTrigger(new GameplayAbilityTriggerData(
                GameplayAbilityTriggerSource.GameplayEvent,
                eventTag));
        }

        internal T NewTask<T>(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo) where T : GameplayAbilityTask, new()
        {
            T task = new();
            task.Initialize(this, handle, actorInfo, activationInfo);
            _activeTasks.Add(task);
            return task;
        }

        internal void UnregisterTask(GameplayAbilityTask task)
        {
            _activeTasks.Remove(task);
        }

        private void EndActiveTasks()
        {
            for (int i = _activeTasks.Count - 1; i >= 0; i--)
            {
                _activeTasks[i].EndFromAbility();
            }

            _activeTasks.Clear();
        }

        protected int GetAbilityLevel(
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilitySpecHandle handle)
        {
            return actorInfo.AbilitySystem.TryGetAbilitySpec(handle, out GameplayAbilitySpec spec)
                ? spec.Level
                : 1;
        }
    }
}
