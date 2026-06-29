using System;
using UnityEngine;

namespace Domains.Adventure
{
    // Role:
    // Plays AdventureView intro animation and binds runtime card presentation after it completes.
    public sealed partial class AdventureView
    {
        private async Awaitable<bool> PlayIntroOnce(
            int screenLifetimeVersion,
            AdventureInitialPresentationViewModel initialPresentation)
        {
            if (_introStarted)
                return true;

            _introStarted = true;
            return await PlayIntroAnimation(screenLifetimeVersion, initialPresentation);
        }

        private async Awaitable<bool> PlayIntroAnimation(
            int screenLifetimeVersion,
            AdventureInitialPresentationViewModel initialPresentation)
        {
            if (initialPresentation == null)
                throw new ArgumentNullException(nameof(initialPresentation));

            await _screenUIFlow.PlayIntro(
                _screenWidgets.AdventureRoot,
                _screenWidgets.Background,
                _screenWidgets.Emblem,
                _screenWidgets.CardDeck,
                _screenWidgets.ResourceStatusBar,
                _screenWidgets.Banner,
                initialPresentation,
                () => IsCurrentScreenLifetime(screenLifetimeVersion));

            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            BindBoardRuntimeConnections(initialPresentation.RuntimeBoardCards);
            return true;
        }
    }
}
