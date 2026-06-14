namespace Gameplay.GAS
{
    public sealed class GameplayAbilityActorInfo
    {
        public GameplayAbilityActorInfo(GameplayActor owner, AbilitySystemComponent abilitySystem)
        {
            Owner = owner;
            AbilitySystem = abilitySystem;
        }

        public GameplayActor Owner { get; }
        public AbilitySystemComponent AbilitySystem { get; }
        public object Avatar { get; internal set; }

        public T GetAvatar<T>() where T : class
        {
            return Avatar as T;
        }

        public void ClearAvatar()
        {
            Avatar = null;
        }
    }
}
