using System;
using System.Collections.Generic;
using Domains.Combat;

namespace Domains.Adventure
{
    public sealed class AdventureEvents
    {
        public AdventureBoardEvents Board { get; }
        public AdventureCombatEvents Combat { get; }

        public AdventureEvents(
            AdventureBoardEvents board,
            AdventureCombatEvents combat)
        {
            Board = board;
            Combat = combat;
        }
    }

    public sealed class AdventureBoardEvents
    {
        public Action<IReadOnlyList<AdventureCardViewModel>> RefreshRequested;
    }

    public sealed class AdventureCombatEvents
    {
        public Action PlayerTurnBannerRequested;
        public Action EnemyTurnBannerRequested;
        public Action<ECombatEndResult> ResultRequested;
    }
}
