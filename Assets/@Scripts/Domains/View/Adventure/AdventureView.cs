using Game.Core.Managers.Dependency;
using Game.Core.Managers.View;
using Domains.View.Widgets;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView : BaseView
    {
        [Inject]
        private AdventureController _controller;

        private VisualElement _cardBoard;
        private Banner _banner;
        private VisualElement _resourceStatusBar;
        private VisualElement _resourceStatusBarRegion;
        private VisualElement _resourceStatusBarResources;
        private VisualElement _progressBar;
        private VisualElement _progressBarTrackFill;
        private VisualElement _cardDeck;
        private Pouch _pouch;
        private CardDealer _cardDealer;

        protected override void OnBind(VisualElement root)
        {
            _cardBoard = Root.Q<VisualElement>("card-board");
            _banner = Root.Q<Banner>("banner");
            _resourceStatusBar = Root.Q<VisualElement>("resource-status-bar");
            _resourceStatusBarRegion = Root.Q<VisualElement>("resource-status-bar-region");
            _resourceStatusBarResources = Root.Q<VisualElement>("resource-status-bar-resources");
            _progressBar = Root.Q<VisualElement>("progress-bar");
            _progressBarTrackFill = Root.Q<VisualElement>("progress-bar-track-fill");
            _cardDeck = Root.Q<VisualElement>("card-deck");
            _pouch = Root.Q<Pouch>("pouch");

            _cardDealer = new CardDealer();
            _cardDealer.Bind(_cardDeck, _cardBoard);

            RegisterEvents();
        }

        public override void Dispose()
        {
            UnregisterEvents();
            _cardDealer?.Clear();
            base.Dispose();
        }
    }
}
