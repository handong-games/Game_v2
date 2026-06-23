using System;
using Game.Data;

namespace Domains.Card
{
    public sealed class CardService : IDisposable
    {
        private readonly CardRegistry _registry;
        private readonly CardFactory _factory;

        public CardService(CardRegistry registry, CardFactory factory)
        {
            _registry = registry;
            _factory = factory;
        }

        public Card Create(CardModelBase model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            Card card = _factory.Create(
                _registry.CreateCardId(),
                model,
                ECardFace.Front);
            _registry.Add(card);
            return card;
        }

        public void Replace(uint cardId, CardModelBase model, ECardFace face)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (!_registry.TryGet(cardId, out Card card))
                throw new InvalidOperationException($"Card id {cardId} is not registered.");

            _factory.Replace(card, model, face);
        }

        public bool TryGet(uint cardId, out Card card)
        {
            return _registry.TryGet(cardId, out card);
        }

        public void Remove(uint cardId)
        {
            _registry.Remove(cardId);
        }

        public void Clear()
        {
            _registry.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
