using System;
using System.Collections.Generic;
using Game.Core.Managers.DB;
using Game.Core.Managers.Dependency;
using Game.Data;
using Game.Generated;

namespace Domains.Adventure
{
    [Dependency]
    public sealed class CardDeckService : IDisposable
    {
        private readonly List<CardState> _deck = new();
        private readonly List<CardState> _drawnCards = new();

        private CardState _playerCard;

        public CardDeckModel CurrentCardDeck { get; private set; }
        public ECharacter CharacterId { get; private set; }

        public void Initialize(ECardDeck cardDeckId, ECharacter characterId)
        {
            CurrentCardDeck = DBManager.Instance.CardDeck.Get(cardDeckId);
            CharacterId = characterId;
            _deck.Clear();
            _drawnCards.Clear();
            _playerCard = null;

            BuildCards();
        }

        public IReadOnlyList<CardState> DrawCard(uint count)
        {
            _drawnCards.Clear();

            int drawCount = (int)Math.Min(count, (uint)_deck.Count);
            for (int i = 0; i < drawCount; i++)
            {
                int lastIndex = _deck.Count - 1;
                CardState card = _deck[lastIndex];
                _deck.RemoveAt(lastIndex);
                _drawnCards.Add(card);
            }

            return _drawnCards;
        }

        public void Dispose()
        {
            CurrentCardDeck = null;
            _deck.Clear();
            _drawnCards.Clear();
            _playerCard = null;
        }

        private void BuildCards()
        {
            AddBossCard();
            AddShopCards();
            AddEventCards();
            AddMonsterCards();

            _deck.Add(CreateFirstMonsterCard());
            _deck.Add(GetOrCreatePlayerCard());
        }

        private CardState GetOrCreatePlayerCard()
        {
            if (_playerCard != null)
                return _playerCard;

            CharacterModel model = DBManager.Instance.Character.Get(CharacterId);
            _playerCard = new CardState(
                model,
                ECardKind.Player,
                ECardFace.Front,
                ECardBoardSide.Left);

            return _playerCard;
        }

        private CardState CreateFirstMonsterCard()
        {
            MonsterModel[] pool = CurrentCardDeck.MonsterPool;
            if (pool == null || pool.Length == 0)
                throw new InvalidOperationException($"{CurrentCardDeck.Name} requires at least one monster.");

            MonsterModel model = pool[0];
            return new CardState(
                model,
                GetMonsterKind(model),
                ECardFace.Front,
                ECardBoardSide.Right);
        }

        private void AddMonsterCards()
        {
            MonsterModel[] pool = CurrentCardDeck.MonsterPool;
            if (CurrentCardDeck.MonsterCount == 0 || pool == null || pool.Length == 0)
                return;

            for (uint i = 0; i < CurrentCardDeck.MonsterCount; i++)
            {
                MonsterModel model = pool[(int)((i + 1) % (uint)pool.Length)];
                _deck.Add(new CardState(
                    model,
                    GetMonsterKind(model),
                    ECardFace.Back,
                    ECardBoardSide.Right));
            }
        }

        private void AddEventCards()
        {
            EventModel[] pool = CurrentCardDeck.EventPool;
            if (CurrentCardDeck.EventCount == 0 || pool == null || pool.Length == 0)
                return;

            for (uint i = 0; i < CurrentCardDeck.EventCount; i++)
            {
                EventModel model = pool[(int)(i % (uint)pool.Length)];
                _deck.Add(new CardState(
                    model,
                    ECardKind.Event,
                    ECardFace.Back,
                    ECardBoardSide.Right));
            }
        }

        private void AddShopCards()
        {
            if (CurrentCardDeck.ShopCount == 0 || CurrentCardDeck.Shop == null)
                return;

            for (uint i = 0; i < CurrentCardDeck.ShopCount; i++)
            {
                _deck.Add(new CardState(
                    CurrentCardDeck.Shop,
                    ECardKind.Shop,
                    ECardFace.Back,
                    ECardBoardSide.Right));
            }
        }

        private void AddBossCard()
        {
            if (CurrentCardDeck.Boss == null)
                return;

            _deck.Add(new CardState(
                CurrentCardDeck.Boss,
                ECardKind.Boss,
                ECardFace.Back,
                ECardBoardSide.Right));
        }

        private static ECardKind GetMonsterKind(MonsterModel model)
        {
            return model.Rank == EMonsterRank.Boss
                ? ECardKind.Boss
                : ECardKind.Monster;
        }
    }
}
