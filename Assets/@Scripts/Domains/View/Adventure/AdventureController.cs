using System;
using System.Collections.Generic;
using Domains.Card;
using Domains.Event;
using Domains.Player;
using Domains.Scene;
using Game.Core.Managers.DB;
using Game.Core.Managers.Dependency;
using Game.Data;

namespace Domains.Adventure
{
    using Card = global::Domains.Card.Card;

    [Dependency(nameof(AdventureScene))]
    public sealed partial class AdventureController : IDisposable
    {
        [Inject] private PlayerService _playerService;
        [Inject] private AdventureService _adventureService;
        [Inject] private CardDeckService _cardDeckService;
        [Inject] private CardService _cardService;
        [Inject] private CardBoardService _cardBoardService;

        public void StartAdventure()
        {
            RegisterEvents();

            AdventureEvents.AdventureStarted?.Invoke();
        }

        public void Dispose()
        {
            UnregisterEvents();
        }

        public CoinFlipDto OnPouchClicked()
        {
            return _playerService.OpenPouch();
        }

        public AdventureInitialViewModel CreateInitialViewModel()
        {
            return new AdventureInitialViewModel(GetSkillSlotViewModelsV2());
        }

        public void OnIntroAnimationCompleted()
        {
            InitStage();
        }

        public void OnCardClicked(uint cardId)
        {
            if (!_cardService.TryGet(cardId, out Card card))
                return;

            if (!_cardDeckService.TryResolveChoice(card.Model, out ICardModel resolvedModel))
                return;

            _cardService.Replace(cardId, resolvedModel, CardViewModelFactory.GetDefaultFace(resolvedModel));

            _cardBoardService.MoveAllExcept(ECardZone.Right, cardId, ECardZone.Removed);
            AdventureEvents.BoardChanged?.Invoke(CreateBoardCards());
        }

        public void OnEndTurnClicked()
        {
        }

        private void RegisterEvents()
        {
            AdventureEvents.CardDealCompleted += OnCardDealCompleted;
            AdventureEvents.StageCompleted += OnStageCompleted;
        }

        private void UnregisterEvents()
        {
            AdventureEvents.CardDealCompleted -= OnCardDealCompleted;
            AdventureEvents.StageCompleted -= OnStageCompleted;
        }

        private void OnCardDealCompleted()
        {
            if (_adventureService.GetCurrentStageType() == EAdventureStageType.Choice)
                return;

            AdventureEvents.TurnBannerRequested?.Invoke();
        }

        private void OnStageCompleted()
        {
            _adventureService.AdvanceStage();
            NextStage();
        }

        private void InitStage()
        {
            AdventureStageDto currentStageDto = _adventureService.GetCurrentStage();

            List<Card> cards = new();
            Card playerCard = _playerService.GetPlayerCard();
            if (playerCard != null)
            {
                cards.Add(playerCard);
            }

            uint drawCount = playerCard != null && currentStageDto.DrawCount > 0
                ? currentStageDto.DrawCount - 1
                : currentStageDto.DrawCount;

            IReadOnlyList<ICardModel> models = _cardDeckService.DrawCards(drawCount);
            for (int i = 0; i < models.Count; i++)
            {
                cards.Add(_cardService.Create(models[i]));
            }

            _cardBoardService.Clear();
            for (int i = 0; i < cards.Count; i++)
            {
                _cardBoardService.PlaceCard(GetZone(cards[i].Model), cards[i].CardId);
            }

            AdventureEvents.CardsDrawn?.Invoke(CreateBoardCards());
        }

        private void NextStage()
        {
            AdventureStageDto currentStageDto = _adventureService.GetCurrentStage();

            List<Card> cards = new();

            IReadOnlyList<ICardModel> models = _cardDeckService.DrawCards(currentStageDto.DrawCount);
            for (int i = 0; i < models.Count; i++)
            {
                cards.Add(_cardService.Create(models[i]));
            }

            _cardBoardService.Clear();
            for (int i = 0; i < cards.Count; i++)
            {
                _cardBoardService.PlaceCard(GetZone(cards[i].Model), cards[i].CardId);
            }

            AdventureEvents.CardsDrawn?.Invoke(CreateBoardCards());
        }

        private IReadOnlyList<AdventureCardViewModel> CreateBoardCards()
        {
            List<AdventureCardViewModel> cards = new();
            cards.AddRange(CreateBoardZoneCards(ECardZone.Left));
            cards.AddRange(CreateBoardZoneCards(ECardZone.Right));
            return cards;
        }

        private IReadOnlyList<AdventureCardViewModel> CreateBoardZoneCards(ECardZone zone)
        {
            IReadOnlyList<uint> cardIds = _cardBoardService.GetCardIds(zone);
            List<AdventureCardViewModel> cards = new(cardIds.Count);

            for (int i = 0; i < cardIds.Count; i++)
            {
                uint cardId = cardIds[i];
                if (!_cardService.TryGet(cardId, out Card card))
                    continue;

                cards.Add(new AdventureCardViewModel(
                    cardId,
                    zone,
                    CardViewModelFactory.Create(card),
                    card.AbilitySystem));
            }

            return cards;
        }

        private static ECardZone GetZone(ICardModel model)
        {
            return model is CharacterModel
                ? ECardZone.Left
                : ECardZone.Right;
        }
    }
}
