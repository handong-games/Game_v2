using System.Collections.Generic;

namespace Domains.Intent.Runtime
{
    // Role:
    // Stores intent runtime states by CardId for the active Adventure combat.
    public sealed class IntentRuntime
    {
        private readonly Dictionary<uint, IntentRuntimeState> _statesByCardId = new();

        public void Clear()
        {
            _statesByCardId.Clear();
        }

        public IntentRuntimeState GetOrCreate(uint cardId)
        {
            if (_statesByCardId.TryGetValue(cardId, out IntentRuntimeState state))
                return state;

            state = new IntentRuntimeState();
            _statesByCardId.Add(cardId, state);
            return state;
        }

        public bool TryGet(uint cardId, out IntentRuntimeState state)
        {
            return _statesByCardId.TryGetValue(cardId, out state);
        }

        public bool Remove(uint cardId)
        {
            return _statesByCardId.Remove(cardId);
        }
    }
}
