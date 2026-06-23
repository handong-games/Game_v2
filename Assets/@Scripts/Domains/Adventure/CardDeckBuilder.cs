using System;
using System.Collections.Generic;
using Game.Data;

namespace Domains.Adventure
{
    public sealed class CardDeckBuilder
    {
        public void Build(CardDeckState state, CardDeckModel cardDeck, uint seed)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            state.Initialize(cardDeck, seed);
            BuildModelPools(state);
            BuildDeck(state);
        }

        private static void BuildModelPools(CardDeckState state)
        {
            MonsterModel[] monsterPool = state.CurrentCardDeck.MonsterPool;
            if (monsterPool != null)
            {
                for (int i = 0; i < monsterPool.Length; i++)
                {
                    state.AddRemainingMonster(monsterPool[i]);
                }
            }

            EventModel[] eventPool = state.CurrentCardDeck.EventPool;
            if (eventPool != null)
            {
                for (int i = 0; i < eventPool.Length; i++)
                {
                    state.AddRemainingEvent(eventPool[i]);
                }
            }
        }

        private static void BuildDeck(CardDeckState state)
        {
            state.AddDeckCard(state.TakeFirstMonster());

            List<CardModelBase> choiceCards = BuildChoiceCards(state);
            Shuffle(state, choiceCards);

            for (int i = 0; i < choiceCards.Count; i++)
            {
                state.AddDeckCard(choiceCards[i]);
            }

            state.AddDeckCard(GetRequiredCard(state, state.CurrentCardDeck.BossChoiceCard, "boss choice card"));
        }

        private static List<CardModelBase> BuildChoiceCards(CardDeckState state)
        {
            List<CardModelBase> choiceCards = new();

            for (uint i = 0; i < state.CurrentCardDeck.MonsterCardCount; i++)
            {
                choiceCards.Add(GetRequiredCard(state, state.CurrentCardDeck.MonsterChoiceCard, "monster choice card"));
            }

            for (uint i = 0; i < state.CurrentCardDeck.EventCardCount; i++)
            {
                choiceCards.Add(GetRequiredCard(state, state.CurrentCardDeck.EventChoiceCard, "event choice card"));
            }

            for (uint i = 0; i < state.CurrentCardDeck.ShopCardCount; i++)
            {
                choiceCards.Add(GetRequiredCard(state, state.CurrentCardDeck.ShopChoiceCard, "shop choice card"));
            }

            return choiceCards;
        }

        private static CardModelBase GetRequiredCard(CardDeckState state, CardModelBase card, string label)
        {
            if (card == null)
                throw new InvalidOperationException($"{state.CurrentCardDeck.Name} requires {label}.");

            return card;
        }

        private static void Shuffle(CardDeckState state, List<CardModelBase> cards)
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = state.NextRandomIndex(0, i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }
    }
}
