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
    }
}
