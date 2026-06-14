using Gameplay.GAS;

namespace Game.Messages
{
    [GameplayTagProvider]
    public static class GameplayMessageTags
    {
        public const string Combat_Death = "Message.Combat.Death";

        public static readonly GameplayTag CombatDeath = GameplayTag.Define(Combat_Death);
    }
}
