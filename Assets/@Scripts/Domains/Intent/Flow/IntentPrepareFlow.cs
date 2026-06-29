using System.Collections.Generic;
using System;
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
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _displayBuilder = displayBuilder ?? throw new ArgumentNullException(nameof(displayBuilder));
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
                throw new InvalidOperationException(
                    $"Cannot prepare intent because enemy card is missing. Card: {enemyCardId}.");

            if (!IsAlive(enemyCard))
                return false;

            IntentRuntimeState state = _runtime.GetOrCreate(enemyCardId);
            ResolvedIntentActionData resolvedAction = _resolver.Resolve(enemyCard, state);

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
