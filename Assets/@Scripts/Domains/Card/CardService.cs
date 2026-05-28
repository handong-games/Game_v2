using System;
using System.Collections.Generic;
using Game.AbilitySystem.Attributes;
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
            CreateAndApplyAttributeSets(card, model.AttributeSetDefaults);

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

        private static void CreateAndApplyAttributeSets(
            Card card,
            IReadOnlyList<AttributeSetDefaultsDefinition> definitions)
        {
            if (card == null || definitions == null)
                return;

            HashSet<Type> createdSetTypes = new();

            for (int i = 0; i < definitions.Count; i++)
            {
                AttributeSetDefaultsDefinition definition = definitions[i];
                if (definition == null)
                    continue;

                Type setType = definition.GetAttributeSetType();
                if (setType == null)
                    continue;

                if (!typeof(AttributeSet).IsAssignableFrom(setType))
                    continue;

                if (!createdSetTypes.Add(setType))
                    continue;

                AttributeSet attributeSet = Activator.CreateInstance(setType) as AttributeSet;
                if (attributeSet == null)
                    continue;

                definition.ApplyTo(attributeSet);
                card.AbilitySystem.AddAttributeSet(attributeSet);
            }
        }

        private static void ApplyOwnedTags(Card card, ICardModel model)
        {
            IReadOnlyList<GameplayTag> ownedTags = model.OwnedTags;
            for (int i = 0; i < ownedTags.Count; i++)
            {
                GameplayTag tag = ownedTags[i];
                if (tag.IsValid)
                    card.AbilitySystem.OwnedTags.AddTag(tag);
            }
        }

        private static void RemoveOwnedTags(Card card, ICardModel model)
        {
            IReadOnlyList<GameplayTag> ownedTags = model.OwnedTags;
            for (int i = 0; i < ownedTags.Count; i++)
            {
                GameplayTag tag = ownedTags[i];
                if (tag.IsValid)
                    card.AbilitySystem.OwnedTags.RemoveTag(tag);
            }
        }

    }
}
