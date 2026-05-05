using System.Collections.Generic;
using Domains.Event;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private void RegisterEvents()
        {
            AdventureEvents.AdventureStarted += OnAdventureStarted;
            AdventureEvents.CardsDrawn += OnCardsDrawn;
            AdventureEvents.TurnBannerRequested += OnTurnBannerRequested;
        }

        private void UnregisterEvents()
        {
            AdventureEvents.AdventureStarted -= OnAdventureStarted;
            AdventureEvents.CardsDrawn -= OnCardsDrawn;
            AdventureEvents.TurnBannerRequested -= OnTurnBannerRequested;
        }

        private void OnAdventureStarted()
        {
            _ = PlayIntroAnimation();
        }

        private async void OnCardsDrawn(IReadOnlyList<CardState> cards)
        {
            if (_cardDealer == null)
                return;

            await _cardDealer.DealAsync(cards);

            AdventureEvents.CardDealCompleted?.Invoke();
        }

        private async void OnTurnBannerRequested()
        {
            await PlayTurnBannerAnimation();
        }
    }
}
