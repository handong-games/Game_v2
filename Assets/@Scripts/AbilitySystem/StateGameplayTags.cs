using Gameplay.GAS;

namespace Game.AbilitySystem
{
    [GameplayTagProvider]
    public static class StateGameplayTags
    {
        public const string StunnedName = "State.Stunned";
        public const string SilencedName = "State.Silenced";

        public static readonly GameplayTag Stunned = GameplayTag.Define(StunnedName);
        public static readonly GameplayTag Silenced = GameplayTag.Define(SilencedName);
    }
}
