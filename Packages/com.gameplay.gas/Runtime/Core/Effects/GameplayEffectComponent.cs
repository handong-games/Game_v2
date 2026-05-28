using UnityEngine;

namespace Gameplay.GAS
{
    public abstract class GameplayEffectComponent : ScriptableObject
    {
        public virtual bool CanGameplayEffectApply(
            GameplayEffectSpec spec,
            AbilitySystemComponent target)
        {
            return true;
        }

        public virtual void OnActiveGameplayEffectAdded(
            ActiveGameplayEffect activeEffect,
            AbilitySystemComponent target)
        {
        }

        public virtual void OnGameplayEffectExecuted(
            GameplayEffectSpec spec,
            GameplayEffectExecutionOutput output,
            AbilitySystemComponent target)
        {
        }

        public virtual void OnGameplayEffectApplied(
            GameplayEffectSpec spec,
            AbilitySystemComponent target)
        {
        }

        public virtual void OnGameplayEffectRemoved(
            ActiveGameplayEffect activeEffect,
            AbilitySystemComponent target)
        {
        }
    }
}
