namespace Gameplay.GAS
{
    public sealed class GameplayTagRequirements
    {
        public GameplayTagContainer RequiredTags { get; } = new();
        public GameplayTagContainer BlockedTags { get; } = new();

        public bool IsEmpty => RequiredTags.Count == 0 && BlockedTags.Count == 0;

        public bool RequirementsMet(GameplayTagCountContainer ownedTags)
        {
            return ownedTags.HasAll(RequiredTags) && !ownedTags.HasAny(BlockedTags);
        }

        public bool RequirementsMet(GameplayTagContainer ownedTags)
        {
            return ownedTags.HasAll(RequiredTags) && !ownedTags.HasAny(BlockedTags);
        }
    }
}
