using Domains.Adventure;
using Domains.Intent.Runtime;
using Game.Data;
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
            _cards = cards;
            _runtime = runtime;
        }

        public bool Consume(uint enemyCardId)
        {
            if (!_runtime.TryGet(enemyCardId, out IntentRuntimeState state))
                return false;

            if (!_cards.TryGet(enemyCardId, out CardActor enemyCard))
                return false;

            if (enemyCard.Model is not MonsterModel monsterModel)
                return false;

            return state.Consume(monsterModel.ActionSequence.Count);
        }
    }
}
