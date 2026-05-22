using System;
using System.Collections.Generic;
using Domains.Combat;
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
        [Inject] private CombatService _combatService;

        private EAdventureStageType _currentStageType;

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

        public void OnCardClicked(uint cardId)
        {
            if (!_cardService.TryGet(cardId, out Card card))
                return;

            if (!_cardDeckService.TryResolveChoice(card.Model, out ICardModel resolvedModel))
                return;

            _cardService.Replace(cardId, resolvedModel, CardViewModelFactory.GetDefaultFace(resolvedModel));

            if (!_cardService.TryGet(cardId, out card))
                return;

            RegisterCombatCreature(card);

            _cardBoardService.MoveAllExcept(ECardZone.Right, cardId, ECardZone.Removed);
            AdventureEvents.BoardChanged?.Invoke(CreateBoardViewModel());
        }

        public void OnEndTurnClicked()
        {
            _combatService.EndPlayerTurn();
        }

        private void RegisterEvents()
        {
            AdventureEvents.IntroCompleted += OnIntroCompleted;
            AdventureEvents.CardDealCompleted += OnCardDealCompleted;
            AdventureEvents.StageCompleted += OnStageCompleted;
        }

        private void UnregisterEvents()
        {
            AdventureEvents.IntroCompleted -= OnIntroCompleted;
            AdventureEvents.CardDealCompleted -= OnCardDealCompleted;
            AdventureEvents.StageCompleted -= OnStageCompleted;
        }

        private void OnIntroCompleted()
        {
            DealCurrentStageCards();
        }

        private void OnCardDealCompleted()
        {
            if (_currentStageType == EAdventureStageType.Choice)
                return;

            AdventureEvents.TurnBannerRequested?.Invoke();
        }

        private void OnStageCompleted()
        {
            _adventureService.AdvanceStage();
            DealCurrentStageCards();
        }

        private void DealCurrentStageCards()
        {
            AdventureStageRequest request = _adventureService.CreateCurrentStageRequest();
            _currentStageType = request.StageType;

            IReadOnlyList<Card> cards = DrawCurrentStageCards(request);
            _cardBoardService.Clear();
            for (int i = 0; i < cards.Count; i++)
            {
                RegisterCombatCreature(cards[i]);
                _cardBoardService.PlaceCard(GetZone(cards[i].Model), cards[i].CardId);
            }

            AdventureEvents.CardsDrawn?.Invoke(CreateBoardViewModel());
        }

        private IReadOnlyList<Card> DrawCurrentStageCards(AdventureStageRequest request)
        {
            List<Card> cards = new();
            Card playerCard = _playerService.GetPlayerCard();

            if (request.StageType == EAdventureStageType.First && playerCard != null)
            {
                cards.Add(playerCard);
            }

            uint drawCount = request.StageType == EAdventureStageType.First && request.DrawCount > 0
                ? request.DrawCount - 1
                : request.DrawCount;

            IReadOnlyList<ICardModel> models = _cardDeckService.DrawCards(drawCount);
            for (int i = 0; i < models.Count; i++)
            {
                cards.Add(_cardService.Create(models[i]));
            }

            return cards;
        }

        private void RegisterCombatCreature(Card card)
        {
            if (_combatService.TryGetCreature(card.CardId, out _))
                return;

            switch (card.Model)
            {
                case CharacterModel character:
                    _combatService.RegisterPlayer(card.CardId, character);
                    break;
                case MonsterModel monster:
                    _combatService.RegisterMonster(card.CardId, monster);
                    break;
            }
        }

        private CardBoardViewModel CreateBoardViewModel()
        {
            return new CardBoardViewModel(
                CreateBoardCards(ECardZone.Left),
                CreateBoardCards(ECardZone.Right));
        }

        private BoardCardViewModel[] CreateBoardCards(ECardZone zone)
        {
            var cardIds = _cardBoardService.GetCardIds(zone);
            List<BoardCardViewModel> cards = new(cardIds.Count);

            for (int i = 0; i < cardIds.Count; i++)
            {
                uint cardId = cardIds[i];
                if (!_cardService.TryGet(cardId, out Card card))
                    continue;

                CardHealthBinding health = _combatService.TryGetCreature(cardId, out CreatureState creature)
                    ? new CardHealthBinding(creature)
                    : null;

                cards.Add(new BoardCardViewModel(
                    cardId,
                    CardViewModelFactory.Create(card),
                    health));
            }

            return cards.ToArray();
        }

        private static ECardZone GetZone(ICardModel model)
        {
            return model is CharacterModel
                ? ECardZone.Left
                : ECardZone.Right;
        }
    }
}
