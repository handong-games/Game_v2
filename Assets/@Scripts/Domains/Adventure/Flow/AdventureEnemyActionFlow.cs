using System.Collections.Generic;
using Domains.Combat;
using Domains.Intent.Data;
using Domains.Intent.Execution;
using Domains.Intent.Flow;
using Domains.Intent.Runtime;
using Game.AbilitySystem;
using Game.Scenes.Adventure.Events.Flow;
using Gameplay.GAS;
using CardActor = Domains.Card.Card;

namespace Domains.Adventure
{
    // Role:
    // Executes all living enemy actions for the current enemy turn.
    // It reports EnemyTurnCompleted after the enemy turn execution finishes.
    public sealed class AdventureEnemyActionFlow
    {
        private readonly AdventureCards _cards;
        private readonly AdventureCombatRuntime _combat;
        private readonly AdventureCombatEvents _events;
        private readonly IntentRuntime _intentRuntime;
        private readonly ActionExecutionBindingStore _actionBindings;
        private readonly IntentConsumeFlow _intentConsumeFlow;
        private int _pendingActions;
        private bool _completionSent;

        public AdventureEnemyActionFlow(
            AdventureCards cards,
            AdventureCombatRuntime combat,
            AdventureCombatEvents events,
            IntentRuntime intentRuntime,
            ActionExecutionBindingStore actionBindings,
            IntentConsumeFlow intentConsumeFlow)
        {
            _cards = cards;
            _combat = combat;
            _events = events;
            _intentRuntime = intentRuntime;
            _actionBindings = actionBindings;
            _intentConsumeFlow = intentConsumeFlow;
        }

        public void ExecuteEnemyTurn()
        {
            if (_combat.IsEnded)
                return;

            _completionSent = false;
            _pendingActions = 0;

            if (_combat.PlayerCardIds.Count == 0 ||
                !_cards.TryGet(_combat.PlayerCardIds[0], out CardActor playerCard))
            {
                CompleteEnemyTurn();
                return;
            }

            IReadOnlyList<uint> enemyCardIds = _combat.EnemyCardIds;
            for (int i = 0; i < enemyCardIds.Count; i++)
            {
                if (!_cards.TryGet(enemyCardIds[i], out CardActor enemyCard))
                    continue;

                if (!IsAlive(enemyCard))
                    continue;

                _pendingActions++;
                _events.IntentTriggeredRequested?.Invoke(enemyCard.CardId);
                ExecuteIntentAction(enemyCard, playerCard);
            }

            if (_pendingActions == 0)
                CompleteEnemyTurn();
        }

        private void ExecuteIntentAction(CardActor enemyCard, CardActor playerCard)
        {
            if (!TryGetFinalAction(enemyCard.CardId, out MonsterActionModel actionModel))
            {
                OnEnemyActionCompleted(enemyCard.CardId);
                return;
            }

            if (!_actionBindings.TryGetHandle(
                    enemyCard.CardId,
                    actionModel,
                    out GameplayAbilitySpecHandle handle))
            {
                OnEnemyActionCompleted(enemyCard.CardId);
                return;
            }

            bool started = enemyCard.AbilitySystem.TriggerAbilityFromGameplayEvent(
                handle,
                new GameplayEventData(AbilityGameplayTags.EventTurnStarted)
                {
                    Instigator = enemyCard.AbilitySystem,
                    Target = playerCard.AbilitySystem,
                    OptionalObject = new CombatTurnActionContext(
                        () => OnEnemyActionCompleted(enemyCard.CardId)),
                });

            if (!started)
                OnEnemyActionCompleted(enemyCard.CardId);
        }

        private bool TryGetFinalAction(uint enemyCardId, out MonsterActionModel actionModel)
        {
            actionModel = null;

            if (!_intentRuntime.TryGet(enemyCardId, out IntentRuntimeState state))
                return false;

            if (!state.TryGetCachedResolvedAction(out ResolvedIntentActionData resolvedAction))
                return false;

            actionModel = resolvedAction.FinalActionModel;
            return actionModel != null;
        }

        private void OnEnemyActionCompleted(uint enemyCardId)
        {
            if (_combat.IsEnded || _completionSent)
                return;

            _intentConsumeFlow.Consume(enemyCardId);
            _pendingActions--;
            if (_pendingActions <= 0)
                CompleteEnemyTurn();
        }

        private void CompleteEnemyTurn()
        {
            if (_completionSent)
                return;

            _completionSent = true;
            _events.EnemyTurnCompleted?.Invoke();
        }

        private static bool IsAlive(CardActor card)
        {
            return card != null &&
                   !card.AbilitySystem.OwnedTags.HasTagExact(StateGameplayTags.Dead);
        }
    }
}
