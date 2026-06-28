using Domains.Intent.Data;

namespace Domains.Intent.Runtime
{
    public readonly struct ResolvedIntentActionData
    {
        public ResolvedIntentActionData(
            MonsterActionModel baseActionModel,
            MonsterActionModel finalActionModel,
            IntentOverrideRule appliedOverrideRule)
        {
            BaseActionModel = baseActionModel;
            FinalActionModel = finalActionModel;
            AppliedOverrideRule = appliedOverrideRule;
        }

        public MonsterActionModel BaseActionModel { get; }
        public MonsterActionModel FinalActionModel { get; }
        public IntentOverrideRule AppliedOverrideRule { get; }
        public bool HasOverride => AppliedOverrideRule != null;
    }
}
