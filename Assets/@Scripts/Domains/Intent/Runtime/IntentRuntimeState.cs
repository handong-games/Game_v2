using System;
using System.Collections.Generic;

namespace Domains.Intent.Runtime
{
    // Role:
    // Holds one monster's intent sequence position and latest resolved/display cache.
    public sealed class IntentRuntimeState
    {
        private static readonly IReadOnlyList<IntentDisplayData> EmptyDisplays =
            Array.Empty<IntentDisplayData>();

        private ResolvedIntentActionData _cachedResolvedAction;
        private IReadOnlyList<IntentDisplayData> _cachedIntentDisplays = EmptyDisplays;

        public int CurrentIntentActionIndex { get; private set; }
        public IReadOnlyList<IntentDisplayData> CachedIntentDisplays => _cachedIntentDisplays;
        public bool HasCachedResolvedAction { get; private set; }

        public bool TryGetCachedResolvedAction(out ResolvedIntentActionData resolvedAction)
        {
            resolvedAction = _cachedResolvedAction;
            return HasCachedResolvedAction;
        }

        public void SetCache(
            ResolvedIntentActionData resolvedAction,
            IReadOnlyList<IntentDisplayData> intentDisplays)
        {
            _cachedResolvedAction = resolvedAction;
            _cachedIntentDisplays = intentDisplays ?? EmptyDisplays;
            HasCachedResolvedAction = true;
        }

        public bool Consume(int actionCount)
        {
            if (!HasCachedResolvedAction)
                return false;

            bool shouldAdvance =
                !_cachedResolvedAction.HasOverride ||
                _cachedResolvedAction.AppliedOverrideRule.Advance;

            if (shouldAdvance && actionCount > 0)
                CurrentIntentActionIndex = (CurrentIntentActionIndex + 1) % actionCount;

            ClearCache();
            return true;
        }

        public void ClearCache()
        {
            _cachedResolvedAction = default;
            _cachedIntentDisplays = EmptyDisplays;
            HasCachedResolvedAction = false;
        }
    }
}
