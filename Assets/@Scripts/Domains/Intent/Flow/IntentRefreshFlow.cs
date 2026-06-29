using System;

namespace Domains.Intent.Flow
{
    // Role:
    // Refreshes one monster's intent when an explicitly declared dependency changes.
    public sealed class IntentRefreshFlow
    {
        private readonly IntentPrepareFlow _prepareFlow;

        public IntentRefreshFlow(IntentPrepareFlow prepareFlow)
        {
            _prepareFlow = prepareFlow ?? throw new ArgumentNullException(nameof(prepareFlow));
        }

        public bool Refresh(uint enemyCardId)
        {
            return _prepareFlow.Prepare(enemyCardId);
        }
    }
}
