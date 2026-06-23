using System;
using System.Collections.Generic;

namespace Domains.Card
{
    public sealed class CardRegistry
    {
        private readonly Dictionary<uint, Card> _cards = new();
        private uint _nextCardId = 1;

        public uint CreateCardId()
        {
            return _nextCardId++;
        }

        public void Add(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            _cards.Add(card.CardId, card);
        }

        public bool TryGet(uint cardId, out Card card)
        {
            return _cards.TryGetValue(cardId, out card);
        }

        public void Remove(uint cardId)
        {
            _cards.Remove(cardId);
        }

        public void Clear()
        {
            _cards.Clear();
            _nextCardId = 1;
        }
    }
}
