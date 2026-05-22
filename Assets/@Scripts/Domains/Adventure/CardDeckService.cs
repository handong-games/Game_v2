using System;
using System.Collections.Generic;
using Game.Core.Managers.DB;
using Game.Core.Managers.Dependency;
using Game.Core.Utility;
using Game.Data;
using Game.Generated;
using UnityRandom = Unity.Mathematics.Random;

namespace Domains.Adventure
{
    [Dependency]
    public sealed class CardDeckService : IDisposable
    {
        private readonly List<ICardModel> _deck = new();
        private readonly List<MonsterModel> _remainingMonsterPool = new();
        private readonly List<EventModel> _remainingEventPool = new();

        private UnityRandom _random;
        private int _drawIndex;

        public CardDeckModel CurrentCardDeck { get; private set; }

        public void Initialize(ECardDeck cardDeckId, uint seed)
        {
            CurrentCardDeck = DBManager.Instance.CardDeck.Get(cardDeckId);
            _random = new UnityRandom(RandomUtility.CombineSeed(seed, "CardDeck"));
            _drawIndex = 0;

            _deck.Clear();
            _remainingMonsterPool.Clear();
            _remainingEventPool.Clear();

            BuildModelPools();
            BuildDeck();
        }

        public IReadOnlyList<ICardModel> DrawCards(uint count)
        {
            List<ICardModel> models = new();

            int drawCount = (int)Math.Min(count, (uint)(_deck.Count - _drawIndex));
            for (int i = 0; i < drawCount; i++)
            {
                models.Add(_deck[_drawIndex]);
                _drawIndex++;
            }

            return models;
        }

        public bool TryResolveChoice(ICardModel model, out ICardModel resolvedModel)
        {
            resolvedModel = null;
            if (!model.TryGetChoiceType(out EChoiceCardType choiceType))
                return false;

            resolvedModel = ResolveChoiceCard(choiceType);
            return true;
        }

        public void Dispose()
        {
            CurrentCardDeck = null;
            _deck.Clear();
            _remainingMonsterPool.Clear();
            _remainingEventPool.Clear();
            _drawIndex = 0;
        }

        private void BuildModelPools()
        {
            MonsterModel[] monsterPool = CurrentCardDeck.MonsterPool;
            if (monsterPool != null)
            {
                for (int i = 0; i < monsterPool.Length; i++)
                {
                    _remainingMonsterPool.Add(monsterPool[i]);
                }
            }

            EventModel[] eventPool = CurrentCardDeck.EventPool;
            if (eventPool != null)
            {
                for (int i = 0; i < eventPool.Length; i++)
                {
                    _remainingEventPool.Add(eventPool[i]);
                }
            }
        }

        private void BuildDeck()
        {
            _deck.Add(TakeFirstMonster());

            List<ICardModel> choiceCards = BuildChoiceCards();
            Shuffle(choiceCards);

            for (int i = 0; i < choiceCards.Count; i++)
            {
                _deck.Add(choiceCards[i]);
            }

            _deck.Add(GetRequiredCard(CurrentCardDeck.BossChoiceCard, "boss choice card"));
        }

        private List<ICardModel> BuildChoiceCards()
        {
            List<ICardModel> choiceCards = new();

            for (uint i = 0; i < CurrentCardDeck.MonsterCardCount; i++)
            {
                choiceCards.Add(GetRequiredCard(CurrentCardDeck.MonsterChoiceCard, "monster choice card"));
            }

            for (uint i = 0; i < CurrentCardDeck.EventCardCount; i++)
            {
                choiceCards.Add(GetRequiredCard(CurrentCardDeck.EventChoiceCard, "event choice card"));
            }

            for (uint i = 0; i < CurrentCardDeck.ShopCardCount; i++)
            {
                choiceCards.Add(GetRequiredCard(CurrentCardDeck.ShopChoiceCard, "shop choice card"));
            }

            return choiceCards;
        }

        private ICardModel GetRequiredCard(ICardModel card, string label)
        {
            if (card == null)
                throw new InvalidOperationException($"{CurrentCardDeck.Name} requires {label}.");

            return card;
        }

        private MonsterModel TakeFirstMonster()
        {
            if (_remainingMonsterPool.Count == 0)
                throw new InvalidOperationException($"{CurrentCardDeck.Name} requires at least one monster.");

            MonsterModel monster = _remainingMonsterPool[0];
            _remainingMonsterPool.RemoveAt(0);
            return monster;
        }

        private ICardModel ResolveChoiceCard(EChoiceCardType choiceType)
        {
            switch (choiceType)
            {
                case EChoiceCardType.Monster:
                case EChoiceCardType.Elite:
                    return TakeRandomMonster();
                case EChoiceCardType.Boss:
                    return GetBoss();
                case EChoiceCardType.Event:
                    return TakeRandomEvent();
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

        private MonsterModel TakeRandomMonster()
        {
            if (_remainingMonsterPool.Count == 0)
                throw new InvalidOperationException($"{CurrentCardDeck.Name} has no remaining monster.");

            int index = _random.NextInt(0, _remainingMonsterPool.Count);
            MonsterModel monster = _remainingMonsterPool[index];
            _remainingMonsterPool.RemoveAt(index);
            return monster;
        }

        private EventModel TakeRandomEvent()
        {
            if (_remainingEventPool.Count == 0)
                throw new InvalidOperationException($"{CurrentCardDeck.Name} has no remaining event.");

            int index = _random.NextInt(0, _remainingEventPool.Count);
            EventModel stageEvent = _remainingEventPool[index];
            _remainingEventPool.RemoveAt(index);
            return stageEvent;
        }

        private void Shuffle(List<ICardModel> cards)
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = _random.NextInt(0, i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }
    }
}
