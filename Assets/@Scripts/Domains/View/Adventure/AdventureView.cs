using Game.Core.Managers.Dependency;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView : BaseView
    {
        [Inject]
        private AdventureController _controller;

        private VisualElement _cardBoard;
        private VisualElement _adventureBanner;
        private VisualElement _resourceStatusBar;
        private VisualElement _resourceStatusBarRegion;
        private VisualElement _resourceStatusBarResources;
        private VisualElement _progressBar;
        private VisualElement _progressBarTrackFill;
        private VisualElement _cardDeck;
        private CardDealer _cardDealer;

        protected override void OnBind(VisualElement root)
        {
            _cardBoard = Root.Q<VisualElement>("card-board");
            _adventureBanner = Root.Q<VisualElement>("banner");
            _resourceStatusBar = Root.Q<VisualElement>("resource-status-bar");
            _resourceStatusBarRegion = Root.Q<VisualElement>("resource-status-bar-region");
            _resourceStatusBarResources = Root.Q<VisualElement>("resource-status-bar-resources");
            _progressBar = Root.Q<VisualElement>("progress-bar");
            _progressBarTrackFill = Root.Q<VisualElement>("progress-bar-track-fill");
            _cardDeck = Root.Q<VisualElement>("card-deck");

            _cardDealer = new CardDealer();
            _cardDealer.Bind(_cardDeck, _cardBoard);
        }
        
        protected override void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _ = PlayIntroAnimation();
        }

        public override void Dispose()
        {
            _cardDealer?.Clear();
            base.Dispose();
        }

        private async Awaitable PlayPlaceholderCardsAsync()
        {
            if (_cardDealer == null)
                return;

            await _cardDealer.DealPlaceholderAsync(ECardBoardSide.Player, 1);

            await _cardDealer.DealPlaceholderAsync(ECardBoardSide.Encounter, 3);
            await _cardDealer.DealPlaceholderAsync(ECardBoardSide.Encounter, 3);
            await _cardDealer.DealPlaceholderAsync(ECardBoardSide.Encounter, 3);
        }
    }
}
