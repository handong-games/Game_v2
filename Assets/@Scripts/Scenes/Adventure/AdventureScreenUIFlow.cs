using Domains.Adventure;
using Domains.Combat;
using Domains.Intent.Presentation;
using Domains.Player;
using Domains.View.Widgets;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure
{
    // Role:
    // Coordinates screen-level Adventure UI sequences by composing smaller UI flows.
    // It does not execute gameplay commands or call AdventureScreenController.
    public sealed class AdventureScreenUIFlow
    {
        private readonly AdventureIntroUIFlow _introUIFlow;
        private readonly AdventureCoinUIFlow _coinUIFlow;
        private readonly AdventurePlayerTurnUIFlow _playerTurnUIFlow;
        private readonly AdventureEnemyTurnUIFlow _enemyTurnUIFlow;
        private readonly AdventureIntentUIFlow _intentUIFlow;
        private readonly AdventureCombatResultUIFlow _combatResultUIFlow;
        private readonly AdventureRewardUIFlow _rewardUIFlow;
        private readonly AdventureChoiceRefreshUIFlow _choiceRefreshUIFlow;
        private readonly DamageNumberUIFlow _damageNumberUIFlow;

        public AdventureScreenUIFlow(
            AdventureIntroUIFlow introUIFlow,
            AdventureCoinUIFlow coinUIFlow,
            AdventurePlayerTurnUIFlow playerTurnUIFlow,
            AdventureEnemyTurnUIFlow enemyTurnUIFlow,
            AdventureIntentUIFlow intentUIFlow,
            AdventureCombatResultUIFlow combatResultUIFlow,
            AdventureRewardUIFlow rewardUIFlow,
            AdventureChoiceRefreshUIFlow choiceRefreshUIFlow,
            DamageNumberUIFlow damageNumberUIFlow)
        {
            _introUIFlow = introUIFlow ?? throw new ArgumentNullException(nameof(introUIFlow));
            _coinUIFlow = coinUIFlow ?? throw new ArgumentNullException(nameof(coinUIFlow));
            _playerTurnUIFlow = playerTurnUIFlow ?? throw new ArgumentNullException(nameof(playerTurnUIFlow));
            _enemyTurnUIFlow = enemyTurnUIFlow ?? throw new ArgumentNullException(nameof(enemyTurnUIFlow));
            _intentUIFlow = intentUIFlow ?? throw new ArgumentNullException(nameof(intentUIFlow));
            _combatResultUIFlow = combatResultUIFlow ?? throw new ArgumentNullException(nameof(combatResultUIFlow));
            _rewardUIFlow = rewardUIFlow ?? throw new ArgumentNullException(nameof(rewardUIFlow));
            _choiceRefreshUIFlow = choiceRefreshUIFlow ?? throw new ArgumentNullException(nameof(choiceRefreshUIFlow));
            _damageNumberUIFlow = damageNumberUIFlow ?? throw new ArgumentNullException(nameof(damageNumberUIFlow));
        }

        public void Bind(VisualElement effectLayer)
        {
            if (effectLayer == null)
                throw new ArgumentNullException(nameof(effectLayer));

            _coinUIFlow.Bind(effectLayer);
            _damageNumberUIFlow.Bind(effectLayer);
        }

        public Awaitable PlayIntro(
            VisualElement adventureRoot,
            VisualElement background,
            VisualElement emblem,
            VisualElement cardDeck,
            ResourceStatusBar resourceStatusBar,
            Banner banner,
            AdventureInitialPresentationViewModel viewModel,
            Func<bool> canContinue)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            return _introUIFlow.Play(adventureRoot, background, emblem, cardDeck, resourceStatusBar, banner, viewModel, canContinue);
        }

        public Awaitable PlayPlayerTurnStart(
            Banner banner,
            Pouch pouch,
            CoinStatusWidget coinStatus,
            EndTurnWidget endTurnWidget,
            AdventurePlayerTurnStartViewModel viewModel,
            Func<bool> canContinue)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            return PlayPlayerTurnStartedSequence(
                banner,
                pouch,
                coinStatus,
                endTurnWidget,
                viewModel,
                canContinue);
        }

        private async Awaitable PlayPlayerTurnStartedSequence(
            Banner banner,
            Pouch pouch,
            CoinStatusWidget coinStatus,
            EndTurnWidget endTurnWidget,
            AdventurePlayerTurnStartViewModel viewModel,
            Func<bool> canContinue)
        {
            _ = RunIntentRevealWithoutBlocking(viewModel, canContinue);

            await _playerTurnUIFlow.PlayPlayerTurnStart(
                banner,
                endTurnWidget,
                viewModel.Turn);
            if (!CanContinue(canContinue))
                return;

            await _playerTurnUIFlow.ShowPlayerTurnReadyWidgets(
                coinStatus,
                pouch,
                canContinue);
        }

        private async Awaitable RunIntentRevealWithoutBlocking(
            AdventurePlayerTurnStartViewModel viewModel,
            Func<bool> canContinue)
        {
            try
            {
                if (!CanContinue(canContinue))
                    return;

                await _intentUIFlow.PlayReveal(viewModel.IntentReveals, canContinue);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public async Awaitable PlayPouchClicked(
            Pouch pouch,
            CoinStatusWidget coinStatus,
            Func<bool> canContinue)
        {
            await _playerTurnUIFlow.PlayPouchClicked(pouch);
            if (!CanContinue(canContinue))
                return;

            await _coinUIFlow.ShowStatus(coinStatus);
        }

        public async Awaitable PlayCoinFlip(
            CoinFlipCueData data,
            Pouch pouch,
            CoinStatusWidget coinStatus,
            AdventureSkillSlotGroup skillSlots,
            EndTurnWidget endTurnWidget,
            Func<bool> canContinue)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            await _coinUIFlow.PlayCoinFlip(data, pouch, coinStatus);
            if (!CanContinue(canContinue))
                return;

            await _playerTurnUIFlow.PlayAfterCoinFlip(
                skillSlots,
                endTurnWidget,
                canContinue);
        }

        public Awaitable PlayCoinChange(
            CoinChangeCueData data,
            CoinStatusWidget coinStatus,
            Func<bool> canContinue)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return _coinUIFlow.PlayCoinChange(data, coinStatus, canContinue);
        }

        public Awaitable PlayEndTurnClicked(
            CoinStatusWidget coinStatus,
            EndTurnWidget endTurnWidget,
            Func<bool> canContinue)
        {
            if (coinStatus == null)
                throw new ArgumentNullException(nameof(coinStatus));

            if (endTurnWidget == null)
                throw new ArgumentNullException(nameof(endTurnWidget));

            if (!CanContinue(canContinue))
                return Awaitable.NextFrameAsync();

            return _playerTurnUIFlow.PlayEndTurnClicked(coinStatus, endTurnWidget);
        }

        public Awaitable PlayEnemyTurnStart(
            Banner banner,
            AdventureEnemyTurnStartViewModel viewModel,
            Func<bool> canContinue)
        {
            if (!CanContinue(canContinue))
                return Awaitable.NextFrameAsync();

            return _enemyTurnUIFlow.PlayStart(banner, viewModel);
        }

        public Awaitable<bool> PlayIntentTriggered(uint cardId, Func<bool> canContinue)
        {
            return _intentUIFlow.PlayTriggered(cardId, canContinue);
        }

        public Awaitable PlayIntentRefresh(
            MonsterIntentRevealViewModel viewModel,
            Func<bool> canContinue)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            return _intentUIFlow.PlayRefresh(viewModel, canContinue);
        }

        public Awaitable PlayCombatResult(
            Banner banner,
            ECombatEndResult result,
            Func<bool> canContinue)
        {
            if (!CanContinue(canContinue))
                return Awaitable.NextFrameAsync();

            return _combatResultUIFlow.Play(banner, result);
        }

        public Awaitable<AdventureRewardUIResult> PlayReward(
            VisualElement adventureRoot,
            AdventureRewardViewModel viewModel,
            Func<bool> canContinue)
        {
            if (adventureRoot == null)
                throw new ArgumentNullException(nameof(adventureRoot));

            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            return _rewardUIFlow.Play(adventureRoot, viewModel, canContinue);
        }

        public Awaitable<bool> PlayChoiceRefresh(
            VisualElement cardDeck,
            AdventureChoiceRefreshViewModel viewModel,
            Func<bool> canContinue)
        {
            if (cardDeck == null)
                throw new ArgumentNullException(nameof(cardDeck));

            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            return _choiceRefreshUIFlow.Play(cardDeck, viewModel, canContinue);
        }

        public void Unbind()
        {
            _coinUIFlow.Unbind();
            _damageNumberUIFlow.Unbind();
        }

        private static bool CanContinue(Func<bool> canContinue)
        {
            return canContinue == null || canContinue.Invoke();
        }
    }
}
