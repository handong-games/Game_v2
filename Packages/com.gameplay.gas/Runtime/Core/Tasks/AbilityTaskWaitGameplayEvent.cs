using System;

namespace Gameplay.GAS
{
    public sealed class AbilityTaskWaitGameplayEvent : GameplayAbilityTask
    {
        public event Action<GameplayEventData> EventReceived;

        public GameplayTag EventTag { get; private set; }
        public bool OnlyTriggerOnce { get; private set; }
        public bool OnlyMatchExact { get; private set; }

        public static AbilityTaskWaitGameplayEvent WaitGameplayEvent(
            GameplayAbility ability,
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            GameplayTag eventTag,
            bool onlyTriggerOnce = false,
            bool onlyMatchExact = true)
        {
            AbilityTaskWaitGameplayEvent task =
                ability.NewTask<AbilityTaskWaitGameplayEvent>(handle, actorInfo, activationInfo);
            task.EventTag = eventTag;
            task.OnlyTriggerOnce = onlyTriggerOnce;
            task.OnlyMatchExact = onlyMatchExact;
            return task;
        }

        public override void Activate()
        {
            if (AbilitySystem == null)
            {
                EndTask();
                return;
            }

            AbilitySystem.GameplayEventReceived += OnGameplayEventReceived;
        }

        protected override void OnDestroy(bool abilityEnding)
        {
            if (AbilitySystem != null)
                AbilitySystem.GameplayEventReceived -= OnGameplayEventReceived;

            EventReceived = null;
        }

        private void OnGameplayEventReceived(GameplayEventData eventData)
        {
            if (eventData == null || !Matches(eventData.EventTag))
                return;

            if (ShouldBroadcastAbilityTaskDelegates())
                EventReceived?.Invoke(eventData);

            if (OnlyTriggerOnce)
                EndTask();
        }

        private bool Matches(GameplayTag eventTag)
        {
            return OnlyMatchExact
                ? eventTag.MatchesTagExact(EventTag)
                : eventTag.MatchesTag(EventTag);
        }
    }
}
