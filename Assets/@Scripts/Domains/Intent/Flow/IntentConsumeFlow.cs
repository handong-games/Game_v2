using Domains.Adventure;
using Domains.Intent.Runtime;
using Game.Data;
using System;
using CardActor = Domains.Card.Card;

namespace Domains.Intent.Flow
{
    // Role:
    // Consumes an executed intent after the enemy action has finished and advances sequence state.
    public sealed class IntentConsumeFlow
    {
        private readonly AdventureCards _cards;
        private readonly IntentRuntime _runtime;

        public IntentConsumeFlow(
            AdventureCards cards,
            IntentRuntime runtime)
        {
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public bool Consume(uint enemyCardId)
        {
            if (!_runtime.TryGet(enemyCardId, out IntentRuntimeState state))
                throw new InvalidOperationException(
                    $"Cannot consume intent because runtime state is missing. Card: {enemyCardId}.");

            if (!_cards.TryGet(enemyCardId, out CardActor enemyCard))
                throw new InvalidOperationException(
                    $"Cannot consume intent because enemy card is missing. Card: {enemyCardId}.");

            if (enemyCard.Model is not MonsterModel monsterModel)
                throw new InvalidOperationException(
                    $"Cannot consume intent because card model is not MonsterModel. Card: {enemyCardId}.");

            if (!state.HasCachedResolvedAction)
                throw new InvalidOperationException(
                    $"Cannot consume intent because cached resolved action is missing. Card: {enemyCardId}.");

            return state.Consume(monsterModel.ActionSequence.Count);
        }
    }
}
