using System;
using Domains.Combat;
using Domains.View.Widgets;
using Game.Core.Managers.View;
using UnityEngine;

namespace Game.Scenes.Adventure
{
    // Role:
    // Presents Adventure combat result feedback on the screen.
    // It does not advance Adventure stages or apply combat rewards.
    public sealed class AdventureCombatResultUIFlow
    {
        private readonly ViewTransitionManager _viewTransitionManager;

        public AdventureCombatResultUIFlow(ViewTransitionManager viewTransitionManager)
        {
            _viewTransitionManager = viewTransitionManager ?? throw new ArgumentNullException(nameof(viewTransitionManager));
        }

        public Awaitable Play(Banner banner, ECombatEndResult result)
        {
            if (banner == null)
                throw new ArgumentNullException(nameof(banner));

            return result switch
            {
                ECombatEndResult.Victory => banner.PresentVictory(_viewTransitionManager),
                ECombatEndResult.Defeat => banner.PresentDefeat(_viewTransitionManager),
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
            };
        }
    }
}
