using System;
using Domains.Adventure;
using Domains.Card;
using Domains.Player;
using VContainer.Unity;

namespace Domains.Scene.Adventure
{
    using Card = global::Domains.Card.Card;

    public sealed class AdventureSceneEntryPoint : IStartable, IDisposable
    {
        private readonly AdventureSceneLocalization _localization;
        private readonly AdventureViewEventBinder _viewEventBinder;
        private readonly AdventureSceneInitialData _initialData;
        private readonly AdventureService _adventureService;
        private readonly CardDeckService _cardDeckService;
        private readonly PlayerService _playerService;
        private readonly CardService _cardService;
        private readonly CardBoardService _cardBoardService;
        private readonly AdventureSceneNavigator _navigator;

        public AdventureSceneEntryPoint(
            AdventureSceneLocalization localization,
            AdventureViewEventBinder viewEventBinder,
            AdventureSceneInitialData initialData,
            AdventureService adventureService,
            CardDeckService cardDeckService,
            PlayerService playerService,
            CardService cardService,
            CardBoardService cardBoardService,
            AdventureSceneNavigator navigator)
        {
            _localization = localization;
            _viewEventBinder = viewEventBinder;
            _initialData = initialData;
            _adventureService = adventureService;
            _cardDeckService = cardDeckService;
            _playerService = playerService;
            _cardService = cardService;
            _cardBoardService = cardBoardService;
            _navigator = navigator;
        }

        public void Start()
        {
            InitializeAdventure();
            _localization.Preload();
            _navigator.ShowAdventure();
        }

        public void Dispose()
        {
            _viewEventBinder.Dispose();
            _navigator.HideCurrent();
        }

        private void InitializeAdventure()
        {
            AdventureRun run = new(
                _initialData.SelectedCharacterId,
                _initialData.Adventure.Id,
                _initialData.CardDeck.Id,
                _initialData.Adventure.MaxStageCount,
                _initialData.Seed);

            _adventureService.SetCurrent(run);

            _cardService.Clear();
            _cardBoardService.Clear();

            Card playerCard = _cardService.Create(_initialData.Character);
            _playerService.Initialize(_initialData.Character);
            _playerService.SetPlayerCard(playerCard);
            _cardDeckService.Initialize(_initialData.CardDeck, _initialData.Seed);
        }
    }
}
