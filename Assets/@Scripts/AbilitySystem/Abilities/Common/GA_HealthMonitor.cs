using Game.AbilitySystem.Attributes;
using Game.Messages;
using Gameplay.GAS;

namespace Game.AbilitySystem.Abilities
{
    public sealed class GA_HealthMonitor : GameplayAbility
    {
        private VitalAttributeSet _vitalSet;
        private bool _deathRaised;

        public GA_HealthMonitor()
        {
            AbilityTags.AddTag(AbilityGameplayTags.AbilityCombatHealthMonitor);
        }

        public override void OnGiveAbility(
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilitySpec spec)
        {
            base.OnGiveAbility(actorInfo, spec);

            _vitalSet = actorInfo?.AbilitySystem.GetSet<VitalAttributeSet>();
            if (_vitalSet == null)
                return;

            _vitalSet.Health.CurrentValueChanged += OnHealthChanged;
        }

        public override void OnRemoveAbility(
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilitySpec spec)
        {
            if (_vitalSet != null)
                _vitalSet.Health.CurrentValueChanged -= OnHealthChanged;

            _vitalSet = null;
            _deathRaised = false;
            base.OnRemoveAbility(actorInfo, spec);
        }

        public override void ActivateAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            GameplayEventData triggerEventData)
        {
        }

        private void OnHealthChanged(float oldValue, float newValue)
        {
            if (!_deathRaised && oldValue > 0f && newValue <= 0f)
            {
                _deathRaised = true;
                PublishInternalDeath();
                PublishExternalDeath();
                return;
            }

            if (_deathRaised && newValue > 0f)
                _deathRaised = false;
        }

        private void PublishInternalDeath()
        {
            GameplayAbilityActorInfo actorInfo = CurrentActorInfo;
            if (actorInfo?.AbilitySystem == null)
                return;

            actorInfo.AbilitySystem.HandleGameplayEvent(
                new GameplayEventData(AbilityGameplayTags.EventCombatDeath)
                {
                    Target = actorInfo.AbilitySystem
                });
        }

        private void PublishExternalDeath()
        {
            GameplayAbilityActorInfo actorInfo = CurrentActorInfo;
            if (actorInfo == null)
                return;

            object avatar = actorInfo.Avatar ?? actorInfo.Owner;
            GameplayMessageManager.Instance.Publish(
                GameplayMessageTags.CombatDeath,
                new GameplayDeathMessage(avatar));
        }
    }
}
