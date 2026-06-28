using System;
using System.Collections.Generic;
using Domains.Card;
using Domains.Combat;
using Domains.Intent.Presentation;
using Domains.Player;
using UnityEngine;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private IReadOnlyList<AdventureCardViewModel> _gameplayCueReceiverCards =
            Array.Empty<AdventureCardViewModel>();

        internal async void OnTurnBannerRequested()
        {
            await PlayTurnBannerAnimation();
        }

        internal async void OnIntentRevealRequested(
            IReadOnlyList<MonsterIntentRevealViewModel> revealSequence)
        {
            await Awaitable.NextFrameAsync();
            _controller.OnIntentRevealCompleted();
        }

        internal async void OnIntentTriggeredRequested(uint cardId)
        {
            await Awaitable.NextFrameAsync();
        }

        internal async void OnEnemyTurnCompleted()
        {
            _controller.OnEnemyTurnCompleted();
            await PlayTurnBannerAnimation();
        }

        internal void OnCombatEnded(ECombatEndResult result)
        {
            switch (result)
            {
                case ECombatEndResult.Victory:
                    break;

                case ECombatEndResult.Defeat:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        internal async void OnPouchClicked()
        {
            await _coinStatusWidget.Show();
            _controller.OnPouchClicked();
        }

        public void HandleCoinFlipCue(CoinFlipCueData data)
        {
            _ = PlayCoinFlipAsync(data);
        }

        public void HandleCoinChangeCue(CoinChangeCueData data)
        {
            _ = PlayCoinChangeAsync(data);
        }

        public void HandleDamageCue(DamageCueData data)
        {
        }

        private void BindGameplayCueReceivers()
        {
            _gameplayCueReceiverCards = _controller.GetRuntimeBoardCards();

            for (int i = 0; i < _gameplayCueReceiverCards.Count; i++)
            {
                AdventureCardViewModel card = _gameplayCueReceiverCards[i];
                if (card.Zone != ECardZone.Left)
                    continue;

                card.AbilitySystem?.SetAvatar(this);
                _avatarRegistry.Register(card.CardId, this);
            }
        }

        private void UnbindGameplayCueReceivers()
        {
            for (int i = 0; i < _gameplayCueReceiverCards.Count; i++)
            {
                AdventureCardViewModel card = _gameplayCueReceiverCards[i];
                if (card.Zone != ECardZone.Left)
                    continue;

                if (ReferenceEquals(card.AbilitySystem?.GetAvatar<IAdventureGameplayCueReceiver>(), this))
                    card.AbilitySystem.ClearAvatar();
            }

            _avatarRegistry.Unregister(this);
            _gameplayCueReceiverCards = Array.Empty<AdventureCardViewModel>();
        }

        internal async void OnEndTurnClicked()
        {
            _coinStatusWidget.Hide();
            _coinStatusWidget.Reset();
            _endTurnWidget.Hide();

            _controller.OnEndTurnClicked();
            await PlayEnemyTurnBannerAnimation();
            _controller.OnEnemyTurnBannerCompleted();
        }
    }
}
