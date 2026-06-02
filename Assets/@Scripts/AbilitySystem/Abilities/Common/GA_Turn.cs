using System;
using Domains.Card;
using Domains.Combat;
using Game.AbilitySystem.Tasks;
using Gameplay.GAS;
using UIToolkit.Timeline;
using UnityEngine;

namespace Game.AbilitySystem.Abilities
{
    [CreateAssetMenu(menuName = "Game/AbilitySystem/Abilities/Common/GA Turn")]
    public sealed class GA_Turn : GameplayAbility
    {
        [SerializeField]
        private TimelineAsset _attackTimeline;

        [SerializeField]
        private TimelineAsset _hitTimeline;

        [SerializeField]
        private GameplayEffect _impactEffect;

        [SerializeField]
        private GameplayTag _impactNotifyTag = AbilityGameplayTags.TimelineNotifyImpact;

        public GA_Turn()
        {
            AbilityTags.AddTag(AbilityGameplayTags.AbilityTurn);
            AddGameplayEventTrigger(AbilityGameplayTags.EventTurnStarted);
        }

        public override void ActivateAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            GameplayEventData triggerEventData)
        {
            CombatTurnActionContext turnContext = GetTurnActionContext(triggerEventData);
            if (!TryResolveParticipants(
                    actorInfo,
                    triggerEventData,
                    out Card attacker,
                    out Card target))
            {
                CompleteTurnAction(turnContext, handle, actorInfo, activationInfo);
                return;
            }

            if (_attackTimeline == null)
            {
                Debug.LogError($"{nameof(GA_Turn)} requires an attack timeline.");
                CompleteTurnAction(turnContext, handle, actorInfo, activationInfo);
                return;
            }

            if (_impactEffect == null)
            {
                Debug.LogError($"{nameof(GA_Turn)} requires an impact gameplay effect.");
                CompleteTurnAction(turnContext, handle, actorInfo, activationInfo);
                return;
            }

            if (!_impactNotifyTag.IsValid)
            {
                Debug.LogError($"{nameof(GA_Turn)} requires an impact notify tag.");
                CompleteTurnAction(turnContext, handle, actorInfo, activationInfo);
                return;
            }

            PlayTurnSequence(
                handle,
                actorInfo,
                activationInfo,
                attacker,
                target,
                turnContext);
        }

        private void PlayTurnSequence(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            Card attacker,
            Card target,
            CombatTurnActionContext turnContext)
        {
            bool attackDone = false;
            bool hitStarted = false;
            bool hitDone = false;
            bool impactHandled = false;
            bool turnCompleted = false;

            AbilityTask_PlayTimeline attackTask =
                AbilityTask_PlayTimeline.PlayTimeline(
                    this,
                    handle,
                    actorInfo,
                    activationInfo,
                    attacker.Timeline,
                    _attackTimeline);

            attackTask.NotifyRaised += notify =>
            {
                if (impactHandled || !IsImpactNotify(notify))
                    return;

                impactHandled = true;
                ApplyGameplayEffectToTarget(
                    actorInfo,
                    handle,
                    _impactEffect,
                    target.AbilitySystem);

                hitStarted = TryPlayHitTimeline(
                    handle,
                    actorInfo,
                    activationInfo,
                    target,
                    () =>
                    {
                        hitDone = true;
                        TryComplete();
                    });

                if (!hitStarted)
                    hitDone = true;

                TryComplete();
            };

            attackTask.Completed += () =>
            {
                attackDone = true;
                if (!impactHandled)
                    Debug.LogError($"{nameof(GA_Turn)} attack timeline finished without an impact notify.");

                TryComplete();
            };

            attackTask.Cancelled += () =>
            {
                attackDone = true;
                TryComplete();
            };

            attackTask.Activate();

            void TryComplete()
            {
                if (turnCompleted)
                    return;

                if (!attackDone)
                    return;

                if (hitStarted && !hitDone)
                    return;

                turnCompleted = true;
                CompleteTurnAction(turnContext, handle, actorInfo, activationInfo);
            }
        }

        private bool TryPlayHitTimeline(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            Card target,
            Action onFinished)
        {
            if (_hitTimeline == null)
                return false;

            if (!target.Timeline.CanPlay(_hitTimeline))
                return false;

            AbilityTask_PlayTimeline hitTask =
                AbilityTask_PlayTimeline.PlayTimeline(
                    this,
                    handle,
                    actorInfo,
                    activationInfo,
                    target.Timeline,
                    _hitTimeline);

            hitTask.Completed += onFinished;
            hitTask.Cancelled += onFinished;
            hitTask.Activate();
            return true;
        }

        private bool IsImpactNotify(TimelineNotify notify)
        {
            return notify.Definition is GameplayTagTimelineNotifyDefinition definition &&
                   definition.Tag.MatchesTagExact(_impactNotifyTag);
        }

        private static CombatTurnActionContext GetTurnActionContext(
            GameplayEventData triggerEventData)
        {
            return triggerEventData != null &&
                   triggerEventData.TryGetOptionalObject(out CombatTurnActionContext context)
                ? context
                : null;
        }

        private static bool TryResolveParticipants(
            GameplayAbilityActorInfo actorInfo,
            GameplayEventData triggerEventData,
            out Card attacker,
            out Card target)
        {
            attacker = actorInfo?.Owner as Card;
            target = triggerEventData?.Target?.Owner as Card;

            if (attacker == null)
                Debug.LogError($"{nameof(GA_Turn)} attacker card is missing.");

            if (target == null)
                Debug.LogError($"{nameof(GA_Turn)} target card is missing.");

            return attacker != null && target != null;
        }

        private void CompleteTurnAction(
            CombatTurnActionContext turnContext,
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo)
        {
            turnContext?.Complete();

            EndAbility(
                handle,
                actorInfo,
                activationInfo,
                wasCancelled: false);
        }
    }
}
