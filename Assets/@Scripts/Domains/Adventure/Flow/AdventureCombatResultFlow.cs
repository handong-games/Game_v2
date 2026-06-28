using System;
using Domains.Combat;
using Game.Scenes.Adventure.Events.Flow;

namespace Domains.Adventure
{
    // Role:
    // Applies combat result to Adventure progression and notifies scene events.
    // It does not observe death messages or execute enemy actions.
    public sealed class AdventureCombatResultFlow
    {
        private readonly AdventureProgress _progress;
        private readonly AdventureCombatRuntime _combat;
        private readonly AdventureCombatEvents _events;

        public AdventureCombatResultFlow(
            AdventureProgress progress,
            AdventureCombatRuntime combat,
            AdventureCombatEvents events)
        {
            _progress = progress;
            _combat = combat;
            _events = events;
        }

        public void CompleteCombat(ECombatEndResult result)
        {
            if (_combat.IsEnded)
                return;

            _combat.EndCombat();

            switch (result)
            {
                case ECombatEndResult.Victory:
                    _progress.EnterReward();
                    break;
                case ECombatEndResult.Defeat:
                    _progress.EnterDefeat();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }

            _events.ResultRequested?.Invoke(result);
        }
    }
}
