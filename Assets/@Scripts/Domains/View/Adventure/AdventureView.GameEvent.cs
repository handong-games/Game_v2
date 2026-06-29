using Domains.Combat;
using Domains.Intent.Presentation;
using System;
using UnityEngine;

namespace Domains.Adventure
{
    // Role:
    // Receives game-flow presentation requests and delegates them to screen UI flows.
    public sealed partial class AdventureView
    {
        internal async Awaitable<bool> OnGameInitialPresentationPrepared(
            AdventureInitialPresentationViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            int screenLifetimeVersion = _screenLifetimeVersion;

            if (!await WaitUntilScreenReady(screenLifetimeVersion))
                return false;

            PrepareInitialPresentation(viewModel);
            return await PlayIntroOnce(screenLifetimeVersion, viewModel);
        }

        internal async Awaitable<bool> OnGamePlayerTurnStarted(AdventurePlayerTurnStartViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            await _screenUIFlow.PlayPlayerTurnStart(
                _screenWidgets.Banner,
                _screenWidgets.Pouch,
                _screenWidgets.CoinStatus,
                _screenWidgets.EndTurn,
                viewModel,
                () => IsCurrentScreenLifetime(screenLifetimeVersion));

            return IsCurrentScreenLifetime(screenLifetimeVersion);
        }

        internal async Awaitable<bool> OnGameEnemyTurnStarted(AdventureEnemyTurnStartViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            await _screenUIFlow.PlayEnemyTurnStart(
                _screenWidgets.Banner,
                viewModel,
                () => IsCurrentScreenLifetime(screenLifetimeVersion));

            return IsCurrentScreenLifetime(screenLifetimeVersion);
        }

        internal async Awaitable<bool> OnGameIntentTriggeredRequested(uint cardId)
        {
            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            bool completed = await _screenUIFlow.PlayIntentTriggered(
                cardId,
                () => IsCurrentScreenLifetime(screenLifetimeVersion));

            return completed && IsCurrentScreenLifetime(screenLifetimeVersion);
        }

        internal async Awaitable OnGameIntentRefreshRequested(
            MonsterIntentRevealViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return;

            await _screenUIFlow.PlayIntentRefresh(
                viewModel,
                () => IsCurrentScreenLifetime(screenLifetimeVersion));
        }

        internal async Awaitable<bool> OnGameCombatEnded(ECombatEndResult result)
        {
            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            await _screenUIFlow.PlayCombatResult(
                _screenWidgets.Banner,
                result,
                () => IsCurrentScreenLifetime(screenLifetimeVersion));

            return IsCurrentScreenLifetime(screenLifetimeVersion);
        }

        internal async Awaitable<AdventureRewardUIResult> OnGameRewardStarted(
            AdventureRewardViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return AdventureRewardUIResult.Canceled();

            AdventureRewardUIResult result =
                await _screenUIFlow.PlayReward(
                    _screenWidgets.AdventureRoot,
                    viewModel,
                    () => IsCurrentScreenLifetime(screenLifetimeVersion));

            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return AdventureRewardUIResult.Canceled();

            return result;
        }

        internal async Awaitable<bool> OnGameBoardRefreshRequested(
            AdventureBoardPresentationViewModel board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            return await ReplaceBoardPresentationAfterExit(board, screenLifetimeVersion);
        }

        internal async Awaitable<bool> OnGameBoardCardRemoveRequested(
            AdventureBoardCardRemoveViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            return await RemoveBoardCardAfterExit(viewModel, screenLifetimeVersion);
        }

        internal async Awaitable<bool> OnGameBoardCardDeathRequested(
            AdventureBoardCardDeathViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            return await PlayBoardCardDeathAndRemove(viewModel, screenLifetimeVersion);
        }

        internal async Awaitable<bool> OnGameBoardSideClearRequested(
            AdventureBoardSideClearViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            return await ClearBoardSideAfterExit(viewModel, screenLifetimeVersion);
        }

        internal async Awaitable<bool> OnGameChoiceRefreshStarted(
            AdventureChoiceRefreshViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (viewModel.Board == null)
                throw new InvalidOperationException("Choice refresh board presentation is missing.");

            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            UnbindGameplayCueReceivers();
            _skillUIFlow.Clear();

            bool completed = await _screenUIFlow.PlayChoiceRefresh(
                _screenWidgets.CardDeck,
                viewModel,
                () => IsCurrentScreenLifetime(screenLifetimeVersion));

            if (!completed || !IsCurrentScreenLifetime(screenLifetimeVersion))
                return false;

            BindBoardRuntimeConnections(viewModel.Board.RuntimeCards);
            return true;
        }
    }
}
