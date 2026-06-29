using Domains.Adventure;
using Domains.View.Widgets;
using Game.Core.Managers.View;
using System;
using UnityEngine;

namespace Game.Scenes.Adventure
{
    // Role:
    // Owns enemy-turn presentation.
    // It does not start enemy actions or mutate combat runtime state.
    public sealed class AdventureEnemyTurnUIFlow
    {
        private readonly ViewTransitionManager _viewTransitionManager;

        public AdventureEnemyTurnUIFlow(ViewTransitionManager viewTransitionManager)
        {
            _viewTransitionManager = viewTransitionManager ?? throw new ArgumentNullException(nameof(viewTransitionManager));
        }

        public Awaitable PlayStart(
            Banner banner,
            AdventureEnemyTurnStartViewModel viewModel)
        {
            if (banner == null)
                throw new ArgumentNullException(nameof(banner));

            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            return banner.PresentEnemyTurn(_viewTransitionManager);
        }
    }
}
