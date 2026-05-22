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
            AdventureEvents.BoardChanged += OnBoardChanged;
            AdventureEvents.TurnBannerRequested += OnTurnBannerRequested;
            _pouch.Clicked += OnPouchClicked;
            _endTurnWidget.Clicked += OnEndTurnClicked;
        }

        private void UnregisterEvents()
        {
            AdventureEvents.AdventureStarted -= OnAdventureStarted;
            AdventureEvents.CardsDrawn -= OnCardsDrawn;
            AdventureEvents.BoardChanged -= OnBoardChanged;
            AdventureEvents.TurnBannerRequested -= OnTurnBannerRequested;
            _pouch.Clicked -= OnPouchClicked;
            _endTurnWidget.Clicked -= OnEndTurnClicked;
        }

        private void OnAdventureStarted()
        {
            _ = PlayIntroAnimation();
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

            await ShowSkillSlots();
            await _endTurnWidget.Show();
        }

        private void OnEndTurnClicked()
        {
            _controller.OnEndTurnClicked();
            _endTurnWidget.Hide();
        }
    }
}
