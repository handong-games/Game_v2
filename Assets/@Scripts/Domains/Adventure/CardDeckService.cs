using System;
using System.Collections.Generic;
using Game.Data;
using Game.Generated;

namespace Domains.Adventure
{
    public sealed class CardDeckService : IDisposable
    {
        private readonly CardDeckState _state;
        private readonly CardDeckBuilder _builder;

        public CardDeckService(CardDeckState state, CardDeckBuilder builder)
        {
            _state = state;
            _builder = builder;
        }

        public CardDeckModel CurrentCardDeck => _state.CurrentCardDeck;

        public void Initialize(CardDeckModel cardDeck, uint seed)
        {
            _builder.Build(_state, cardDeck, seed);
        }

        public IReadOnlyList<CardModelBase> DrawCards(uint count)
        {
            List<CardModelBase> models = new();

            int drawCount = _state.GetAvailableDrawCount(count);
            for (int i = 0; i < drawCount; i++)
            {
                models.Add(_state.DrawNext());
            }

            return models;
        }

        public bool TryResolveChoice(CardModelBase model, out CardModelBase resolvedModel)
        {
            resolvedModel = null;
            if (!model.TryGetChoiceType(out EChoiceCardType choiceType))
                return false;

            resolvedModel = ResolveChoiceCard(choiceType);
            return true;
        }

        public void Dispose()
        {
            _state.Clear();
        }

        private CardModelBase ResolveChoiceCard(EChoiceCardType choiceType)
        {
            switch (choiceType)
            {
                case EChoiceCardType.Monster:
                case EChoiceCardType.Elite:
                    return _state.TakeRandomMonster();
                case EChoiceCardType.Boss:
                    return GetBoss();
                case EChoiceCardType.Event:
                    return _state.TakeRandomEvent();
                case EChoiceCardType.Shop:
                    return GetShop();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private MonsterModel GetBoss()
        {
            MonsterModel boss = CurrentCardDeck.Boss;
            if (boss == null)
                throw new InvalidOperationException($"{CurrentCardDeck.Name} requires boss.");

            return boss;
        }

        private ShopModel GetShop()
        {
            ShopModel shop = CurrentCardDeck.Shop;
            if (shop == null)
                throw new InvalidOperationException($"{CurrentCardDeck.Name} requires shop.");

            return shop;
        }
    }
}
