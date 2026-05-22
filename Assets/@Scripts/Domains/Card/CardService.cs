using System;
using System.Collections.Generic;
using Game.Core.Managers.Dependency;
using Game.Data;
using Gameplay.GAS;

namespace Domains.Card
{
    [Dependency]
    public sealed class CardService : IDisposable
    {
        private readonly Dictionary<uint, Card> _cards = new();
        private uint _nextCardId = 1;

        public Card Create(ICardModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            Card card = new(CreateCardId(), model, ECardFace.Front);
            ApplyOwnedTags(card, model);
            model.AbilitySet?.GiveAbilities(card.AbilitySystem);
            _cards.Add(card.CardId, card);
            return card;
        }

        public void Replace(uint cardId, ICardModel model, ECardFace face)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (!_cards.TryGetValue(cardId, out Card card))
                throw new InvalidOperationException($"Card id {cardId} is not registered.");

            RemoveOwnedTags(card, card.Model);
            card.SetModel(model);
            card.SetFace(face);
            ApplyOwnedTags(card, model);
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

        public void Dispose()
        {
            Clear();
        }

        private uint CreateCardId()
        {
            return _nextCardId++;
        }

        private static void ApplyOwnedTags(Card card, ICardModel model)
        {
            IReadOnlyList<GameplayTagReference> ownedTags = model.OwnedTags;
            for (int i = 0; i < ownedTags.Count; i++)
            {
                GameplayTag tag = ownedTags[i].Tag;
                if (tag.IsValid)
                    card.AbilitySystem.OwnedTags.AddTag(tag);
            }
        }

        private static void RemoveOwnedTags(Card card, ICardModel model)
        {
            IReadOnlyList<GameplayTagReference> ownedTags = model.OwnedTags;
            for (int i = 0; i < ownedTags.Count; i++)
            {
                GameplayTag tag = ownedTags[i].Tag;
                if (tag.IsValid)
                    card.AbilitySystem.OwnedTags.RemoveTag(tag);
            }
        }

    }
}
