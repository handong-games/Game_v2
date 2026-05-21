namespace Gameplay.GAS
{
    public abstract class GameplayAbilityTask
    {
        public GameplayAbility Ability { get; private set; }
        public GameplayAbilitySpecHandle AbilityHandle { get; private set; }
        public GameplayAbilityActorInfo ActorInfo { get; private set; }
        public GameplayAbilityActivationInfo ActivationInfo { get; private set; }
        public AbilitySystemComponent AbilitySystem => ActorInfo?.AbilitySystem;
        public GameplayAbilityTaskWaitState WaitState { get; private set; } =
            GameplayAbilityTaskWaitState.WaitingOnGame;
        public bool IsActive { get; private set; }
        public bool IsEnded { get; private set; }

        public virtual void Activate()
        {
        }

        public void EndTask()
        {
            if (IsEnded)
                return;

            Ability?.UnregisterTask(this);
            EndTaskInternal(false);
        }

        internal void Initialize(
            GameplayAbility ability,
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo)
        {
            Ability = ability;
            AbilityHandle = handle;
            ActorInfo = actorInfo;
            ActivationInfo = activationInfo;
            IsActive = true;
            IsEnded = false;
        }

        internal void EndFromAbility()
        {
            EndTaskInternal(true);
        }

        protected bool ShouldBroadcastAbilityTaskDelegates()
        {
            return IsActive && !IsEnded;
        }

        protected void SetWaitingOnGame()
        {
            WaitState = GameplayAbilityTaskWaitState.WaitingOnGame;
        }

        protected void SetWaitingOnUser()
        {
            WaitState = GameplayAbilityTaskWaitState.WaitingOnUser;
        }

        protected virtual void OnDestroy(bool abilityEnding)
        {
        }

        private void EndTaskInternal(bool abilityEnding)
        {
            if (IsEnded)
                return;

            IsEnded = true;
            IsActive = false;
            OnDestroy(abilityEnding);
            Ability = null;
            AbilityHandle = GameplayAbilitySpecHandle.Invalid;
            ActorInfo = null;
        }
    }
}
