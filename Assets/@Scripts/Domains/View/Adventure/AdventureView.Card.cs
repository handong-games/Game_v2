using System.Collections.Generic;
using Domains.Player;
using System;
using UnityEngine;

namespace Domains.Adventure
{
    // Role:
    // Handles dynamic board card presentation and runtime card receiver binding for AdventureView.
    public sealed partial class AdventureView
    {
        private void ClearCards()
        {
            _skillUIFlow.Clear();
            _boardUIFlow.ClearAll();
        }

        private void BindBoardRuntimeConnections(
            IReadOnlyList<AdventureCardViewModel> runtimeCards)
        {
            if (runtimeCards == null)
                throw new ArgumentNullException(nameof(runtimeCards));

            BindGameplayCueReceivers(runtimeCards);
        }

        private async Awaitable<bool> ReplaceBoardPresentationAfterExit(
            AdventureBoardPresentationViewModel board,
            int screenLifetimeVersion)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            UnbindGameplayCueReceivers();
            _skillUIFlow.Clear();

            await _boardUIFlow.ReplaceBoardAfterExit(
                board.DisplayCards,
                () => IsCurrentScreenLifetime(screenLifetimeVersion));
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            BindBoardRuntimeConnections(board.RuntimeCards);
            return true;
        }

        private async Awaitable<bool> RemoveBoardCardAfterExit(
            AdventureBoardCardRemoveViewModel viewModel,
            int screenLifetimeVersion)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            UnbindGameplayCueReceivers();
            _skillUIFlow.Clear();

            bool removed = await _boardUIFlow.RemoveCardAfterExit(
                viewModel.CardId,
                () => IsCurrentScreenLifetime(screenLifetimeVersion));
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            BindBoardRuntimeConnections(viewModel.Board.RuntimeCards);
            return removed;
        }

        private async Awaitable<bool> PlayBoardCardDeathAndRemove(
            AdventureBoardCardDeathViewModel viewModel,
            int screenLifetimeVersion)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            UnbindGameplayCueReceivers();
            _skillUIFlow.Clear();

            bool removed = await _boardUIFlow.PlayCardDeathAndRemove(
                viewModel.CardId,
                () => IsCurrentScreenLifetime(screenLifetimeVersion));

            return removed && IsCurrentScreenLifetime(screenLifetimeVersion);
        }

        private async Awaitable<bool> ClearBoardSideAfterExit(
            AdventureBoardSideClearViewModel viewModel,
            int screenLifetimeVersion)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            UnbindGameplayCueReceivers();
            _skillUIFlow.Clear();

            bool cleared = await _boardUIFlow.ClearSideAfterExit(
                viewModel.Side,
                () => IsCurrentScreenLifetime(screenLifetimeVersion));
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            BindBoardRuntimeConnections(viewModel.Board.RuntimeCards);
            return cleared;
        }
    }
}
