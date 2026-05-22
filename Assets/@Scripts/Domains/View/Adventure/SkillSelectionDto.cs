namespace Domains.Adventure
{
    public readonly struct SkillSelectionDto
    {
        public SkillSelectionDto(bool canUse, bool requiresTarget)
        {
            CanUse = canUse;
            RequiresTarget = requiresTarget;
        }

        public bool CanUse { get; }
        public bool RequiresTarget { get; }
    }
}
