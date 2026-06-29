using System.Collections.Generic;
using Domains.Combat;
using Domains.Intent.Data;
using Domains.Intent.Execution;
using Domains.Intent.Flow;
using Domains.Intent.Runtime;
using Game.AbilitySystem;
using Game.Scenes.Adventure.Events.Flow;
using Gameplay.GAS;
using System;
using UnityEngine;
using CardActor = Domains.Card.Card;

namespace Domains.Adventure
{
    // Role:
    // Executes all living enemy actions for the current enemy turn.
    // It does not decide the next turn; AdventureTurnFlow does that after awaiting execution.
    public sealed class AdventureEnemyActionFlow
    {
        private readonly AdventureCards _cards;
        private readonly AdventureCombatRuntime _combat;
        private readonly AdventureCombatEvents _events;
        private readonly IntentRuntime _intentRuntime;
        private readonly ActionExecutionBindingStore _actionBindings;
        private readonly IntentConsumeFlow _intentConsumeFlow;
        private bool _isExecuting;
        private bool _executionCompleted;
        private AwaitableCompletionSource _turnCompletionSource;
        private PendingEnemyActionCompletion _currentActionCompletion;

        public AdventureEnemyActionFlow(
            AdventureCards cards,
            AdventureCombatRuntime combat,
            AdventureCombatEvents events,
            IntentRuntime intentRuntime,
            ActionExecutionBindingStore actionBindings,
            IntentConsumeFlow intentConsumeFlow)
        {
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _intentRuntime = intentRuntime ?? throw new ArgumentNullException(nameof(intentRuntime));
            _actionBindings = actionBindings ?? throw new ArgumentNullException(nameof(actionBindings));
            _intentConsumeFlow = intentConsumeFlow ?? throw new ArgumentNullException(nameof(intentConsumeFlow));
        }

        public async Awaitable ExecuteEnemyTurn()
        {
            if (_combat.IsEnded)
                return;

            if (_isExecuting)
            {
                if (_turnCompletionSource != null)
                    await _turnCompletionSource.Awaitable;

                return;
            }

            _isExecuting = true;
            _executionCompleted = false;
            _turnCompletionSource = new AwaitableCompletionSource();

            try
            {
                await ExecuteEnemyTurnCore();
            }
            finally
            {
                StopExecution();
                CompleteTurnAwaiters();
            }
        }

        private async Awaitable ExecuteEnemyTurnCore()
        {
            if (_combat.PlayerCardIds.Count == 0 ||
                !_cards.TryGet(_combat.PlayerCardIds[0], out CardActor playerCard))
            {
                MarkExecutionCompleted();
                return;
            }

            List<uint> enemyCardIds = new(_combat.EnemyCardIds);
            for (int i = 0; i < enemyCardIds.Count; i++)
            {
                if (!_cards.TryGet(enemyCardIds[i], out CardActor enemyCard))
                    continue;

                if (!IsAlive(enemyCard))
                    continue;

                if (_events.IntentTriggeredRequested == null)
                    throw new InvalidOperationException(
                        $"{nameof(AdventureCombatEvents.IntentTriggeredRequested)} is not bound.");

                bool intentTriggerPresentationCompleted =
                    await _events.IntentTriggeredRequested.Invoke(enemyCard.CardId);

                if (!intentTriggerPresentationCompleted)
                    return;

                if (_combat.IsEnded || _executionCompleted)
                {
                    return;
                }

                await ExecuteIntentAction(enemyCard, playerCard);

                if (_combat.IsEnded || _executionCompleted)
                {
                    return;
                }
            }

            MarkExecutionCompleted();
        }

        private async Awaitable ExecuteIntentAction(CardActor enemyCard, CardActor playerCard)
        {
            MonsterActionModel actionModel = GetFinalAction(enemyCard.CardId);

            if (!_actionBindings.TryGetHandle(
                    enemyCard.CardId,
                    actionModel,
                    out GameplayAbilitySpecHandle handle))
            {
                throw new InvalidOperationException(
                    $"Intent action binding is missing. Card: {enemyCard.CardId}, Action: {actionModel.name}.");
            }

            PendingEnemyActionCompletion actionCompletion = new();
            _currentActionCompletion = actionCompletion;
            bool started = enemyCard.AbilitySystem.TriggerAbilityFromGameplayEvent(
                handle,
                new GameplayEventData(AbilityGameplayTags.EventTurnStarted)
                {
                    Instigator = enemyCard.AbilitySystem,
                    Target = playerCard.AbilitySystem,
                    OptionalObject = new CombatTurnActionContext(
                        actionCompletion.Complete),
                });

            if (!started)
            {
                ClearCurrentActionCompletion(actionCompletion);
                throw new InvalidOperationException(
                    $"Failed to start enemy intent action. Card: {enemyCard.CardId}, Action: {actionModel.name}.");
            }

            await actionCompletion.Awaitable;
            ClearCurrentActionCompletion(actionCompletion);

            if (_combat.IsEnded || _executionCompleted)
                return;

            CompleteEnemyAction(enemyCard.CardId);
        }

        private MonsterActionModel GetFinalAction(uint enemyCardId)
        {
            if (!_intentRuntime.TryGet(enemyCardId, out IntentRuntimeState state))
                throw new InvalidOperationException(
                    $"Cached intent runtime state is missing. Card: {enemyCardId}.");

            if (!state.TryGetCachedResolvedAction(out ResolvedIntentActionData resolvedAction))
                throw new InvalidOperationException(
                    $"Cached resolved intent action is missing. Card: {enemyCardId}.");

            return resolvedAction.FinalActionModel ??
                   throw new InvalidOperationException(
                       $"Cached resolved intent action has null final action. Card: {enemyCardId}.");
        }

        private void CompleteEnemyAction(uint enemyCardId)
        {
            if (_combat.IsEnded || _executionCompleted)
                return;

            _intentConsumeFlow.Consume(enemyCardId);
        }

        private void MarkExecutionCompleted()
        {
            if (_executionCompleted)
                return;

            _executionCompleted = true;
        }

        private void StopExecution()
        {
            _isExecuting = false;
        }

        public void CancelExecution()
        {
            _executionCompleted = true;
            _currentActionCompletion?.Complete();
        }

        private void CompleteTurnAwaiters()
        {
            AwaitableCompletionSource completionSource = _turnCompletionSource;
            _turnCompletionSource = null;
            completionSource?.SetResult();
        }

        private void ClearCurrentActionCompletion(PendingEnemyActionCompletion actionCompletion)
        {
            if (_currentActionCompletion == actionCompletion)
                _currentActionCompletion = null;
        }

        private static bool IsAlive(CardActor card)
        {
            return card != null &&
                   !card.AbilitySystem.OwnedTags.HasTagExact(StateGameplayTags.Dead);
        }

        private sealed class PendingEnemyActionCompletion
        {
            private readonly AwaitableCompletionSource _completionSource = new();
            private bool _completed;

            public Awaitable Awaitable => _completionSource.Awaitable;

            public void Complete()
            {
                if (_completed)
                    return;

                _completed = true;
                _completionSource.SetResult();
            }
        }
    }
}
