using System;
using System.Collections.Generic;
using Game.Scenes.Adventure.Events.Flow;
using UnityEngine;

namespace Domains.Adventure
{
    public enum AdventureStageAdvanceStatus
    {
        NoCurrentRun,
        ScreenUnavailable,
        Advanced,
        Completed
    }

    // Role:
    // Advances Adventure after one encounter is fully resolved and requests the next board presentation.
    // It removes outgoing stage cards only after the requested board exit/refresh presentation completes.
    public sealed class AdventureStageAdvanceFlow
    {
        private readonly AdventureProgress _progress;
        private readonly AdventureRunState _runState;
        private readonly AdventureStageFlow _stageFlow;
        private readonly AdventureCards _cards;
        private readonly AdventureBoard _board;
        private readonly AdventurePresenter _presenter;
        private readonly AdventureScreenEvents _screenEvents;
        private readonly AdventureBoardEvents _boardEvents;

        public AdventureStageAdvanceFlow(
            AdventureProgress progress,
            AdventureRunState runState,
            AdventureStageFlow stageFlow,
            AdventureCards cards,
            AdventureBoard board,
            AdventurePresenter presenter,
            AdventureScreenEvents screenEvents,
            AdventureBoardEvents boardEvents)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _runState = runState ?? throw new ArgumentNullException(nameof(runState));
            _stageFlow = stageFlow ?? throw new ArgumentNullException(nameof(stageFlow));
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _screenEvents = screenEvents ?? throw new ArgumentNullException(nameof(screenEvents));
            _boardEvents = boardEvents ?? throw new ArgumentNullException(nameof(boardEvents));
        }

        public async Awaitable<AdventureStageAdvanceStatus> AdvanceOrComplete(
            IReadOnlyList<uint> removeAfterExitCardIds = null)
        {
            AdventureRun run = _runState.CurrentRun;
            if (run == null)
                return AdventureStageAdvanceStatus.NoCurrentRun;

            List<uint> pendingRemovalCardIds = CreatePendingRemovalCardIds(removeAfterExitCardIds);

            if (run.StageNumber >= run.MaxStageCount)
            {
                _progress.EnterComplete();
                AddPendingRemovalCardIds(pendingRemovalCardIds, _stageFlow.TakeStageBoardCardIds());
                bool cleared = await NotifyBoardSideClearRequested(AdventureBoardSide.Right);
                RemoveCards(pendingRemovalCardIds);
                return cleared
                    ? AdventureStageAdvanceStatus.Completed
                    : AdventureStageAdvanceStatus.ScreenUnavailable;
            }

            // Board state is detached before the refresh presentation is created so
            // Presenter sees only the next stage cards. Runtime card removal remains
            // delayed until the screen finishes the exit/refresh presentation.
            AddPendingRemovalCardIds(pendingRemovalCardIds, _stageFlow.TakeStageBoardCardIds());
            run.AdvanceStage();
            _stageFlow.StartCurrentStage(removeExistingStageCards: false);

            bool choiceRefreshCompleted = await NotifyChoiceRefreshStarted();
            RemoveCards(pendingRemovalCardIds);
            if (!choiceRefreshCompleted)
                return AdventureStageAdvanceStatus.ScreenUnavailable;

            _stageFlow.OpenChoiceSelection();
            return AdventureStageAdvanceStatus.Advanced;
        }

        private async Awaitable<bool> NotifyChoiceRefreshStarted()
        {
            if (_screenEvents.ChoiceRefreshStarted == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureScreenEvents.ChoiceRefreshStarted)} is not bound.");

            return await _screenEvents.ChoiceRefreshStarted.Invoke(
                new AdventureChoiceRefreshViewModel(
                    _presenter.CreateBoardPresentation()));
        }

        private async Awaitable<bool> NotifyBoardSideClearRequested(AdventureBoardSide side)
        {
            if (_boardEvents.SideClearRequested == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureBoardEvents.SideClearRequested)} is not bound.");

            return await _boardEvents.SideClearRequested.Invoke(
                new AdventureBoardSideClearViewModel(
                    side,
                    _presenter.CreateBoardPresentation()));
        }

        private static List<uint> CreatePendingRemovalCardIds(
            IReadOnlyList<uint> cardIds)
        {
            List<uint> pendingRemovalCardIds = new();
            AddPendingRemovalCardIds(pendingRemovalCardIds, cardIds);
            return pendingRemovalCardIds;
        }

        private static void AddPendingRemovalCardIds(
            List<uint> target,
            IReadOnlyList<uint> cardIds)
        {
            if (target == null || cardIds == null)
                return;

            for (int i = 0; i < cardIds.Count; i++)
            {
                target.Add(cardIds[i]);
            }
        }

        private void RemoveCards(IReadOnlyList<uint> cardIds)
        {
            _cards.RemoveAll(cardIds);
        }
    }
}
