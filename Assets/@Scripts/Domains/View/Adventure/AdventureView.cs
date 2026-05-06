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
        private VisualElement _progressBar;
        private VisualElement _progressBarTrackFill;
        private VisualElement _cardDeck;
        private VisualElement _effectLayer;
        private Pouch _pouch;
        private CoinStatusWidget _coinStatusWidget;
        private SkillSlotWidget _skillSlotWidget;
        private CardDealer _cardDealer;
        private CoinEffectPlayer _coinEffectPlayer;

        protected override void OnBind(VisualElement root)
        {
            _cardBoard = Root.Q<VisualElement>("card-board");
            _banner = Root.Q<Banner>("banner");
            _resourceStatusBar = Root.Q<VisualElement>("resource-status-bar");
            _progressBar = Root.Q<VisualElement>("progress-bar");
            _progressBarTrackFill = Root.Q<VisualElement>("progress-bar-track-fill");
            _cardDeck = Root.Q<VisualElement>("card-deck");
            _effectLayer = Root.Q<VisualElement>("adventure-effect-layer");
            _pouch = Root.Q<Pouch>("pouch");
            _coinStatusWidget = Root.Q<CoinStatusWidget>("coin-status-widget");
            _skillSlotWidget = Root.Q<SkillSlotWidget>("skill-slot-widget");
            _skillSlotWidget.Bind(_controller.GetSkillSlots());

            _cardDealer = new CardDealer();
            _cardDealer.Bind(_cardDeck, _cardBoard);

            _coinEffectPlayer = new CoinEffectPlayer();
            _coinEffectPlayer.Bind(_effectLayer);

            RegisterEvents();
        }

        public override void Dispose()
        {
            UnregisterEvents();
            _cardDealer?.Clear();
            _coinEffectPlayer?.Clear();
            base.Dispose();
        }
    }
}
