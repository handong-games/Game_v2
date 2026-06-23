using System;
using System.Collections.Generic;
using Game.AbilitySystem.Attributes;
using Game.Data;
using Gameplay.GAS;

namespace Domains.Card
{
    public sealed class CardFactory
    {
        public Card Create(uint cardId, CardModelBase model, ECardFace face)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            Card card = new(cardId, model, face);
            ApplyOwnedTags(card, model);
            CreateAndApplyAttributeSets(card, model.AttributeSetDefaults);
            model.AbilitySet?.GiveAbilities(card.AbilitySystem);
            return card;
        }

        public void Replace(Card card, CardModelBase model, ECardFace face)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            RemoveOwnedTags(card, card.Model);
            card.SetModel(model);
            card.SetFace(face);
            ApplyOwnedTags(card, model);
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

        private static void ApplyOwnedTags(Card card, CardModelBase model)
        {
            IReadOnlyList<GameplayTag> ownedTags = model.OwnedTags;
            for (int i = 0; i < ownedTags.Count; i++)
            {
                GameplayTag tag = ownedTags[i];
                if (tag.IsValid)
                    card.AbilitySystem.OwnedTags.AddTag(tag);
            }
        }

        private static void RemoveOwnedTags(Card card, CardModelBase model)
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
