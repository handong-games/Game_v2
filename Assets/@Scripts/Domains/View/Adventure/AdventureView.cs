using Game.Core.Managers.View;
using Domains.View.Widgets;
using Game.Scenes.Adventure.Events.Widgets;
using Game.Scenes.Adventure;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView : BaseView, IAdventureGameplayCueReceiver
    {
        private readonly AdventureSceneController _controller;
        private readonly AdventureWidgetEvents _widgetEvents;
        private readonly AdventureCardAvatarRegistry _avatarRegistry;
        private readonly ViewTransitionManager _viewTransitionManager;
        private readonly AdventureBoardWidgets _boardWidgets;
        private readonly AdventureBoardUIFlow _boardUIFlow;

        private VisualElement _adventureRoot;
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
        private AdventureEntryPresentationViewModel _entryPresentation;
        private bool _introStarted;

        public AdventureView(
            AdventureSceneController controller,
            AdventureWidgetEvents widgetEvents,
            AdventureCardAvatarRegistry avatarRegistry,
            ViewTransitionManager viewTransitionManager,
            AdventureBoardWidgets boardWidgets,
            AdventureBoardUIFlow boardUIFlow)
        {
            _controller = controller;
            _widgetEvents = widgetEvents;
            _avatarRegistry = avatarRegistry;
            _viewTransitionManager = viewTransitionManager;
            _boardWidgets = boardWidgets;
            _boardUIFlow = boardUIFlow;
        }

        protected override void OnVisualTreeCloned(VisualElement root)
        {
            _adventureRoot = Root.Q<VisualElement>("adventure-root");
            _banner = Root.Q<Banner>("banner");
            _resourceStatusBar = Root.Q<VisualElement>("resource-status-bar");
            _progressBar = Root.Q<VisualElement>("progress-bar");
            _progressBarTrackFill = Root.Q<VisualElement>("progress-bar-track-fill");
            _effectLayer = Root.Q<VisualElement>("adventure-effect-layer");
            _pouch = Root.Q<Pouch>("pouch");
            _coinStatusWidget = Root.Q<CoinStatusWidget>("coin-status-widget");
            _endTurnWidget = Root.Q<EndTurnWidget>("end-turn-widget");
            _arrowWidget = Root.Q<ArrowWidget>("arrow-widget");
            _cardBoard = Root.Q<VisualElement>("card-board");
            _cardDeck = Root.Q<VisualElement>("card-deck");
            
            _endTurnWidget?.Bind(_widgetEvents.Turn);
            _pouch?.Bind(_widgetEvents.Pouch);
            _boardWidgets.Initialize(_cardBoard);

            _controller.StartInitialStage();
            _entryPresentation = _controller.GetEntryPresentation();
            _skillSlots = _controller.GetSkillSlots();
            _targetingEventRoot = Root.Q<VisualElement>("adventure-root") ?? Root;
            _skillSlotGroup = Root.Q<AdventureSkillSlotGroup>("skill-slot-group");
            _skillSlotGroup?.Bind(_skillSlots, _widgetEvents.SkillSlot);

            _coinEffectPlayer = new CoinEffectPlayer();
            _coinEffectPlayer.Bind(_effectLayer);
            _coinChangeEffectPlayer = new CoinChangeEffectPlayer();
        }

        protected override void OnShown()
        {
            if (_introStarted)
                return;

            _introStarted = true;
            
            _ = PlayIntroAnimation();
        }

        public override void Dispose()
        {
            UnbindGameplayCueReceivers();
            ClearSkillPreview();
            ClearCards();
            _pouch?.Unbind();
            _endTurnWidget?.Unbind();
            _skillSlotGroup?.Unbind();

            _coinEffectPlayer?.Clear();
            _coinEffectPlayer = null;
            _coinChangeEffectPlayer = null;
            _entryPresentation = null;
            base.Dispose();
        }
    }
}
