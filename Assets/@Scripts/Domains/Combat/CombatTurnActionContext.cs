using System;

namespace Domains.Combat
{
    public sealed class CombatTurnActionContext
    {
        private readonly Action _onCompleted;
        private bool _completed;

        public CombatTurnActionContext(Action onCompleted)
        {
            _onCompleted = onCompleted;
        }

        public void Complete()
        {
            if (_completed)
                return;

            _completed = true;
            _onCompleted?.Invoke();
        }
    }
}
