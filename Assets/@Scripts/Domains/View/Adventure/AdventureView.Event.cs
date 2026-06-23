using System;
using Domains.Combat;
using Domains.Player;
using UnityEngine;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        internal async void OnTurnBannerRequested()
        {
            await PlayTurnBannerAnimation();
        }

        internal async void OnEnemyTurnBannerRequested()
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
            if (_initialViewModel?.BoardCards == null)
                return;

            for (int i = 0; i < _initialViewModel.BoardCards.Count; i++)
            {
                _initialViewModel.BoardCards[i].AbilitySystem?.SetAvatar(this);
            }
        }

        private void UnbindGameplayCueReceivers()
        {
            if (_initialViewModel?.BoardCards == null)
                return;

            for (int i = 0; i < _initialViewModel.BoardCards.Count; i++)
            {
                var abilitySystem = _initialViewModel.BoardCards[i].AbilitySystem;
                if (ReferenceEquals(abilitySystem?.GetAvatar<IAdventureGameplayCueReceiver>(), this))
                {
                    abilitySystem.ClearAvatar();
                }
            }
        }

        internal async void OnEndTurnClicked()
        {
            _coinStatusWidget.Hide();
            _coinStatusWidget.Reset();
            _endTurnWidget.Hide();

            await PlayEnemyTurnBannerAnimation();

            _controller.OnEndTurnClicked();
        }
    }
}
