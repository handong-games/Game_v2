using Domains.Combat;
using Domains.Card;
using Game.Scenes.Adventure.Events.Flow;
using System;
using UnityEngine;

namespace Domains.Adventure
{
    // Role:
    // Starts committed Adventure encounters and performs the immediate-followup encounter check.
    // It owns the transition from choice state to concrete encounter flow.
    public sealed class AdventureEncounterStartFlow
    {
        private readonly AdventureChoiceFlow _choiceFlow;
        private readonly AdventureEncounterFlow _encounterFlow;
        private readonly AdventureTurnFlow _turnFlow;
        private readonly AdventureStageAdvanceFlow _stageAdvanceFlow;
        private readonly AdventureCards _cards;
        private readonly AdventureBoard _board;
        private readonly AdventurePresenter _presenter;
        private readonly AdventureBoardEvents _boardEvents;

        public AdventureEncounterStartFlow(
            AdventureChoiceFlow choiceFlow,
            AdventureEncounterFlow encounterFlow,
            AdventureTurnFlow turnFlow,
            AdventureStageAdvanceFlow stageAdvanceFlow,
            AdventureCards cards,
            AdventureBoard board,
            AdventurePresenter presenter,
            AdventureBoardEvents boardEvents)
        {
            _choiceFlow = choiceFlow ?? throw new ArgumentNullException(nameof(choiceFlow));
            _encounterFlow = encounterFlow ?? throw new ArgumentNullException(nameof(encounterFlow));
            _turnFlow = turnFlow ?? throw new ArgumentNullException(nameof(turnFlow));
            _stageAdvanceFlow = stageAdvanceFlow ?? throw new ArgumentNullException(nameof(stageAdvanceFlow));
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _boardEvents = boardEvents ?? throw new ArgumentNullException(nameof(boardEvents));
        }

        public async Awaitable TryStartImmediateEncounter()
        {
            if (!_choiceFlow.TryCommitImmediateEncounter(out AdventureChoiceCommitResult committed))
                return;

            await StartEncounter(committed, refreshBoard: false);
        }

        public async Awaitable StartSelectedChoice(uint cardId)
        {
            AdventureChoiceSelectionResult selection = _choiceFlow.Select(cardId);
            if (!selection.Selected)
                return;

            AdventureChoiceCommitResult committed = _choiceFlow.CommitSelection();
            await StartEncounter(committed, refreshBoard: true);
        }

        public bool CanStartSelectedChoice(uint cardId)
        {
            return _choiceFlow.CanSelect(cardId);
        }

        private async Awaitable StartEncounter(
            AdventureChoiceCommitResult committed,
            bool refreshBoard)
        {
            AdventureEncounterStartResult encounter = _encounterFlow.StartEncounter(committed);

            if (refreshBoard)
            {
                bool boardRefreshed = await NotifyBoardRefreshRequested();
                if (!boardRefreshed)
                    return;

                RemoveCommittedChoiceDiscardCards();
            }

            if (encounter.EncounterType is AdventureEncounterType.Combat or AdventureEncounterType.Boss)
            {
                await _turnFlow.StartInitialPlayerTurn();
                return;
            }

            await CompleteNonCombatEncounter(encounter);
        }

        private void RemoveCommittedChoiceDiscardCards()
        {
            _cards.RemoveAll(_board.TakeCardIds(ECardZone.Removed));
        }

        private async Awaitable CompleteNonCombatEncounter(
            AdventureEncounterStartResult encounter)
        {
            await Awaitable.NextFrameAsync();

            _encounterFlow.CompleteNonCombatEncounter(encounter);

            AdventureStageAdvanceStatus status = await _stageAdvanceFlow.AdvanceOrComplete(
                new[] { encounter.OfferCardId });
            if (status != AdventureStageAdvanceStatus.Advanced)
                return;

            await TryStartImmediateEncounter();
        }

        private async Awaitable<bool> NotifyBoardRefreshRequested()
        {
            if (_boardEvents.RefreshRequested == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureBoardEvents.RefreshRequested)} is not bound.");

            return await _boardEvents.RefreshRequested.Invoke(_presenter.CreateBoardPresentation());
        }
    }
}
