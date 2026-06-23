using System.Collections.Generic;
using Domains.Card;
using Domains.Combat;
using Domains.Player;
using Game.AbilitySystem;
using Game.Core.Managers.DB;
using Game.Data;
using Gameplay.GAS;

namespace Domains.Adventure
{
    using Card = global::Domains.Card.Card;

    public sealed partial class AdventureController
    {
        private readonly PlayerService _playerService;
        private readonly AdventureService _adventureService;
        private readonly CardDeckService _cardDeckService;
        private readonly CardService _cardService;
        private readonly CardBoardService _cardBoardService;
        private readonly CombatService _combatService;
        private readonly AdventureEvents _events;

        public AdventureController(
            PlayerService playerService,
            AdventureService adventureService,
            CardDeckService cardDeckService,
            CardService cardService,
            CardBoardService cardBoardService,
            CombatService combatService,
            AdventureEvents events)
        {
            _playerService = playerService;
            _adventureService = adventureService;
            _cardDeckService = cardDeckService;
            _cardService = cardService;
            _cardBoardService = cardBoardService;
            _combatService = combatService;
            _events = events;
        }

        public void OnPouchClicked()
        {
            Card playerCard = _playerService.GetPlayerCard();
            if (playerCard == null)
                return;

            GameplayEventData eventData = new(AbilityGameplayTags.EventCoinFlip)
            {
                Instigator = playerCard.AbilitySystem
            };

            playerCard.AbilitySystem.HandleGameplayEvent(eventData);
        }

        public AdventureInitialViewModel StartInitialStage()
        {
            InitStage();
            return new AdventureInitialViewModel(
                GetSkillSlotViewModels(),
                CreateBoardCards());
        }

        public CombatTurnViewModel GetCombatTurnViewModel()
        {
            return new CombatTurnViewModel(
                _combatService.CurrentSide,
                _combatService.RoundNumber);
        }

        public void OnCardClicked(uint cardId)
        {
            if (!_cardService.TryGet(cardId, out Card card))
                return;

            if (!_cardDeckService.TryResolveChoice(card.Model, out CardModelBase resolvedModel))
                return;

            _cardService.Replace(cardId, resolvedModel, CardViewModelFactory.GetDefaultFace(resolvedModel));

            _cardBoardService.MoveAllExcept(ECardZone.Right, cardId, ECardZone.Removed);
            _events.Board.RefreshRequested?.Invoke(CreateBoardCards());
        }

        public void OnEndTurnClicked()
        {
            _combatService.NextTurn();
        }

        public void OnEnemyTurnCompleted()
        {
            _combatService.NextTurn();
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

            IReadOnlyList<CardModelBase> models = _cardDeckService.DrawCards(drawCount);
            for (int i = 0; i < models.Count; i++)
            {
                cards.Add(_cardService.Create(models[i]));
            }

            _cardBoardService.Clear();
            for (int i = 0; i < cards.Count; i++)
            {
                _cardBoardService.PlaceCard(GetZone(cards[i].Model), cards[i].CardId);
            }

            ReadyCombat(cards);
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

        private static ECardZone GetZone(CardModelBase model)
        {
            return model is CharacterModel
                ? ECardZone.Left
                : ECardZone.Right;
        }

        private void ReadyCombat(IReadOnlyList<Card> cards)
        {
            List<CombatCard> combatCards = new(cards.Count);
            for (int i = 0; i < cards.Count; i++)
            {
                Card card = cards[i];
                combatCards.Add(new CombatCard(card, GetCombatSide(card.Model)));
            }

            _combatService.ReadyCombat(combatCards);
        }

        private static ECombatSide GetCombatSide(CardModelBase model)
        {
            return model is CharacterModel
                ? ECombatSide.Player
                : ECombatSide.Enemy;
        }
    }
}
