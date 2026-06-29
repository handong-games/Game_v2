using System.Collections.Generic;
using System;
using Domains.Combat;
using Domains.Intent.Presentation;

namespace Domains.Adventure
{
    // Role:
    // Carries all presentation data needed when the Adventure screen enters player turn.
    // Game flow prepares the data; UI flow decides how to present it.
    public sealed class AdventurePlayerTurnStartViewModel
    {
        public AdventurePlayerTurnStartViewModel(
            CombatTurnViewModel turn,
            IReadOnlyList<MonsterIntentRevealViewModel> intentReveals)
        {
            Turn = turn;
            IntentReveals = intentReveals ?? Array.Empty<MonsterIntentRevealViewModel>();
        }

        public CombatTurnViewModel Turn { get; }
        public IReadOnlyList<MonsterIntentRevealViewModel> IntentReveals { get; }
    }
}
