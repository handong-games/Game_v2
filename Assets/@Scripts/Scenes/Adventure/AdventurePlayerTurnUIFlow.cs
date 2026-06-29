using System;
using Domains.Adventure;
using Domains.Combat;
using Domains.View.Widgets;
using Game.Core.Managers.View;
using UnityEngine;

namespace Game.Scenes.Adventure
{
    // Role:
    // Owns player-turn presentation phase transitions.
    // It does not execute gameplay commands or mutate Adventure runtime state.
    public sealed class AdventurePlayerTurnUIFlow
    {
        private readonly ViewTransitionManager _viewTransitionManager;
        private readonly AdventureBoardUIFlow _boardUIFlow;

        public AdventurePlayerTurnUIFlow(
            ViewTransitionManager viewTransitionManager,
            AdventureBoardUIFlow boardUIFlow)
        {
            _viewTransitionManager = viewTransitionManager ?? throw new ArgumentNullException(nameof(viewTransitionManager));
            _boardUIFlow = boardUIFlow ?? throw new ArgumentNullException(nameof(boardUIFlow));
        }

        public async Awaitable PlayPlayerTurnStart(
            Banner banner,
            EndTurnWidget endTurnWidget,
            CombatTurnViewModel turn)
        {
            if (banner == null)
                throw new ArgumentNullException(nameof(banner));

            if (endTurnWidget == null)
                throw new ArgumentNullException(nameof(endTurnWidget));

            endTurnWidget.Hide();
            Awaitable bannerTransition = banner.PresentPlayerTurn(turn.TurnNumber, _viewTransitionManager);
            Awaitable healthBarTransition = _boardUIFlow.ShowHealthBars(_viewTransitionManager);

            await bannerTransition;
            await healthBarTransition;
        }

        public async Awaitable ShowPlayerTurnReadyWidgets(
            CoinStatusWidget coinStatus,
            Pouch pouch,
            Func<bool> canContinue)
        {
            if (coinStatus == null)
                throw new ArgumentNullException(nameof(coinStatus));

            if (pouch == null)
                throw new ArgumentNullException(nameof(pouch));

            await coinStatus.Show(_viewTransitionManager);

            if (canContinue != null && !canContinue.Invoke())
                return;

            await pouch.Show(_viewTransitionManager);
        }

        public async Awaitable PlayAfterCoinFlip(
            AdventureSkillSlotGroup skillSlots,
            EndTurnWidget endTurnWidget,
            Func<bool> canContinue)
        {
            if (skillSlots == null)
                throw new ArgumentNullException(nameof(skillSlots));

            if (endTurnWidget == null)
                throw new ArgumentNullException(nameof(endTurnWidget));

            await skillSlots.Show(_viewTransitionManager);

            if (canContinue != null && !canContinue.Invoke())
                return;

            await endTurnWidget.Show(_viewTransitionManager);
        }

        public Awaitable PlayPouchClicked(Pouch pouch)
        {
            if (pouch == null)
                throw new ArgumentNullException(nameof(pouch));

            return pouch.Hide(_viewTransitionManager);
        }

        public Awaitable PlayEndTurnClicked(
            CoinStatusWidget coinStatus,
            EndTurnWidget endTurnWidget)
        {
            if (coinStatus == null)
                throw new ArgumentNullException(nameof(coinStatus));

            if (endTurnWidget == null)
                throw new ArgumentNullException(nameof(endTurnWidget));

            coinStatus.Hide();
            coinStatus.Reset();
            endTurnWidget.Hide();
            return Awaitable.NextFrameAsync();
        }
    }
}
