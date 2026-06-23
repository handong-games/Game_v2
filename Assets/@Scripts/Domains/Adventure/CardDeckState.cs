using System;
using System.Collections.Generic;
using Game.Core.Utility;
using Game.Data;
using UnityRandom = Unity.Mathematics.Random;

namespace Domains.Adventure
{
    public sealed class CardDeckState
    {
        private readonly List<CardModelBase> _deck = new();
        private readonly List<MonsterModel> _remainingMonsterPool = new();
        private readonly List<EventModel> _remainingEventPool = new();

        private UnityRandom _random;
        private int _drawIndex;

        public CardDeckModel CurrentCardDeck { get; private set; }

        public void Initialize(CardDeckModel cardDeck, uint seed)
        {
            CurrentCardDeck = cardDeck ?? throw new ArgumentNullException(nameof(cardDeck));
            _random = new UnityRandom(RandomUtility.CombineSeed(seed, "CardDeck"));
            _drawIndex = 0;

            _deck.Clear();
            _remainingMonsterPool.Clear();
            _remainingEventPool.Clear();
        }

        public int GetAvailableDrawCount(uint requestedCount)
        {
            return (int)Math.Min(requestedCount, (uint)(_deck.Count - _drawIndex));
        }

        public CardModelBase DrawNext()
        {
            CardModelBase model = _deck[_drawIndex];
            _drawIndex++;
            return model;
        }

        public void AddDeckCard(CardModelBase model)
        {
            _deck.Add(model);
        }

        public void AddRemainingMonster(MonsterModel model)
        {
            _remainingMonsterPool.Add(model);
        }

        public void AddRemainingEvent(EventModel model)
        {
            _remainingEventPool.Add(model);
        }

        public MonsterModel TakeFirstMonster()
        {
            if (_remainingMonsterPool.Count == 0)
                throw new InvalidOperationException($"{CurrentCardDeck.Name} requires at least one monster.");

            MonsterModel monster = _remainingMonsterPool[0];
            _remainingMonsterPool.RemoveAt(0);
            return monster;
        }

        public MonsterModel TakeRandomMonster()
        {
            if (_remainingMonsterPool.Count == 0)
                throw new InvalidOperationException($"{CurrentCardDeck.Name} has no remaining monster.");

            int index = _random.NextInt(0, _remainingMonsterPool.Count);
            MonsterModel monster = _remainingMonsterPool[index];
            _remainingMonsterPool.RemoveAt(index);
            return monster;
        }

        public EventModel TakeRandomEvent()
        {
            if (_remainingEventPool.Count == 0)
                throw new InvalidOperationException($"{CurrentCardDeck.Name} has no remaining event.");

            int index = _random.NextInt(0, _remainingEventPool.Count);
            EventModel stageEvent = _remainingEventPool[index];
            _remainingEventPool.RemoveAt(index);
            return stageEvent;
        }

        public int NextRandomIndex(int minInclusive, int maxExclusive)
        {
            return _random.NextInt(minInclusive, maxExclusive);
        }

        public void Clear()
        {
            CurrentCardDeck = null;
            _deck.Clear();
            _remainingMonsterPool.Clear();
            _remainingEventPool.Clear();
            _drawIndex = 0;
        }
    }
}
