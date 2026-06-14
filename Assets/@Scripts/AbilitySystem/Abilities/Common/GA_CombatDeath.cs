using Domains.Card;
using Game.AbilitySystem.Tasks;
using Gameplay.GAS;
using UIToolkit.Timeline;
using UnityEngine;

namespace Game.AbilitySystem.Abilities
{
    [CreateAssetMenu(menuName = "Game/AbilitySystem/Abilities/Common/GA Combat Death")]
    public sealed class GA_CombatDeath : GameplayAbility
    {
        [SerializeField]
        private TimelineAsset _deathTimeline;

        private bool _deathStarted;

        public GA_CombatDeath()
        {
            AbilityTags.AddTag(AbilityGameplayTags.AbilityCombatDeath);
            ActivationOwnedTags.AddTag(StateGameplayTags.Dead);
            AddGameplayEventTrigger(AbilityGameplayTags.EventCombatDeath);
        }

        public override void ActivateAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            GameplayEventData triggerEventData)
        {
            if (_deathStarted)
                return;

            _deathStarted = true;

            AbilitySystemComponent abilitySystem = actorInfo?.AbilitySystem;
            if (abilitySystem?.Owner is not Card card)
            {
                Debug.LogError($"{nameof(GA_CombatDeath)} requires a card owner.");
                return;
            }

            abilitySystem.CancelAbilities(ignore: this);
            SetCanBeCanceled(false);

            if (!card.Timeline.TrySetPointerInputEnabled(false))
            {
                Debug.LogError($"{nameof(GA_CombatDeath)} failed to disable pointer input.");
            }

            if (_deathTimeline == null)
            {
                Debug.LogError($"{nameof(GA_CombatDeath)} has no death timeline.");
                return;
            }

            if (!card.Timeline.CanPlay(_deathTimeline))
            {
                Debug.LogError($"{nameof(GA_CombatDeath)} cannot play death timeline.");
                return;
            }

            AbilityTask_PlayTimeline deathTask =
                AbilityTask_PlayTimeline.PlayTimeline(
                    this,
                    handle,
                    actorInfo,
                    activationInfo,
                    card.Timeline,
                    _deathTimeline,
                    cancelWhenAbilityEnds: false);

            deathTask.Activate();
        }

        public override void OnRemoveAbility(
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilitySpec spec)
        {
            _deathStarted = false;
            base.OnRemoveAbility(actorInfo, spec);
        }
    }
}
