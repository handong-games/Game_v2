using System.Collections.Generic;
using Domains.Event;
using Domains.Player;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private void RegisterEvents()
        {
            AdventureEvents.AdventureStarted += OnAdventureStarted;
            AdventureEvents.CardsDrawn += OnCardsDrawn;
            AdventureEvents.TurnBannerRequested += OnTurnBannerRequested;
            _pouch.Clicked += OnPouchClicked;
        }

        private void UnregisterEvents()
        {
            AdventureEvents.AdventureStarted -= OnAdventureStarted;
            AdventureEvents.CardsDrawn -= OnCardsDrawn;
            AdventureEvents.TurnBannerRequested -= OnTurnBannerRequested;
            _pouch.Clicked -= OnPouchClicked;
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

        private async void OnPouchClicked()
        {
            CoinFlipDto coinFlip = _controller.OnPouchClicked();
            _coinStatusWidget.Reset();

            await _coinStatusWidget.Show();
            await _coinEffectPlayer.Play(
                coinFlip,
                _pouch,
                _coinStatusWidget.GetTarget(ECoinFace.Heads),
                _coinStatusWidget.GetTarget(ECoinFace.Tails),
                _coinStatusWidget.Add);

            await _skillSlotWidget.Show();
        }
    }
}
