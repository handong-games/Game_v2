using System;
using Domains.Combat;
using Domains.Event;
using Domains.Player;
using UnityEngine;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private void RegisterEvents()
        {
            AdventureEvents.AdventureStarted += OnAdventureStarted;
            AdventureEvents.CardsDrawn += OnCardsDrawn;
            AdventureEvents.BoardChanged += OnBoardChanged;
            AdventureEvents.TurnBannerRequested += OnTurnBannerRequested;
            AdventureEvents.EnemyTurnBannerRequested += OnEnemyTurnBannerRequested;
            AdventureEvents.CombatEnded += OnCombatEnded;
            _pouch.Clicked += OnPouchClicked;
            _endTurnWidget.Clicked += OnEndTurnClicked;
        }

        private void UnregisterEvents()
        {
            AdventureEvents.AdventureStarted -= OnAdventureStarted;
            AdventureEvents.CardsDrawn -= OnCardsDrawn;
            AdventureEvents.BoardChanged -= OnBoardChanged;
            AdventureEvents.TurnBannerRequested -= OnTurnBannerRequested;
            AdventureEvents.EnemyTurnBannerRequested -= OnEnemyTurnBannerRequested;
            AdventureEvents.CombatEnded -= OnCombatEnded;
            _pouch.Clicked -= OnPouchClicked;
            _endTurnWidget.Clicked -= OnEndTurnClicked;
        }

        private void OnAdventureStarted()
        {
            _ = PlayIntroAnimation();
        }

        private async void OnTurnBannerRequested()
        {
            await PlayTurnBannerAnimation();
        }

        private async void OnEnemyTurnBannerRequested()
        {
            _controller.OnEnemyTurnCompleted();
            await PlayTurnBannerAnimation();
        }

        private void OnCombatEnded(ECombatEndResult result)
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

        private async void OnPouchClicked()
        {
            await _coinStatusWidget.Show();
            _controller.OnPouchClicked();
        }

        private async Awaitable PlayCoinFlipAsync(CoinFlipCueData data)
        {
            if (data == null)
                return;

            await _coinEffectPlayer.Play(
                data,
                _pouch,
                _coinStatusWidget.GetTarget(ECoinFace.Heads),
                _coinStatusWidget.GetTarget(ECoinFace.Tails),
                _coinStatusWidget.Add);

            await ShowSkillSlots();
            await _endTurnWidget.Show();
        }

        private async Awaitable PlayCoinChangeAsync(CoinChangeCueData data)
        {
            if (data == null || !data.HasEntries)
                return;

            await _coinStatusWidget.Show();
            await _coinChangeEffectPlayer.Play(
                data,
                (face, delta) => _coinStatusWidget.ApplyDelta(face, delta));
        }

        private async void OnEndTurnClicked()
        {
            _coinStatusWidget.Hide();
            _coinStatusWidget.Reset();
            _endTurnWidget.Hide();

            await PlayEnemyTurnBannerAnimation();

            _controller.OnEndTurnClicked();
        }
    }
}
