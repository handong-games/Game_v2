using Domains.Combat.Intent.Data;

namespace Domains.Combat.Intent
{
    public readonly struct ResolvedIntentActionData
    {
        public ResolvedIntentActionData(
            IntentActionModel baseActionModel,
            IntentActionModel finalActionModel,
            IntentOverrideRule appliedOverrideRule)
        {
            BaseActionModel = baseActionModel;
            FinalActionModel = finalActionModel;
            AppliedOverrideRule = appliedOverrideRule;
        }

        public IntentActionModel BaseActionModel { get; }
        public IntentActionModel FinalActionModel { get; }
        public IntentOverrideRule AppliedOverrideRule { get; }
        public bool HasOverride => AppliedOverrideRule != null;
    }
}
