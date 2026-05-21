namespace Gameplay.GAS
{
    public sealed class GameplayAbilitySpec
    {
        public GameplayAbilitySpec(GameplayAbilitySpecHandle handle, GameplayAbility ability, int level)
        {
            Handle = handle;
            Ability = ability;
            Level = level;
        }

        public GameplayAbilitySpecHandle Handle { get; }
        public GameplayAbility Ability { get; }
        public int Level { get; }
        public GameplayTagContainer DynamicSourceTags { get; } = new();
    }
}
