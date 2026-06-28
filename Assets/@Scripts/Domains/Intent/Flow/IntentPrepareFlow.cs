using System.Collections.Generic;
using Domains.Adventure;
using Domains.Combat;
using Domains.Intent.Runtime;
using Game.AbilitySystem;
using CardActor = Domains.Card.Card;

namespace Domains.Intent.Flow
{
    // Role:
    // Prepares visible intent contracts for living enemy cards at player turn start.
    public sealed class IntentPrepareFlow
    {
        private readonly AdventureCards _cards;
        private readonly AdventureCombatRuntime _combat;
        private readonly IntentRuntime _runtime;
        private readonly IntentActionResolver _resolver;
        private readonly IntentDisplayBuilder _displayBuilder;

        public IntentPrepareFlow(
            AdventureCards cards,
            AdventureCombatRuntime combat,
            IntentRuntime runtime,
            IntentActionResolver resolver,
            IntentDisplayBuilder displayBuilder)
        {
            _cards = cards;
            _combat = combat;
            _runtime = runtime;
            _resolver = resolver;
            _displayBuilder = displayBuilder;
        }

        public void PrepareAll()
        {
            IReadOnlyList<uint> enemyCardIds = _combat.EnemyCardIds;
            for (int i = 0; i < enemyCardIds.Count; i++)
            {
                Prepare(enemyCardIds[i]);
            }
        }

        public bool Prepare(uint enemyCardId)
        {
            if (!_cards.TryGet(enemyCardId, out CardActor enemyCard))
                return false;

            if (!IsAlive(enemyCard))
                return false;

            IntentRuntimeState state = _runtime.GetOrCreate(enemyCardId);
            if (!_resolver.TryResolve(enemyCard, state, out ResolvedIntentActionData resolvedAction))
                return false;

            state.SetCache(
                resolvedAction,
                _displayBuilder.Build(resolvedAction.FinalActionModel));
            return true;
        }

        private static bool IsAlive(CardActor card)
        {
            return card != null &&
                   !card.AbilitySystem.OwnedTags.HasTagExact(StateGameplayTags.Dead);
        }
    }
}
