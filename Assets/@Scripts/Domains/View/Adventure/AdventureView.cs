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

        private Banner _banner;
        private VisualElement _resourceStatusBar;
        private VisualElement _progressBar;
        private VisualElement _progressBarTrackFill;
        private VisualElement _effectLayer;
        private Pouch _pouch;
        private CoinStatusWidget _coinStatusWidget;
        private EndTurnWidget _endTurnWidget;
        private ArrowWidget _arrowWidget;
        private CoinEffectPlayer _coinEffectPlayer;
        private CoinChangeEffectPlayer _coinChangeEffectPlayer;

        protected override void OnBind(VisualElement root)
        {
            AdventureInitialViewModel initialViewModel = _controller.CreateInitialViewModel();

            _banner = Root.Q<Banner>("banner");
            _resourceStatusBar = Root.Q<VisualElement>("resource-status-bar");
            _progressBar = Root.Q<VisualElement>("progress-bar");
            _progressBarTrackFill = Root.Q<VisualElement>("progress-bar-track-fill");
            _effectLayer = Root.Q<VisualElement>("adventure-effect-layer");
            _pouch = Root.Q<Pouch>("pouch");
            _coinStatusWidget = Root.Q<CoinStatusWidget>("coin-status-widget");
            _endTurnWidget = Root.Q<EndTurnWidget>("end-turn-widget");
            _arrowWidget = Root.Q<ArrowWidget>("arrow-widget");
            BindCards();
            BindSkillSlots(initialViewModel.SkillSlots);

            _coinEffectPlayer = new CoinEffectPlayer();
            _coinEffectPlayer.Bind(_effectLayer);
            _coinChangeEffectPlayer = new CoinChangeEffectPlayer();

            CoinFlipCueEventBus.Published += OnCoinFlipCuePublished;
            CoinChangeCueEventBus.Published += OnCoinChangeCuePublished;
            RegisterEvents();
        }

        public override void Dispose()
        {
            ClearSkillPreview();
            ClearCards();
            UnregisterEvents();
            CoinFlipCueEventBus.Published -= OnCoinFlipCuePublished;
            CoinChangeCueEventBus.Published -= OnCoinChangeCuePublished;
            _coinEffectPlayer?.Clear();
            base.Dispose();
        }

        private void OnCoinFlipCuePublished(CoinFlipCueData data)
        {
            _ = PlayCoinFlipAsync(data);
        }

        private void OnCoinChangeCuePublished(CoinChangeCueData data)
        {
            _ = PlayCoinChangeAsync(data);
        }
    }
}
