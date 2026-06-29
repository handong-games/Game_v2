using System;
using Domains.Card;
using Domains.Combat;
using Game.Scenes.Adventure.Events.Flow;
using UnityEngine;

namespace Domains.Adventure
{
    // Role:
    // Resolves an enemy death into board death presentation, card removal, and victory progression.
    // It owns enemy death sequencing; it does not subscribe to gameplay messages.
    public sealed class AdventureEnemyDeathFlow
    {
        private readonly AdventureCombatRuntime _combat;
        private readonly AdventureCards _cards;
        private readonly AdventureBoard _board;
        private readonly AdventureBoardEvents _boardEvents;
        private readonly AdventureCombatResultFlow _resultFlow;

        public AdventureEnemyDeathFlow(
            AdventureCombatRuntime combat,
            AdventureCards cards,
            AdventureBoard board,
            AdventureBoardEvents boardEvents,
            AdventureCombatResultFlow resultFlow)
        {
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _boardEvents = boardEvents ?? throw new ArgumentNullException(nameof(boardEvents));
            _resultFlow = resultFlow ?? throw new ArgumentNullException(nameof(resultFlow));
        }

        public async Awaitable HandleEnemyDeath(uint cardId)
        {
            if (_combat.IsEnded)
                return;

            if (!_combat.TryGetSide(cardId, out ECombatSide side) || side != ECombatSide.Enemy)
                return;

            if (!_combat.MarkDeathResolved(cardId))
                return;

            _combat.RemoveCard(cardId);

            bool deathPresentationCompleted = await NotifyCardDeathRequested(cardId);
            if (!deathPresentationCompleted || _combat.IsEnded)
                return;

            _board.RemoveCard(ECardZone.Right, cardId);
            _cards.Remove(cardId);

            if (_combat.EnemyCardIds.Count == 0)
                await _resultFlow.CompleteCombat(ECombatEndResult.Victory);
        }

        private async Awaitable<bool> NotifyCardDeathRequested(uint cardId)
        {
            if (_boardEvents.CardDeathRequested == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureBoardEvents.CardDeathRequested)} is not bound.");

            return await _boardEvents.CardDeathRequested.Invoke(
                new AdventureBoardCardDeathViewModel(cardId));
        }
    }
}
